//using System.Collections.Generic;
//using System.Threading;
//using MadLedFrameworkSDK;

//namespace SimpleRGBCycleProvider
//{
//    public class SimpleFanCycleDriver : ISimpleLEDDriver
//    {
//        private const int LEDCount = 32;
//        private Timer timer;
//        private int currentPos = 0;
//        ControlDevice.LedUnit[] leds = new ControlDevice.LedUnit[32];
//        ControlDevice.LedUnit[] fanLeds = new ControlDevice.LedUnit[32];

//        public SimpleFanCycleDriver()
//        {
//            for (int i = 0; i < LEDCount; i++)
//            {
//                leds[i] = new ControlDevice.LedUnit
//                {
//                    LEDName = "LED " + i,
//                    Data = new SimpleRGBCycleLEDData
//                    {
//                        LED = i,
//                        R = i * 0.1f,
//                        G = i * 0.04f,
//                        B = i * .03f
//                    },
//                };
//            }

//            timer = new Timer(TimerCallback, null, 0, 50);
//        }

//        private void TimerCallback(object state)
//        {
//            currentPos++;

//            for (int i = 0; i < LEDCount; i++)
//            {
//                var tmp = ((SimpleRGBCycleLEDData)leds[i].Data);

//                ControlDevice.LEDColor cl = ((currentPos + i) % 16) != 0 ? new ControlDevice.LEDColor(0, 0, 0) : new ControlDevice.LEDColor(255, 0, 255);

//                leds[i].Color = cl;
//            }

//            currentPos++;

//            for (int i = 0; i < LEDCount; i++)
//            {
//                var tmp = ((SimpleRGBCycleLEDData)fanLeds[i].Data);

//                ControlDevice.LEDColor cl = ((currentPos + i) % 16) != 0 ? new ControlDevice.LEDColor(0, 0, 0) : new ControlDevice.LEDColor(255, 0, 255);

//                fanLeds[i].Color = cl;
//            }
//        }

//        public void Configure(DriverDetails driverDetails)
//        {

//        }

//        public List<ControlDevice> GetDevices()
//        {
//            return new List<ControlDevice>
//            {
//                new ControlDevice
//                {
//                    Name = "Simple Fan Cycler",
//                    Driver = this,
//                    LEDs = leds
//                }
//            };

//        }

//        public void Push(ControlDevice controlDevice)
//        {

//        }

//        public void Pull(ControlDevice controlDevice)
//        {

//        }

//        public DriverProperties GetProperties()
//        {
//            return new DriverProperties
//            {
//                SupportsPull = false
//            };
//        }

//        public class SimpleRGBCycleLEDData
//        {
//            public int LED { get; set; }
//            public float R { get; set; }
//            public float G { get; set; }
//            public float B { get; set; }
//            public float BInc = 0.1f;
//            public float GInc = 0.04f;
//            public float RInc = 0.01f;
//        }

//        public void Dispose()
//        {
//            timer.Dispose();
//        }
//    }
//}