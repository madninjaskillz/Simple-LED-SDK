﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MadLedFrameworkSDK;

namespace FanClock
{
    public class FanClockDriver : ISimpleLEDDriver
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

        private const int LEDCount = 60;
        private Timer timer;
        private int currentPos = 0;
        ControlDevice.LedUnit[] leds = new ControlDevice.LedUnit[60];
        private int dly = 0;

        public FanClockDriver()
        {
            for (int i = 0; i < LEDCount; i++)
            {
                leds[i] = new ControlDevice.LedUnit
                {
                    LEDName = "LED " + i,
                    Data = new ControlDevice.LEDData
                    {
                        LEDNumber = i,
                    },
                };

               
            }

            timer = new Timer(TimerCallback, null, 0, 1000);
        }

        private void TimerCallback(object state)
        {
            for (int i = 0; i < LEDCount; i++)
            {
               

                leds[i].Color = new LEDColor(0,0,0);
            }

            var time = DateTime.Now;

            leds[(time.Second +30) % 60].Color = new LEDColor(255,0,0);
            leds[(time.Minute + 30) % 60].Color.Green=255;
            leds[((int)(((time.Hour % 12)/12f)*60) + 30) % 60].Color.Blue=255;
            //Debug.WriteLine((int)(((time.Hour % 12) / 12f) * 60));
            //Debug.WriteLine(time.Minute);
            //Debug.WriteLine(time.Second);
            //Debug.WriteLine("----");

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
                    Name = "Fan Clock",
                    Driver = this,
                    LEDs = leds,
                    DeviceType = DeviceTypes.Effect,
                    ProductImage = Assembly.GetExecutingAssembly().GetEmbeddedImage("FanClock.clock.png")
                },
                
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
                SupportsPull = false,
                SupportsPush = false,
                IsSource = true,
                Id = Guid.Parse("69440d02-8ca3-4e35-a9a3-88b024cc0e2d"),
                Author = "Mad Ninja",
                Blurb = "Use your RGB Fans as an interesting analog clock",
                IsPublicRelease = false,
                CurrentVersion = new ReleaseNumber(0,0,0,1),
                SupportsCustomConfig = false
            };
        }

        public string Name() => "FanClock";


        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
