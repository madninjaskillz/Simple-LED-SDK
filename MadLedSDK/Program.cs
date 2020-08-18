using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static ControlDevice corsairDevice;
        
        static void Main(string[] args)
        {
            SLSManager ledManager = new SLSManager();

            
            var ICUEDriver = new ICUEDriver.CUEDriver();
            ICUEDriver.Configure(null);
            ledManager.Drivers.Add(ICUEDriver);

            //Console.WriteLine("setting up madled");
            //MadLedDriver madLed = new MadLedDriver();
            //Console.WriteLine("Configuring madled");
            //madLed.Configure(null);
            //Console.WriteLine("Adding madled");
            //ledManager.Drivers.Add(madLed);

            SimpleRGBCycleDriver cycleDriver = new SimpleRGBCycleDriver();
            ledManager.Drivers.Add(cycleDriver);


            Console.WriteLine("Getting devices");
            List<ControlDevice> devices = ledManager.GetDevices();

            foreach (var controlDevice in devices)
            {
                Console.WriteLine(controlDevice.Driver.Name()+"-"+ controlDevice.Name + " - " + controlDevice.DeviceType+", "+controlDevice.LEDs?.Length+" LEDs");
            }

            
            corsairDevice = devices.First(x => x.Name == "Corsair MM800RGB");
            cycleFan = devices.First(x => x.Name == "Simple RGB Propella");

            var timer = new Timer(TimerCallback, null, 0, 33);

            Console.ReadLine();
        }

        private static void TimerCallback(object state)
        {
            corsairDevice.MapLEDs(cycleFan);
            corsairDevice.Push();
                //bottomFan.MapLEDs(cycleFan);
            //bottomFan.Push();
            //topFan.MapLEDs(cycleFan);
            //topFan.Push();

            //spareFan.MapLEDs(cycleFan);
            //spareFan.Push();

            //backFan.MapLEDs(cycleFan);
            //backFan.Push();

            //msiGpu.MapLEDs(cycleFan);
            //msiGpu.Push();

        }
    }
}
