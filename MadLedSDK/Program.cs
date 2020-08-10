using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MadLedFrameworkSDK;
using MadLedSLSDriver;

namespace MadLedSDK
{
    class Program
    {
        static void Main(string[] args)
        {
            SLSManager ledManager = new SLSManager();

            var driver = new MadLedDriver();
            driver.Configure(new MadLedDriverDetails
            {
                ComPort = "COM3"
            });

            ledManager.Drivers.Add(driver);

            List<ControlDevice> devices = ledManager.GetDevices();

            foreach (var controlDevice in devices)
            {
                Console.WriteLine(controlDevice.Name + " - "+controlDevice.DeviceType);
            }

            var bottomFan = devices.First();
            var topFan = devices.Last();

            Console.WriteLine($"device: {bottomFan.Name} has {bottomFan.LEDs.Length} LEDs");
            Console.WriteLine($"device: {topFan.Name} has {topFan.LEDs.Length} LEDs");

            for (int i = 0; i < 7; i++)
            {
                bottomFan.LEDs[i].Color = new ControlDevice.LEDColor(255, 0, 255);
            }

            for (int i = 15; i < 21; i++)
            {
                bottomFan.LEDs[i].Color = new ControlDevice.LEDColor(0, 255, 255);
            }

            bottomFan.Push();

            topFan.MapLEDs(bottomFan);
            topFan.Push();


            Console.ReadLine();
        }
    }
}
