using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MadLedFrameworkSDK;
using MadLedSLSDriver;
using SimpleRGBCycleProvider;

namespace MadLedSDK
{
    class Program
    {
        private static ControlDevice cycleFan;
        private static ControlDevice bottomFan;
        static void Main(string[] args)
        {
            SLSManager ledManager = new SLSManager();

            var driver = new MadLedDriver();
            driver.Configure(new MadLedDriverDetails
            {
                ComPort = "COM3"
            });

            ledManager.Drivers.Add(driver);

            var cycleDriver = new SimpleRGBCycleDriver();
            ledManager.Drivers.Add(cycleDriver);

            List<ControlDevice> devices = ledManager.GetDevices();

            foreach (var controlDevice in devices)
            {
                Console.WriteLine(controlDevice.Name + " - " + controlDevice.DeviceType);
            }

            bottomFan = devices.First();
            var topFan = devices[1];
            cycleFan = devices.First(x => x.Name == "Simple Purple Propella");
            Console.WriteLine($"device: {bottomFan.Name} has {bottomFan.LEDs.Length} LEDs");
            Console.WriteLine($"device: {topFan.Name} has {topFan.LEDs.Length} LEDs");

            var timer = new Timer(TimerCallback, null, 0, 50);

            Console.ReadLine();
        }

        private static void TimerCallback(object state)
        {
            bottomFan.MapLEDs(cycleFan);
            bottomFan.Push();
        }
    }
}
