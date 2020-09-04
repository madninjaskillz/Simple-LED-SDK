﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MadLedFrameworkSDK;

namespace MadLedSLSDriver
{
    public class MadLedDriver : ISimpleLEDDriverWithConfig
    {

        public T GetConfig<T>() where T : SLSConfigData
        {
            //TODO throw new NotImplementedException();
            return null;
        }

        public void PutConfig<T>(T config) where T : SLSConfigData
        {
            //TODO throw new NotImplementedException();
        }

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
                            DeviceType = DeviceTypes.Fan,
                            ProductImage = Assembly.GetExecutingAssembly().GetEmbeddedImage("MadLedSLSDriver.madLedImage.png")

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
                IsSource = false,
                Id = Guid.Parse("79440d02-8ca3-4e35-a9a3-88b024cc0e2d"),
                Author = "Mad Ninja",
                CurrentVersion = new ReleaseNumber(3,0,0,0),
                GitHubLink = "https://github.com/madninjaskillz/MadLed",
                Blurb = "Simple LED control for multiple 3 pin RGB fans based on WS281X for ESP8266 boards",
                IsPublicRelease = false,
                SupportsCustomConfig = true
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

        public UserControl GetCustomConfig(ControlDevice controlDevice)
        {
            return null;
        }

        public bool GetIsDirty()
        {
            return false;
        }

        public void SetIsDirty(bool val)
        {
            
        }
    }
}
