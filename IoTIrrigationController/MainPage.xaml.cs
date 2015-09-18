using FTDI.D2xx.WinRT;
using FTDI.D2xx.WinRT.Device;
using IrrigationController.Model.Types;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Gpio;
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
        ServerEventsClient seClient;
        JsonServiceClient client;

        List<ServerEventMessage> msgs = new List<ServerEventMessage>();
        List<ServerEventMessage> commands = new List<ServerEventMessage>();
        List<Exception> errors = new List<Exception>();
        ServerEventConnect connectMsg = null;

        Dictionary<string, bool> sprinklerValves = new Dictionary<string, bool>();

        const int RELAY_PIN = 5;
        GpioPin relayPin = null;

        public MainPage()
        {
            InitializeComponent();

            string baseUri = "http://irrigationcontroller.azurewebsites.net/";
            client = new JsonServiceClient(baseUri);
            seClient = new ServerEventsClient(baseUri, "relay")
            {
                OnConnect = e =>
                {
                    connectMsg = e;
                },
                OnCommand = e =>
                {
                    commands.Add(e);
                },
                OnMessage = e =>
                {
                    msgs.Add(e);
                },
                OnException = e =>
                {
                    errors.Add(e);
                }
            }
            ;
            seClient.Handlers["ZoneControl"] = HandleRelayCommand;
            seClient.Start();

            InitGPIO();
        }


        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ftManager = new FTManager();
            timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromMilliseconds(500));

            await seClient.Connect();
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

                        SensorReading reading = new SensorReading()
                        {
                            SensorAddress = frame.Address.ToString("X"),
                            RawReading = frame.A1,
                            Timestamp = DateTime.Now,
                            Reading = frame.A1 / 1024m,
                            RelayOn = false
                        };

                        if (relayPin != null)
                        {
                            GpioPinValue value = relayPin.Read();
                            reading.RelayOn = (value == GpioPinValue.High);
                        }

                        try
                        {
                            client.Post(reading);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                    }
                });
            ListenForData();
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                relayPin = null;
                return;
            }

            relayPin = gpio.OpenPin(RELAY_PIN);

            // Show an error if the pin wasn't initialized properly
            if (relayPin == null)
            {
                return;
            }

            relayPin.Write(GpioPinValue.High);
            relayPin.SetDriveMode(GpioPinDriveMode.Output);

        }

        private void HandleRelayCommand(ServerEventsClient client, ServerEventMessage msg)
        {

        }
    }
}
