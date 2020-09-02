using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidSharp;
using MadLedFrameworkSDK;
using DeviceTypes = MadLedFrameworkSDK.DeviceTypes;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using Image = System.Drawing.Image;


namespace IT8297Driver
{
    public class IT8296Provider : ISimpleLEDDriverWithConfig
    {
        HidStream stream = null;

        private ControlDevice.LedUnit[] leds1;
        private ControlDevice.LedUnit[] leds2;
        private ControlDevice.LedUnit[] vrmLeds = new ControlDevice.LedUnit[1];
        private ControlDevice.LedUnit[] pciLeds = new ControlDevice.LedUnit[1];

        private const int VENDOR_ID = 0x048D;
        private List<int> supportedIds = new List<int>{ 0x8297 };
        public void Dispose()
        {
            
        }

        public void Configure(DriverDetails driverDetails)
        {
            IT8297Config config = new IT8297Config();
            //todo read from config

            leds1 = new ControlDevice.LedUnit[config.Fan1LedCount];
            leds2 = new ControlDevice.LedUnit[config.Fan2LedCount];

            for (int i = 0; i < leds1.Length; i++)
            {
                leds1[i] = new ControlDevice.LedUnit
                {
                    Color = new LEDColor(0, 0, 0),
                    Data = new ControlDevice.LEDData
                    {
                        LEDNumber = i
                    },
                    LEDName = "ARGB 1 LED " + (i + 1)
                };
            }


            for (int i = 0; i < leds2.Length; i++)
            {
                leds2[i] = new ControlDevice.LedUnit
                {
                    Color = new LEDColor(0, 0, 0),
                    Data = new ControlDevice.LEDData
                    {
                        LEDNumber = i
                    },
                    LEDName = "ARGB 2 LED " + (i + 1)
                };
            }

            vrmLeds[0] = new ControlDevice.LedUnit
            {
                Data = new ControlDevice.LEDData
                {
                    LEDNumber = 0
                },
                LEDName = "VRM Block"
            };

            pciLeds[0] = new ControlDevice.LedUnit
            {
                Data = new ControlDevice.LEDData
                {
                    LEDNumber = 0
                },
                LEDName = "PCI Area"
            };
        }

        public List<ControlDevice> GetDevices()
        {
            var terp = new OpenConfiguration();
            terp.SetOption(OpenOption.Transient, true);

            var loader = new HidDeviceLoader();
            HidDevice device = null;
            HidSharp.HidDevice[] devices = null;
            foreach (var supportedId in supportedIds)
            {
                HidSharp.HidDevice[] tempdevices = loader.GetDevices(VENDOR_ID, supportedId).ToArray();
                if (tempdevices.Length > 0)
                {
                    devices = tempdevices;
                }
            }

            if (devices == null || devices.Length == 0)
            {
                return new List<ControlDevice>();
            }
            
            
            int attempts = 0;
            bool success = false;
            
            while (attempts < 100 && !success)
            {
                try
                {
                    Console.WriteLine("Attempting connection");
                    device = devices[attempts % devices.Count()];
                    byte[] t = device.GetRawReportDescriptor();
                    Console.WriteLine(device.GetFriendlyName());

                    stream = device.Open(terp);
                    stream.SetCalibration();
                    stream.SendPacket(0x60, 0);
                    success = true;
                }
                catch
                {
                    attempts++;
                    Thread.Sleep(100);
                }
            }

            if (!success)
            {
                return new List<ControlDevice>();
            }

            Bitmap pcieArea;
            Bitmap rgbPins;
            Bitmap vrm;

            Assembly myAssembly = Assembly.GetExecutingAssembly();
            using (Stream myStream = myAssembly.GetManifestResourceStream("IT8297Driver.PCIArea.png"))
            {
                pcieArea = (Bitmap)Image.FromStream(myStream);
            }

            using (Stream myStream = myAssembly.GetManifestResourceStream("IT8297Driver.rgbpins.png"))
            {
                rgbPins = (Bitmap)Image.FromStream(myStream);
            }

            using (Stream myStream = myAssembly.GetManifestResourceStream("IT8297Driver.VRM.png"))
            {
                vrm = (Bitmap)Image.FromStream(myStream);
            }


            byte[] buffer = new byte[64];
            buffer[0] = 0xCC;
            stream.GetFeature(buffer);
            It8297ReportComplete report = GetReport(buffer);

            stream.SetLedCount();

            stream.Init();

            List<ControlDevice> result = new List<ControlDevice>();

            result.Add(new ControlDevice
            {
                LEDs = leds1,
                Driver = this,
                DeviceType = DeviceTypes.Fan,
                Name = "ARGB Header 1",
                ProductImage = rgbPins,
                SupportsLEDCountOverride = true
            });


            result.Add(new ControlDevice
            {
                LEDs = leds2,
                Driver = this,
                DeviceType = DeviceTypes.Fan,
                Name = "ARGB Header 2",
                ProductImage = rgbPins,
                SupportsLEDCountOverride = true
            });


            result.Add(new ControlDevice
            {
                LEDs = vrmLeds,
                Driver = this,
                DeviceType = DeviceTypes.MotherBoard,
                Name = "VRM Block",
                ProductImage = vrm
            });


            result.Add(new ControlDevice
            {
                LEDs = pciLeds,
                Driver = this,
                DeviceType = DeviceTypes.MotherBoard,
                Name = "PCI area",
                ProductImage = pcieArea
            });

            return result;
        }

