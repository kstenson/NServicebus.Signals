using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NServiceBus;
using NServiceBus.MessageInterfaces.MessageMapper.Reflection;
using NServiceBus.Serializers.XML;
using NServiceBus.Unicast;
using NServiceBus.Unicast.Transport;
using SignalR.Client.Hubs;
using log4net;

namespace Signal
{
    public class Monitor : IWantToRunAtStartup
    {
        private readonly string url;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IHubProxy _monitor;
        private HubConnection _hubConnection;
        public ITransport Transport { get; set; }
        public UnicastBus bus { get; set; }

        public Monitor()
        {
            try
            {
                url = ConfigurationManager.ConnectionStrings["SignalsURL"].ConnectionString;
                AppDomain.CurrentDomain.ProcessExit += (sender, args) => Stop();
                Console.CancelKeyPress += (sender, args) => Stop();

            }
            catch (NullReferenceException)
            {
                Logger.ErrorFormat("Health Monitor Connectionstring missing from config. Please add a connection string with the key [HealthMonitorURL]");
            }
        }

        public void Run()
        {

            try
            {
                _hubConnection = new HubConnection(url) { ConnectionId = bus.InputAddress.Queue };
                _monitor = _hubConnection.CreateProxy("MonitorHub");
                _hubConnection.Start().Wait();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Could not connect to hub at {0}, events from this service will not be monitored", url);
                Logger.Error(ex);
                return;
            }

            _monitor.On("Status", () => _monitor.Invoke("StatusUpdate", new { Service = bus.InputAddress.Queue }));
            _monitor.Invoke("Register", new { Server = bus.InputAddress.Machine, Endpoint = bus.InputAddress.Queue, Started = DateTime.UtcNow, Status = "Idle" });

            Transport.TransportMessageReceived += OnTransportMessageReceived;
            Transport.FailedMessageProcessing += TransportOnFailedMessageProcessing;
            Transport.FinishedMessageProcessing += TransportOnFinishedMessageProcessing;

        }

        public void Stop()
        {
            //for some reason when the process is stoping all the private instances in the object are missing
            //need to create specificaly for shutdown
            _hubConnection = new HubConnection(url);
            _hubConnection.ConnectionId = bus.InputAddress.Queue;
            _monitor = _hubConnection.CreateProxy("MonitorHub");
            _hubConnection.Start().Wait();
            _monitor.Invoke("Deregister", new { Server = bus.InputAddress.Machine, Endpoint = bus.InputAddress.Queue });

            _hubConnection.Stop();
        }


        private void TransportOnFinishedMessageProcessing(object sender, EventArgs e)
        {
            dynamic reponse = new ExpandoObject();
            reponse.Queue = bus.InputAddress.Queue;
            reponse.Status = "Complete";
            _monitor.Invoke("MessageComplete", reponse);
        }

        private void TransportOnFailedMessageProcessing(object sender, FailedMessageProcessingEventArgs e)
        {
            dynamic response = new ExpandoObject();
            response.Queue = bus.InputAddress.Queue;

            response.Reason = e.Reason;
            _monitor.Invoke("MessageError", response);

        }

        private void OnTransportMessageReceived(object sender, TransportMessageReceivedEventArgs e)
        {
            dynamic response = new ExpandoObject();

            XmlMessageSerializer serializer = new XmlMessageSerializer(new MessageMapper());
            response.Endpoint = bus.InputAddress.Queue;
            response.Time = DateTime.UtcNow;
            response.type = serializer.Deserialize(new MemoryStream(e.Message.Body)).First().GetType().ToString();
            response.body = serializer.Deserialize(new MemoryStream(e.Message.Body)).First();
            response.headers = e.Message.Headers;


            _monitor.Invoke("MessageReceived", response);


        }


    }
}
