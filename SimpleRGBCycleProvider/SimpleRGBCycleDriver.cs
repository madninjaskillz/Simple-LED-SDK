using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MadLedFrameworkSDK;

namespace SimpleRGBCycleProvider
{
    public class SimpleRGBCycleDriver : ISimpleLEDDriver
    {
        private const int LEDCount = 32;
        private Timer timer;
        private int currentPos = 0;
        ControlDevice.LedUnit[] leds = new ControlDevice.LedUnit[32];
        ControlDevice.LedUnit[] fanLeds = new ControlDevice.LedUnit[32];
        public SimpleRGBCycleDriver()
        {
            for (int i = 0; i < LEDCount; i++)
            {
                leds[i] = new ControlDevice.LedUnit
                {
                    LEDName = "LED " + i,
                    Data = new SimpleRGBCycleLEDData
                    {
                        LED = i,
                        R = i * 0.1f,
                        G = i * 0.04f,
                        B = i * .03f
                    },
                };

                fanLeds[i] = new ControlDevice.LedUnit
                {
                    LEDName = "LED " + i,
                    Data = new SimpleRGBCycleLEDData
                    {
                        LED = i,
                        R = i * 0.1f,
                        G = i * 0.04f,
                        B = i * .03f
                    },
                };
            }

            timer = new Timer(TimerCallback, null, 0, 33);
        }

        private void TimerCallback(object state)
        {
            for (int i = 0; i < LEDCount; i++)
            {
                var tmp = ((SimpleRGBCycleLEDData)leds[i].Data);
                if (tmp.R + tmp.RInc > 1f || tmp.R + tmp.RInc < 0f) tmp.RInc = -tmp.RInc;
                if (tmp.G + tmp.GInc > 1f || tmp.G + tmp.GInc < 0f) tmp.GInc = -tmp.RInc;
                if (tmp.B + tmp.BInc > 1f || tmp.B + tmp.BInc < 0f) tmp.BInc = -tmp.RInc;

                tmp.R = tmp.R + tmp.RInc;
                tmp.G = tmp.G + tmp.GInc;
                tmp.B = tmp.B + tmp.BInc;

                leds[i].Color = new ControlDevice.LEDColor((int)(tmp.R * 255), (int)(tmp.G * 255), (int)(tmp.B * 255));
            }

            for (int i = 0; i < LEDCount; i++)
            {
                var tmp = ((SimpleRGBCycleLEDData)fanLeds[i].Data);

                ControlDevice.LEDColor cl = ((currentPos + i) % (LEDCount/2)) != 0 ? new ControlDevice.LEDColor(0, 0, 0) : new ControlDevice.LEDColor(255, 0, 255);

                fanLeds[i].Color = cl;
            }

            currentPos++;
        }



        public void Configure(DriverDetails driverDetails)
        {
            
        }

        public List<ControlDevice> GetDevices()
        {
            return new List<ControlDevice>
            {
                new ControlDevice
                {
                    Name = "Simple RGB Cycler",
                    Driver = this,
                    LEDs = leds
                },
                new ControlDevice
                {
                    Name = "Simple Purple Propella",
                    Driver = this,
                    LEDs = fanLeds
                }
            };

        }

        public void Push(ControlDevice controlDevice)
        {
            
        }

        public void Pull(ControlDevice controlDevice)
        {
            
        }

        public DriverProperties GetProperties()
        {
            return new DriverProperties
            {
                SupportsPull = false
            };
        }

        public class SimpleRGBCycleLEDData
        {
            public int LED { get; set; }
            public float R { get; set; }
            public float G { get; set; }
            public float B { get; set; }
            public float BInc = 0.1f;
            public float GInc = 0.04f;
            public float RInc = 0.01f;
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
