using System.Web.Mvc;
using MvcContrib.PortableAreas;

namespace NServicebus.Signals.PortableArea.Areas.Signals
{
    public class SignalsAreaRegistration : PortableAreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Signals";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context, IApplicationBus bus)
        {
            RegisterRoutes(context);
            RegisterAreaEmbeddedResources();
        }

        private void RegisterRoutes(AreaRegistrationContext context)
        {
            context.MapRoute(
                AreaName + "_scripts",
                base.AreaRoutePrefix + "/Scripts/{resourceName}",
                new { controller = "EmbeddedResource", action = "Index", resourcePath = "scripts" },
                new[] { "NServicebus.Signals.PortableArea" }
            );

            context.MapRoute(
                AreaName + "_images",
                base.AreaRoutePrefix + "/images/{resourceName}",
                new { controller = "EmbeddedResource", action = "Index", resourcePath = "images" },
                new[] { "NServicebus.Signals.PortableArea" }
            );

            context.MapRoute(
                AreaName + "_default",
                base.AreaRoutePrefix + "/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "NServicebus.Signals.PortableArea.Areas.Signals.Controllers", "MvcContrib" }
            );
        }
    }
}
