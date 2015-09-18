using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Funq;
using ServiceStack;
using IrrigationController.ServiceInterface;
using ServiceStack.Caching;

namespace IrrigationController.Web
{
    public class AppHost
        : AppHostBase
    {
        public AppHost() : base("Irrigation Controller Service",
            typeof(SSEService).Assembly,
            typeof(SensorService).Assembly)
        { }

        public override void Configure(Container container)
        {
            Plugins.Add(new ServerEventsFeature()
            {
                HeartbeatInterval = new TimeSpan(0, 1, 0)
            });

            container.Register<ICacheClient>(new MemoryCacheClient());

            ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;
            ServiceStack.Text.JsConfig.DateHandler = ServiceStack.Text.DateHandler.ISO8601;

            HostConfig hostConfig = new HostConfig()
            {
                ReturnsInnerException = true
            };
            SetConfig(hostConfig);

        }
    }

    [Route("/clients")]
    public class ClientsRequest { }

    public class SSEService : Service
    {
        public object Get(ClientsRequest request)
        {
            var sse = this.TryResolve<IServerEvents>() as MemoryServerEvents;
            var x = sse.ChannelSubcriptions.Count;
            return x;
        }
    }
}