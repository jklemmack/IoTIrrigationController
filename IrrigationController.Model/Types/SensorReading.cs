using ServiceStack;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrrigationController.Model.Types
{    
    [Route("/sensorreading", "POST")]
    public class SensorReading
    {
        [AutoIncrement]
        public long Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string SensorAddress { get; set; }

        public UInt16 RawReading { get; set; }
        public decimal Reading { get; set; }

        public bool? RelayOn { get; set; }

    }
}
