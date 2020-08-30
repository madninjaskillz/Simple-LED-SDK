using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FanClock;
using ICUEDriver;
using IT8297Driver;
using MadLedFrameworkSDK;
using MadLedSLSDriver;
using MSIProvider;
using ScreenShotSource;
using SimpleRGBCycleProvider;
using SteelSeriesSLSProvider;

namespace MadLedSDK
{
    class Program
    {
        static void Main(string[] args)
        {



            SLSManager ledManager = new SLSManager();
            ledManager.Drivers.Add(new IT8296Provider());
            ledManager.Drivers.Add(new SteelSeriesDriver());
            ledManager.Drivers.Add(new SimpleRGBCycleDriver());
            //ledManager.Drivers.Add(new MadLedDriver());
            ledManager.Drivers.Add(new ScreenShotSourceProvider());
            ledManager.Drivers.Add(new CUEDriver());
            ledManager.Init();
            Console.WriteLine("Getting devices");
            List<ControlDevice> devices = ledManager.GetDevices();

            Dictionary<int, ControlDevice> driv = new Dictionary<int, ControlDevice>();
            int ct = 1;
            foreach (var controlDevice in devices)
            {
                Console.WriteLine(ct + ": " + controlDevice.Driver.Name() + "-" + controlDevice.Name + " - " + controlDevice.DeviceType + ", " + controlDevice.LEDs?.Length + " LEDs");
                driv.Add(ct, controlDevice);
                ct++;
            }

            Console.WriteLine("Type Source Number");
            string derp = Console.ReadLine();



            ControlDevice cycleFan = driv[int.Parse(derp)]; //devices.First(xx => xx.Name == "Simple RGB Cycler");

            var timer = new Timer((state) =>
            {
                foreach (var t in devices.Where(xx => xx.Driver.GetProperties().SupportsPush && xx.LEDs != null && xx.LEDs.Length > 0))
                {
                    if (cycleFan.Driver.GetProperties().SupportsPull)
                    {
                        cycleFan.Pull();
                    }

                    t.MapLEDs(cycleFan);
                    t.Push();
                }

            }, null, 0, 33);

            while (true)
            {
                Console.WriteLine("Type Source Number (Q TO QUIT)");
                derp = Console.ReadLine();
                if (derp.ToUpper() == "Q")
                {
                    return;
                }

                cycleFan = driv[int.Parse(derp)]; //devices.First(xx => xx.Name == "Simple RGB Cycler");

            }
            
        }

    }
}
