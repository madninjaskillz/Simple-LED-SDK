using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MadLedFrameworkSDK;

namespace MadLedSLSDriver
{
    public class MadLedDriver : ISimpleLEDDriver
    {
        
        private MadLedSerialDriver serialDriver;

        public void Configure(DriverDetails driverDetails)
        {
            MadLedDriverDetails specificDriverDetails = driverDetails as MadLedDriverDetails;

            if (specificDriverDetails == null)
            {
                throw new ArgumentException();
            }

            serialDriver = new MadLedSerialDriver(specificDriverDetails.ComPort);

            //old init stuff, remove once arduino code reworked.
            serialDriver.AddDevice("Top Front", 1, 4, 21);
            serialDriver.AddDevice("Bottom Front", 0, 3, 21);
        }

        public List<ControlDevice> GetDevices()
        {
            //this is faked to hell till i update the arduino code - this should be read from device via serial driver.

            return new List<ControlDevice>
            {
                new MadLedDevice
                {
                    Bank = 0,
                    Name = "Top Front",
                    Driver = this,
                    Pin = 3,
                    LEDs = new ControlDevice.LedUnit[]
                    {
                        new ControlDevice.LedUnit{ LEDName = "LED 0"},
                        new ControlDevice.LedUnit{ LEDName = "LED 1"},
                        new ControlDevice.LedUnit{ LEDName = "LED 2"},
                        new ControlDevice.LedUnit{ LEDName = "LED 3"},
                        new ControlDevice.LedUnit{ LEDName = "LED 4"},
                        new ControlDevice.LedUnit{ LEDName = "LED 5"},
                        new ControlDevice.LedUnit{ LEDName = "LED 6"},
                        new ControlDevice.LedUnit{ LEDName = "LED 7"},
                        new ControlDevice.LedUnit{ LEDName = "LED 8"},
                        new ControlDevice.LedUnit{ LEDName = "LED 9"},
                        new ControlDevice.LedUnit{ LEDName = "LED 10"},
                        new ControlDevice.LedUnit{ LEDName = "LED 11"},
                        new ControlDevice.LedUnit{ LEDName = "LED 12"},
                        new ControlDevice.LedUnit{ LEDName = "LED 13"},
                        new ControlDevice.LedUnit{ LEDName = "LED 14"},
                        new ControlDevice.LedUnit{ LEDName = "LED 15"},
                        new ControlDevice.LedUnit{ LEDName = "LED 16"},
                        new ControlDevice.LedUnit{ LEDName = "LED 17"},
                        new ControlDevice.LedUnit{ LEDName = "LED 18"},
                        new ControlDevice.LedUnit{ LEDName = "LED 19"},
                        new ControlDevice.LedUnit{ LEDName = "LED 20"},
                    }
                },
                new MadLedDevice
                {
                    Bank = 1,
                    Name = "Bottom Front",
                    Pin = 4,
                    Driver = this,
                    LEDs = new ControlDevice.LedUnit[]
                    {
                        new ControlDevice.LedUnit{ LEDName = "LED 0"},
                        new ControlDevice.LedUnit{ LEDName = "LED 1"},
                        new ControlDevice.LedUnit{ LEDName = "LED 2"},
                        new ControlDevice.LedUnit{ LEDName = "LED 3"},
                        new ControlDevice.LedUnit{ LEDName = "LED 4"},
                        new ControlDevice.LedUnit{ LEDName = "LED 5"},
                        new ControlDevice.LedUnit{ LEDName = "LED 6"},
                        new ControlDevice.LedUnit{ LEDName = "LED 7"},
                        new ControlDevice.LedUnit{ LEDName = "LED 8"},
                        //new ControlDevice.LedUnit{ LEDName = "LED 9"},
                        //new ControlDevice.LedUnit{ LEDName = "LED 10"},
                        //new ControlDevice.LedUnit{ LEDName = "LED 11"},
                        //new ControlDevice.LedUnit{ LEDName = "LED 12"},
                        //new ControlDevice.LedUnit{ LEDName = "LED 13"},
                        //new ControlDevice.LedUnit{ LEDName = "LED 14"},
                        //new ControlDevice.LedUnit{ LEDName = "LED 15"},
                        //new ControlDevice.LedUnit{ LEDName = "LED 16"},
                        //new ControlDevice.LedUnit{ LEDName = "LED 17"},
                        //new ControlDevice.LedUnit{ LEDName = "LED 18"},
                        //new ControlDevice.LedUnit{ LEDName = "LED 19"},
                        //new ControlDevice.LedUnit{ LEDName = "LED 20"},
                    }
                }
            };
        }

        public void Push(ControlDevice controlDevice)
        {
            var groupedByColor = controlDevice.LEDs.GroupBy(x => x.Color.AsString());

            foreach (var group in groupedByColor)
            {
                List<Tuple<int, int?>> batches = new List<Tuple<int, int?>>();

                int r = group.First().Color.Red;
                int g = group.First().Color.Green;
                int b = group.First().Color.Blue;

                int lowest = group.Min(x => int.Parse(x.LEDName.Split(' ').Last()));
                int highest = group.Max(x => int.Parse(x.LEDName.Split(' ').Last()));

                int groupStart = lowest;
                int ptr = lowest;

                int start = lowest;
                int end = 0;
                for (int i = lowest; i <= highest + 1; i++)
                {
                    if (group.Any(x => x.LEDName == "LED " + i))
                    {
                        if (start > -1)
                        {
                            end = i;
                        }
                        else
                        {
                            start = i;
                            end = 0;
                        }
                    }
                    else
                    {
                        if (start > -1)
                        {
                            end = i - 1;
                            if (end - start > 1)
                            {
                                batches.Add(new Tuple<int, int?>(start, end));
                            }
                            else
                            {
                                batches.Add(new Tuple<int, int?>(start, null));
                            }

                            start = -1;
                            end = 0;
                        }
                    }

                }

                foreach (var batch in batches)
                {
                    if (batch.Item2 != null)
                    {
                        ((MadLedDriver)controlDevice.Driver).serialDriver.BatchSetLED(((MadLedDevice)controlDevice).Bank, batch.Item1, batch.Item2.Value, r, g, b);
                    }
                    else
                    {
                        ((MadLedDriver)controlDevice.Driver).serialDriver.SetLED(((MadLedDevice)controlDevice).Bank, batch.Item1, r, g, b);
                    }
                }

            }

            ((MadLedDriver)controlDevice.Driver).serialDriver.Present();

        }

        public void Pull(ControlDevice controlDevice)
        {
            throw new NotImplementedException();
        }

        public DriverProperties GetProperties()
        {
            return new DriverProperties
            {
                SupportsPull = false
            };
        }
    }
}
