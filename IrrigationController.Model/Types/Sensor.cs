using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IrrigationController.Model.Types
{
    [Route("/Sensor")]
    public class Sensor 
    {
        public string Id
        {
            get
            {
                return SensorAddress + ":" + SensorIndex.ToString();
            }
        }

        public string SensorAddress { get; set; }
        public short SensorIndex { get; set; }
    }
}
