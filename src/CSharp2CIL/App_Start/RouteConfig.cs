using System.Web.Mvc;
using System.Web.Routing;

namespace CSharp2CIL
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
	        routes.MapRoute("Default", "{controller}/{action}", new {controller = "Home", action = "Index"});
        }
    }
}