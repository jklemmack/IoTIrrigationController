using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using FTDI.D2xx.WinRT.Device;
using FTDI.D2xx.WinRT;
using Windows.UI.Xaml;
using Windows.System.Threading;
using System.Diagnostics;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace IotBackgroundIrrigationController
{
    public sealed class StartupTask : IBackgroundTask
    {
        FTManager ftManager;
        BackgroundTaskDeferral deferral;


        private ThreadPoolTimer timer;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to start one or more asynchronous methods 
            //
            ftManager = new FTManager();
            timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromMilliseconds(500));
            deferral = taskInstance.GetDeferral();

        }

        private void Timer_Tick(ThreadPoolTimer timer)
        {
            try
            {
                var devicesList = ftManager.GetDeviceList();
                Debug.WriteLine(devicesList.Count);

                if (devicesList.Count > 0)
                {
                    timer.Cancel();

                    var infoNode = devicesList[0];
                    IFTDevice ftDevice = ftManager.OpenByDeviceID(infoNode.DeviceId);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    ftDevice.SetBaudRateAsync(9600);
                    ftDevice.SetDataCharacteristicsAsync(WORD_LENGTH.BITS_8, STOP_BITS.BITS_1, PARITY.NONE);
                    ftDevice.SetFlowControlAsync(FLOW_CONTROL.NONE, 0x00, 0x00);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    XBeeDevice device = new XBeeDevice(ftDevice);

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}
