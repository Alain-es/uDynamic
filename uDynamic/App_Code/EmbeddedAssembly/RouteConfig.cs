using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Umbraco.Core.Logging;

namespace uDynamic.EmbeddedAssembly
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {

            const string pluginBasePath = "App_Plugins/uDynamic";
            string url = string.Empty;

            RouteTable.Routes.MapRoute(
                name: "uDynamic.GetResourcePath0",
                url: pluginBasePath + "/{resource}",
                defaults: new
                {
                    controller = "EmbeddedResource",
                    action = "GetResourcePath0"
                },
                namespaces: new[] { "uDynamic.EmbeddedAssembly" }
            );

            RouteTable.Routes.MapRoute(
                name: "uDynamic.GetResourcePath1",
                url: pluginBasePath + "/{directory1}/{resource}",
                defaults: new
                {
                    controller = "EmbeddedResource",
                    action = "GetResourcePath1"
                },
                namespaces: new[] { "uDynamic.EmbeddedAssembly" }
            );

            RouteTable.Routes.MapRoute(
                name: "uDynamic.GetResourcePath2",
                url: pluginBasePath + "/{directory1}/{directory2}/{resource}",
                defaults: new
                {
                    controller = "EmbeddedResource",
                    action = "GetResourcePath2"
                },
                namespaces: new[] { "uDynamic.EmbeddedAssembly" }
            );

        }
    }
}