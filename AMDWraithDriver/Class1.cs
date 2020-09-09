using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AMDWraith;
using MadLedFrameworkSDK;

namespace AMDWraithDriver
{
    public class AMDWraithDriver : ISimpleLEDDriver
    {
        private ControlDevice.LedUnit[] leds;
        CMRGBController cmrgb;
        private bool disposing;
        private bool pushRequested;
        public AMDWraithDriver()
        {
            cmrgb = new CMRGBController();
            Debug.WriteLine(cmrgb.getVersion());

            var ring_leds = new LedChannel[15];

            for (int r = 0; r < 5; r++)
            {
                for (int x = 0; x < 15; x++)
                {
                    ring_leds[x] = AMDWraith.LedChannel.R_RAINBOW;
                }

                Debug.WriteLine(cmrgb
                    .assign_leds_to_channels(AMDWraith.LedChannel.OFF, AMDWraith.LedChannel.OFF, ring_leds)
                    .PrettyBytes());

                Thread.Sleep(500);

                for (int x = 0; x < 15; x++)
                {
                    ring_leds[x] = AMDWraith.LedChannel.OFF;
                }

                Debug.WriteLine(cmrgb.assign_leds_to_channels(AMDWraith.LedChannel.OFF, AMDWraith.LedChannel.OFF, ring_leds).PrettyBytes());
                Thread.Sleep(500);
            }
            
           



            leds = new ControlDevice.LedUnit[15];
            for (int i = 0; i < 15; i++)
            {
                leds[i] = new ControlDevice.LedUnit
                {
                    Color = new LEDColor(0, 0, 0),
                    LEDName = "Ring " + (i + 1)
                };
            }

            pushRequested = true;
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                ManagePOV();
            }).Start();
        }

        public void Dispose()
        {
            disposing = true;
        }

        public void Configure(DriverDetails driverDetails)
        {
            //throw new NotImplementedException();
        }

        public List<ControlDevice> GetDevices()
        {
            return new List<ControlDevice>
            {
                new ControlDevice
                {
                    DeviceType = DeviceTypes.Fan,
                    Driver = this,
                    LEDs = leds,
                    Name = "Ring PoV"
                }
            };
        }

        public void Push(ControlDevice controlDevice)
        {
            pushRequested = true;
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
                SupportsCustomConfig = false,
                Id = Guid.Parse("49440cc2-8ca3-4e35-a9a3-88b024cc0e2d")
            };
        }

        public T GetConfig<T>() where T : SLSConfigData
        {
            return null;
        }

        public void PutConfig<T>(T config) where T : SLSConfigData
        {
            
        }

        public string Name()
        {
            return "AMD Wraith Prism";
        }

        public void ManagePOV()
        {
            LEDColor[] myleds= new LEDColor[15];
            while (!disposing)
            {
                if (pushRequested)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        myleds[i] = new LEDColor(leds[i].Color.Red, leds[i].Color.Green, leds[i].Color.Blue);
                    }

                    pushRequested = false;
                }

                if (!cmrgb.SetFrame(myleds))
                {
                    Thread.Sleep(33);
                }

            }
        }
    }
}
