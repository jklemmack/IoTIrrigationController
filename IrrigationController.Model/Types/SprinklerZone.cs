using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrrigationController.Model.Types
{
    [Flags]
    public enum ControlStatus
    {
        Off = 0,
        On = 1,
        RequestOff = 2,
        RequestOn = 3
    }

    public class SprinklerZone
    {
        public int Id { get; set; }

        public ControlStatus ControlStatus { get; set; }

    }
}
