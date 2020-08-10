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
                        new ControlDevice.LedUnit{ LEDName = "LED 0", Data = new MadLedData{LedNumber=0}},
                        new ControlDevice.LedUnit{ LEDName = "LED 1", Data = new MadLedData{LedNumber=1}},
                        new ControlDevice.LedUnit{ LEDName = "LED 2", Data = new MadLedData{LedNumber=2}},
                        new ControlDevice.LedUnit{ LEDName = "LED 3", Data = new MadLedData{LedNumber=3}},
                        new ControlDevice.LedUnit{ LEDName = "LED 4", Data = new MadLedData{LedNumber=4}},
                        new ControlDevice.LedUnit{ LEDName = "LED 5", Data = new MadLedData{LedNumber=5}},
                        new ControlDevice.LedUnit{ LEDName = "LED 6", Data = new MadLedData{LedNumber=6}},
                        new ControlDevice.LedUnit{ LEDName = "LED 7", Data = new MadLedData{LedNumber=7}},
                        new ControlDevice.LedUnit{ LEDName = "LED 8", Data = new MadLedData{LedNumber=8}},
                        new ControlDevice.LedUnit{ LEDName = "LED 9", Data = new MadLedData{LedNumber=9}},
                        new ControlDevice.LedUnit{ LEDName = "LED 10", Data = new MadLedData{LedNumber=10}},
                        new ControlDevice.LedUnit{ LEDName = "LED 11", Data = new MadLedData{LedNumber=11}},
                        new ControlDevice.LedUnit{ LEDName = "LED 12", Data = new MadLedData{LedNumber=12}},
                        new ControlDevice.LedUnit{ LEDName = "LED 13", Data = new MadLedData{LedNumber=13}},
                        new ControlDevice.LedUnit{ LEDName = "LED 14", Data = new MadLedData{LedNumber=14}},
                        new ControlDevice.LedUnit{ LEDName = "LED 15", Data = new MadLedData{LedNumber=15}},
                        new ControlDevice.LedUnit{ LEDName = "LED 16", Data = new MadLedData{LedNumber=16}},
                        new ControlDevice.LedUnit{ LEDName = "LED 17", Data = new MadLedData{LedNumber=17}},
                        new ControlDevice.LedUnit{ LEDName = "LED 18", Data = new MadLedData{LedNumber=18}},
                        new ControlDevice.LedUnit{ LEDName = "LED 19", Data = new MadLedData{LedNumber=19}},
                        new ControlDevice.LedUnit{ LEDName = "LED 20", Data = new MadLedData{LedNumber=20}},
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
                        new ControlDevice.LedUnit{ LEDName = "LED 0", Data = new MadLedData{LedNumber=0}},
                        new ControlDevice.LedUnit{ LEDName = "LED 1", Data = new MadLedData{LedNumber=1}},
                        new ControlDevice.LedUnit{ LEDName = "LED 2", Data = new MadLedData{LedNumber=2}},
                        new ControlDevice.LedUnit{ LEDName = "LED 3", Data = new MadLedData{LedNumber=3}},
                        new ControlDevice.LedUnit{ LEDName = "LED 4", Data = new MadLedData{LedNumber=4}},
                        new ControlDevice.LedUnit{ LEDName = "LED 5", Data = new MadLedData{LedNumber=5}},
                        new ControlDevice.LedUnit{ LEDName = "LED 6", Data = new MadLedData{LedNumber=6}},
                        new ControlDevice.LedUnit{ LEDName = "LED 7", Data = new MadLedData{LedNumber=7}},
                        new ControlDevice.LedUnit{ LEDName = "LED 8", Data = new MadLedData{LedNumber=8}},
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
            foreach (ControlDevice.LedUnit led in controlDevice.LEDs)
            {
                ((MadLedDriver)controlDevice.Driver).serialDriver.SetLED(((MadLedDevice)controlDevice).Bank, int.Parse(led.LEDName.Split(' ').Last()), led.Color.Red, led.Color.Green, led.Color.Blue);
            }
            var groupedByColor = controlDevice.LEDs.GroupBy(x => x.Color.AsString());

            foreach (var group in groupedByColor)
            {
                List<Tuple<int, int?>> batches = new List<Tuple<int, int?>>();

                int r = group.First().Color.Red;
                int g = group.First().Color.Green;
                int b = group.First().Color.Blue;

                int lowest = group.Min(x => (x.Data as MadLedData).LedNumber);
                int highest = group.Max(x => (x.Data as MadLedData).LedNumber);

                int groupStart = lowest;
                int ptr = lowest;

                int start = lowest;
                int end = 0;
                for (int i = lowest; i <= highest + 1; i++)
                {
                    if (group.Any(x => (x.Data as MadLedData).LedNumber == i))
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
