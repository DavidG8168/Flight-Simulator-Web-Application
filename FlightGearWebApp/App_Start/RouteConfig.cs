using System.Web.Mvc;
using System.Web.Routing;

namespace FlightGearWebApp {
    // Configurate the URL routes for the program.
    public class RouteConfig {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            // Save.
            routes.MapRoute("save", "save/{serverIP}/{serverPort}/{timer}/{stopTimer}/{path}",
            defaults: new { controller = "Flight", action = "save" });
            // Display.
            routes.MapRoute("display", "display/{serverIP}/{serverPort}",
            defaults: new { controller = "Flight", action = "display" , time = UrlParameter.Optional });
            // Display Route.
            routes.MapRoute("displayRoute", "display/{serverIP}/{serverPort}/{timer}",
            defaults: new { controller = "Flight", action = "displayRoute", time = UrlParameter.Optional });
            // Default Index page.
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Flight", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
