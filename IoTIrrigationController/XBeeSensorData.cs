using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IoTIrrigationController
{
    public class XBeeSensorData : INotifyPropertyChanged
    {
        public UInt64 Address { get; set; }

        private bool d0, d1, d2, d3, d4, d5, d6, d7;
        private ushort a0, a1, a2, a3, a4, a5, a6, a7, a8;

        public bool D0
        {
            get { return d0; }
            set { d0 = value; NotifyPropertyChanged(); }
        }

        public bool D1 { get; set; }
        public bool D2 { get; set; }
        public bool D3 { get; set; }
        public bool D4 { get; set; }
        public bool D5 { get; set; }
        public bool D6 { get; set; }
        public bool D7 { get; set; }


        public ushort A0
        {
            get { return a0; }
            set { a0 = value; NotifyPropertyChanged(); }
        }
        public UInt16 A1 { get; set; }
        public UInt16 A2 { get; set; }
        public UInt16 A3 { get; set; }
        public UInt16 A4 { get; set; }
        public UInt16 A5 { get; set; }
        public UInt16 A6 { get; set; }
        public UInt16 A7 { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
