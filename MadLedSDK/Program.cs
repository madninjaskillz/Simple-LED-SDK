using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FanClock;
using MadLedFrameworkSDK;
using MadLedSLSDriver;
using MSIProvider;
using SimpleRGBCycleProvider;

namespace MadLedSDK
{
    class Program
    {
        private static ControlDevice cycleFan;
        private static ControlDevice bottomFan;
        private static ControlDevice backFan;
        private static ControlDevice msiGpu;
        private static ControlDevice topFan;
        static void Main(string[] args)
        {
            SLSManager ledManager = new SLSManager();

            var driver = new MadLedDriver();
            driver.Configure(null);
            ledManager.Drivers.Add(driver);

            SimpleRGBCycleDriver cycleDriver = new SimpleRGBCycleDriver();
            ledManager.Drivers.Add(cycleDriver);


            List<ControlDevice> devices = ledManager.GetDevices();

            foreach (var controlDevice in devices)
            {
                Console.WriteLine(controlDevice.Name + " - " + controlDevice.DeviceType);
            }

            bottomFan = devices.First(x => x.Name == "Bottom Front");
            
            bottomFan.LedShift = 5;
            
            topFan = devices.First(x => x.Name == "Top Front");
            backFan = devices[2];

            topFan.Reverse = true;
            bottomFan.Reverse = true;

            cycleFan = devices.First(x => x.Name == "Simple RGB Propella");
            
            var timer = new Timer(TimerCallback, null, 0, 33);

            Console.ReadLine();
        }

        private static void TimerCallback(object state)
        {
            bottomFan.MapLEDs(cycleFan);
            bottomFan.Push();

            topFan.MapLEDs(cycleFan);
            topFan.Push();

            backFan.MapLEDs(cycleFan);
            backFan.Push();

        }
    }
}
