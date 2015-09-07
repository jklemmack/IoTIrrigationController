using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Funq;
using ServiceStack;
using IrrigationController.ServiceInterface;

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
            });

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