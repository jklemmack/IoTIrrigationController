using IrrigationController.Model.Types;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrrigationController.Model
{
    public class SensorRequest : QueryBase<Sensor>
    {
        public string Id { get; set; }
    }
}
