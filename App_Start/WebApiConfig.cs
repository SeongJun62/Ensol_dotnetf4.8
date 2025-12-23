using dotnetfsample.Converters;
using dotnetfsample.MiddleWare;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace dotnetfsample
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API 구성 및 서비스

            // Web API 경로
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.ContractResolver = new DefaultContractResolver();
            json.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include; ;
            json.SerializerSettings.DateParseHandling = Newtonsoft.Json.DateParseHandling.None;


            config.MessageHandlers.Add(new LoggingHandler());
        }
    }
}
