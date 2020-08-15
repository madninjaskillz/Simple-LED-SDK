using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MadLedFrameworkSDK;

namespace MadLedSLSDriver
{
    public class MadLedDriver : ISimpleLEDDriver
    {

        //private MadLedSerialDriver serialDriver;
        private List<MadLedSerialDriver> serialDrivers = new List<MadLedSerialDriver>();
        public void Configure(DriverDetails driverDetails)
        {
            MadLedDriverDetails specificDriverDetails = driverDetails as MadLedDriverDetails;

            //if (specificDriverDetails == null)
            //{
            //    throw new ArgumentException();
            //}

            for (int i = 1; i < 10; i++)
            {
                Debug.WriteLine("testing COM"+i+":");
                try
                {
                    var serialDriver = new MadLedSerialDriver("COM"+i);
                
                    if (serialDriver.Ping() < TimeSpan.FromSeconds(1))
                    {
                        Debug.WriteLine("adding COM" + i + ":");
                        serialDrivers.Add(serialDriver);
                    }
                }
                catch
                {
                }
            }
         
            Debug.WriteLine("init done");
        }

        public List<ControlDevice> GetDevices()
        {
            //this is faked to hell till i update the arduino code - this should be read from device via serial driver.

            var results = new List<ControlDevice>();

            foreach (var madLedSerialDriver in serialDrivers)
            {
                string stuff = madLedSerialDriver.GetConfig();

                if (!string.IsNullOrWhiteSpace(stuff))
                {
                    var lines = stuff.Split('~');
                    foreach (var line in lines)
                    {
                        string name = line.Split(':')[0];
                        int bank = int.Parse(line.Split(':')[1]);
                        int ledCount = int.Parse(line.Split(':')[2]);

                        MadLedDevice dvc = new MadLedDevice
                        {
                            Bank = bank,
                            Name = name,
                            Pin = bank,
                            Driver = this,
                            SerialDriver = madLedSerialDriver,
                            DeviceType = DeviceTypes.Fan

                        };

                        ControlDevice.LedUnit[] leds = new ControlDevice.LedUnit[ledCount];
                        for (int i = 0; i < ledCount; i++)
                        {
                            leds[i] = new ControlDevice.LedUnit
                                {LEDName = "LED " + i, Data = new MadLedData {LEDNumber = i}};
                        }

                        dvc.LEDs = leds;

                        results.Add(dvc);
                    }
                }


            }

            return results;
        }

        public void Push(ControlDevice controlDevice)
        {
            foreach (ControlDevice.LedUnit led in controlDevice.LEDs)
            {
                if (((MadLedData) led.Data).SetColor.Red != led.Color.Red ||
                    ((MadLedData) led.Data).SetColor.Green != led.Color.Green ||
                    ((MadLedData) led.Data).SetColor.Blue != led.Color.Blue)
                {
                    ((MadLedDevice)controlDevice).SerialDriver.SetLED(((MadLedDevice) controlDevice).Bank,

                        ((MadLedData) led.Data).LEDNumber,
                        led.Color.Red, led.Color.Green, led.Color.Blue);

                    ((MadLedData)led.Data).SetColor.Red = led.Color.Red;
                    ((MadLedData)led.Data).SetColor.Green = led.Color.Green;
                    ((MadLedData)led.Data).SetColor.Blue = led.Color.Blue;
                }
            }
        }

        public void Pull(ControlDevice controlDevice)
        {
            throw new NotImplementedException();
        }

        public DriverProperties GetProperties()
        {
            return new DriverProperties
            {
                SupportsPull = false,
                SupportsPush = true,
                IsSource = false
            };
        }

        public string Name()
        {
            return "MadLed";
        }

        public void Dispose()
        {
            foreach (var serialDriver in serialDrivers)
            {
                serialDriver?.Dispose();
            }
        }
    }
}
