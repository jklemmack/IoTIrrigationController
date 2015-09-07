using FTDI.D2xx.WinRT;
using FTDI.D2xx.WinRT.Device;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace IoTIrrigationController
{
    public class XBeeDevice
    {
        IFTDevice device;
        MemoryStream memoryStream;

        public XBeeDevice(IFTDevice device)
        {
            this.device = device;
            memoryStream = new MemoryStream();
        }

        public async Task<IEnumerable<XBeeSensorData>> ReadAPIPacket()
        {
            XBeeSensorData result = null;

            var xbee = await Task<IEnumerable<XBeeSensorData>>.Run(async () =>
            {
                List<XBeeSensorData> outData = new List<XBeeSensorData>();
                do
                {
                    try
                    {
                        // Get data from FTDI device
                        uint bytesAvailable = device.GetQueueStatus();
                        if (bytesAvailable == 0) bytesAvailable = 1;
                        byte[] buffer = new byte[bytesAvailable];

                        // Write out to MemoryStream
                        var bytesRead = await device.ReadAsync(buffer, (uint)buffer.Length);
                        memoryStream.Write(buffer, 0, (int)bytesRead);

                        // Process the MemoryStream to get an API sensor data frame
                        // We may have multiple frames, so we output an enumerable list
                        while ((result = ReadAPIFrame()) != null)
                            outData.Add(result);
                    }
                    catch (Exception ex) { }

                } while (outData.Count == 0);
                return outData;
            });

            return xbee;
        }

        XBeeSensorData ReadAPIFrame()
        {
            long position = 0;
            int val = -1;

            // Read to first 0x7E - frame start
            memoryStream.Position = 0;
            do
            {
                val = memoryStream.ReadByte();
                if (val == -1) return null;
                if (val == 0x7E) break;
            } while (true);

            if (val == -1) return null;

            // Note the position of the beginning of the frame, in case we bug out with a partial frame in the future
            position = memoryStream.Position - 1;

            // Read the frame length (next two bytes)
            UInt16 length = 0;
            if (!memoryStream.ReadUInt16(ref length)) return null;

            // Read in the whole frame
            length++;   // The frame length specifier doesn't include the checksum byte

            byte[] data = new byte[length];

            // If we don't have all the data, "reset" the memory stream back to how it was
            // at the frame beginning demarcation
            int bytesRead = memoryStream.Read(data, 0, (int)length);
            if (bytesRead != (int)length)
            {
                var tempMemoryStream2 = new MemoryStream();
                memoryStream.Position = position;
                memoryStream.CopyTo(tempMemoryStream2);
                memoryStream = tempMemoryStream2;
                return null;
            }
            else
            {
                // Else copy to a new memorystream for the next block of data
                var tempMemoryStream = new MemoryStream();
                memoryStream.CopyTo(tempMemoryStream);
                memoryStream = tempMemoryStream;
            }

            return ProcessXBeeFrame(data);
        }

        [Flags]
        enum DigitalBits : UInt16
        {
            D0 = 1 << 0,
            D1 = 1 << 1,
            D2 = 1 << 2,
            D3 = 1 << 3,
            D4 = 1 << 4,
            D5 = 1 << 5,
            D6 = 1 << 6,
            D7 = 1 << 7,
            D8 = 1 << 8
        }

        [Flags]
        enum AnalogChannels : UInt16
        {
            A5 = 1 << 14,
            A4 = 1 << 13,
            A3 = 1 << 12,
            A2 = 1 << 11,
            A1 = 1 << 10,
            A0 = 1 << 9
        }

        static Random random = new Random();
        static ushort count = 0;
        private XBeeSensorData ProcessXBeeFrame(byte[] data)
        {
            byte sum = (byte)data.Sum(q => q);
            if (sum != 0xFF) return null;    // invalid checksum

            int pos = 0;

            byte command = data[pos++];
            if (command != 0x82) return null;   // Only take 64-bit receive packets

            UInt64 address = 0;
            for (int i = 0; i < 8; i++)
            {
                address = (address << 8) + data[pos++];
            }
            var sAddr = address.ToString("X");
            byte rssi = data[pos++];
            byte options = data[pos++];


            int sampleCount = data[pos++];

            //UInt16 channelMask = (UInt16)((UInt16)(data[pos++] << 8) + (UInt16)(data[pos++]));
            UInt16 channelMask = ReadWordFromArray(data, ref pos);

            XBeeSensorData sensorData = null;
            for (int i = 0; i < sampleCount; i++)
            {
                sensorData = new XBeeSensorData()
                {
                    Address = address
                };
                ProcessSensorData(sensorData, channelMask, ref pos, data);
            }

            return sensorData;
        }

        XBeeSensorData ProcessSensorData(XBeeSensorData sensor, UInt16 channelMask, ref int pos, byte[] data)
        {
            const UInt16 DIGITAL_MASK = 0x1F;
            UInt16 digitalMask = (UInt16)(DIGITAL_MASK & channelMask);

            if (((channelMask & (UInt16)AnalogChannels.A5) != 0))
                sensor.A5 = ReadWordFromArray(data, ref pos);

            if (((channelMask & (UInt16)AnalogChannels.A4) != 0))
                sensor.A4 = ReadWordFromArray(data, ref pos);

            if (((channelMask & (UInt16)AnalogChannels.A3) != 0))
                sensor.A3 = ReadWordFromArray(data, ref pos);

            if (((channelMask & (UInt16)AnalogChannels.A2) != 0))
                sensor.A2 = ReadWordFromArray(data, ref pos);

            if (((channelMask & (UInt16)AnalogChannels.A1) != 0))
                sensor.A1 = ReadWordFromArray(data, ref pos);

            if (((channelMask & (UInt16)AnalogChannels.A0) != 0))
                sensor.A0 = ReadWordFromArray(data, ref pos);


            return sensor;
        }

        UInt16 ReadWordFromArray(byte[] data, ref int pos)
        {
            return (UInt16)((UInt16)(data[pos++] << 8) + (UInt16)(data[pos++]));
        }
    }

    static class MemoryStreamExtensions
    {

        public static bool ReadUInt16(this Stream stream, ref UInt16 value)
        {
            byte[] buffer = new byte[2];
            if (stream.Read(buffer, 0, 2) != 2)
                return false;

            value = (UInt16)((UInt16)(buffer[0] << 8)
                + (UInt16)(buffer[1]));

            return true;
        }
    }

}