        public void Push(ControlDevice controlDevice)
        {
            if (controlDevice.Name == "ARGB Header 1")
            {
                stream.SendRGB(leds1.Select(x => x.Color).ToList(), 0x58);
            }


            if (controlDevice.Name == "ARGB Header 2")
            {
                stream.SendRGB(leds2.Select(x => x.Color).ToList(), 0x59);
            }

            if (controlDevice.Name == "VRM Block")
            {
                stream.SetLEDEffect(32, 1, (byte)vrmLeds[0].Color.Red, (byte)vrmLeds[0].Color.Green, (byte)vrmLeds[0].Color.Blue);
            }


            if (controlDevice.Name == "PCI area")
            {
                stream.SetLEDEffect(35, 1, (byte)vrmLeds[0].Color.Red, (byte)vrmLeds[0].Color.Green, (byte)vrmLeds[0].Color.Blue);
            }
        }

        public void Pull(ControlDevice controlDevice)
        {
            
        }

        public DriverProperties GetProperties()
        {
            return new DriverProperties
            {
                SupportsPush = true,
                IsSource = false,
                SupportsPull = false,
                SupportsCustomConfig = true,
                Id = Guid.Parse("49440d02-8ca3-4e35-a9a3-88b024cc0e2d")
            };
        }

        public T GetConfig<T>() where T : SLSConfigData
        {
            GigabyteConfigModel data = new GigabyteConfigModel();
            SLSConfigData proxy = (SLSConfigData) data;
            return (T)proxy;
        }

        public void PutConfig<T>(T config) where T : SLSConfigData
        {
            GigabyteConfigModel proxy = config as GigabyteConfigModel;
            Debug.WriteLine(proxy);
        }

        public UserControl GetCustomConfig()
        {
            return new CustomConfig();
        }

        public string Name()
        {
            return "Aorus Motherboard";
        }




        public static It8297ReportComplete GetReport(byte[] buffer)
        {
            IT8297_Report featureReport = buffer.ReadStruct<IT8297_Report>();
            byte[] str_product = new byte[32];
            Buffer.BlockCopy(buffer, 12, str_product, 0, 32);
            string ProductName = "";

            using (var ms = new MemoryStream(str_product))
            {
                using (var sr = new StreamReader(ms))
                {
                    ProductName = sr.ReadLine();
                }
            }

            for (int i = 0; i < ProductName.Length; i++)
            {
                if (ProductName.Substring(i, 1) == "\0")
                {
                    ProductName = ProductName.Substring(0, i);
                    break;
                }
            }

            return new It8297ReportComplete
            {
                ProductName = ProductName,
                report_id = featureReport.report_id,
                product = featureReport.product,
                device_num = featureReport.device_num,
                total_leds = featureReport.total_leds,
                fw_ver = featureReport.fw_ver,
                curr_led_count = featureReport.curr_led_count,
                reserved0 = featureReport.reserved0,
                byteorder0 = featureReport.byteorder0,
                byteorder1 = featureReport.byteorder1,
                byteorder2 = featureReport.byteorder2,
                chip_id = featureReport.chip_id,
                reserved1 = featureReport.reserved1

            };
        }

        [StructLayout(LayoutKind.Explicit, Size = 64, CharSet = CharSet.Ansi)]
        public class IT8297_Report
        {
            [FieldOffset(0)] public byte report_id;
            [FieldOffset(1)] public byte product;
            [FieldOffset(2)] public byte device_num;
            [FieldOffset(3)] public byte total_leds;
            [FieldOffset(4)] public UInt32 fw_ver;
            [FieldOffset(8)] public UInt16 curr_led_count;
            [FieldOffset(10)] public UInt16 reserved0;
            [FieldOffset(44)] public UInt32 byteorder0;
            [FieldOffset(48)] public UInt32 byteorder1;
            [FieldOffset(52)] public UInt32 byteorder2;
            [FieldOffset(56)] public UInt32 chip_id;
            [FieldOffset(60)] public UInt32 reserved1;
        };

        public class It8297ReportComplete : IT8297_Report
        {
            public string ProductName { get; set; }
            public Extensions.LEDCount LEDCount => (Extensions.LEDCount)total_leds;
        }


    }
}
