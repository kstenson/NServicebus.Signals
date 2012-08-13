using System.Web.Mvc;

namespace NServicebus.Signals.Web.Areas.temp
{
    public class tempAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "temp";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "temp_default", 
                "temp/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
