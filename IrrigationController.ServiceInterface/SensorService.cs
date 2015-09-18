using IrrigationController.Model;
using IrrigationController.Model.Types;
using ServiceStack;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrrigationController.ServiceInterface
{
    public class SensorService : Service
    {
        public IServerEvents ServerEvents { get; set; }

        const int READINGS_TO_KEEP = 24 * 60 * 6;
        const ushort TURN_ON_LEVEL = 200;
        const ushort TURN_OFF_LEVEL = 800;

        static Dictionary<string, Queue<SensorReading>> readings = new Dictionary<string, Queue<SensorReading>>();
        static Dictionary<string, bool?> zoneRelays = new Dictionary<string, bool?>();

        public object Get(GetSensorReadingsRequest request)
        {
            return readings.ToList();
        }

        public object Post(SensorReading request)
        {
            // Make sure we have a sensor reading queue for the sensor address
            if (!readings.ContainsKey(request.SensorAddress))
                readings.Add(request.SensorAddress, new Queue<SensorReading>());

            // Add the sensor reading
            readings[request.SensorAddress].Enqueue(request);

            // If our queue is full of readings, pop the last one off the queue
            if (readings[request.SensorAddress].Count > READINGS_TO_KEEP)
                readings[request.SensorAddress].Dequeue();

            if (!zoneRelays.ContainsKey(request.SensorAddress))
                zoneRelays.Add(request.SensorAddress, false);

            zoneRelays[request.SensorAddress] = request.RelayOn;

            // Notify any listeners of our sensor reading
            ServerEvents.NotifyChannel("sensor", request);


            // Now, based on the sensor reading, turn the sprinklers on or off
            // This is very basic logic, to demonstrate the capabilities
            if (zoneRelays.ContainsKey(request.SensorAddress))
            {
                if (request.Reading > TURN_OFF_LEVEL && zoneRelays[request.SensorAddress] == true)
                    ServerEvents.NotifyChannel("relay", new ZoneControl() { RelayOn = true });
                else if (request.Reading < TURN_ON_LEVEL && zoneRelays[request.SensorAddress] == false)
                    ServerEvents.NotifyChannel("relay", new ZoneControl() { RelayOn = false });
            }

            return null;
        }
    }
}
