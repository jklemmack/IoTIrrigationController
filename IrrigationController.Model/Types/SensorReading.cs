using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrrigationController.Model.Types
{
    public class SensorReading
    {
        [AutoIncrement]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        [References(typeof(Sensor))]
        public string SensorId { get; set; }

        public UInt16 RawReading { get; set; }
        public decimal Reading { get; set; }

    }
}
