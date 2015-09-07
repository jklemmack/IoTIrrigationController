using FTDI.D2xx.WinRT;
using FTDI.D2xx.WinRT.Device;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IoTIrrigationController
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        XBeeDevice device;
        FTManager ftManager;
        ThreadPoolTimer timer;

        public MainPage()
        {
            InitializeComponent();

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ftManager = new FTManager();
            timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromMilliseconds(500));

            //try
            //{
            //    ftManager = new FTManager();
            //    var devicesList = ftManager.GetDeviceList();
            //    Debug.WriteLine(devicesList.Count);

            //    if (devicesList.Count > 0)
            //    {
            //        var deviceId = devicesList[0].DeviceId;
            //        device = await FtdiDevices.Connect(deviceId);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw;
            //}

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

                    device = new XBeeDevice(ftDevice);
                    ListenForData();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        private async void ListenForData()
        {
            await device.ReadAPIPacket()
                .ContinueWith(frames =>
                {
                    foreach (var frame in frames.Result)
                    {
                        Debug.WriteLine("{2}: {0:X}\t{1,4}", frame.Address, frame.A1.ToString("X4"), DateTime.Now.ToString("mm:ss.fff"));
                    }
                });
            ListenForData();
        }

    }
}
