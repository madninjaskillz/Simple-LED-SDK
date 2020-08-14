using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadLedFrameworkSDK
{
    public class ControlDevice
    {
        public string Name { get; set; }
        public virtual string DeviceType { get; }
        public ISimpleLEDDriver Driver { get; set; }
        public LedUnit[] LEDs { get; set; }
        public int LedShift { get; set; } = 0;
        public bool Reverse { get; set; } = false;
        public void MapLEDs(ControlDevice otherDevice)
        {
            
            for (var i = 0; i < LEDs.Length; i++)
            {
                int x = MapIndex(i);
                var doubleIndex1 = (double)i * otherDevice.LEDs.Length / LEDs.Length;
                var index1 = (int)Math.Floor(doubleIndex1);
                var rel = doubleIndex1 - index1;

                LEDs[x].Color.Red = (int)Math.Round((1.0 - rel) * otherDevice.LEDs[index1].Color.Red + (index1 + 1 < otherDevice.LEDs.Length ? rel * otherDevice.LEDs[index1 + 1].Color.Red : 0));
                LEDs[x].Color.Green = (int)Math.Round((1.0 - rel) * otherDevice.LEDs[index1].Color.Green + (index1 + 1 < otherDevice.LEDs.Length ? rel * otherDevice.LEDs[index1 + 1].Color.Green : 0));
                LEDs[x].Color.Blue = (int)Math.Round((1.0 - rel) * otherDevice.LEDs[index1].Color.Blue + (index1 + 1 < otherDevice.LEDs.Length ? rel * otherDevice.LEDs[index1 + 1].Color.Blue : 0));
            }
        }

        public int MapIndex(int index)
        {
            int result = (index + LedShift) % LEDs.Length;
            if (Reverse) result = (LEDs.Length-1) - result;

            return result;
        }

        public LedUnit GetLEDByName(string name)
        {
            return LEDs.FirstOrDefault(x => x.LEDName == name);
        }

        public class LedUnit
        {
            public string LEDName { get; set; }
            public LEDColor Color { get; set; } = new LEDColor(0,0,0);
            public object Data { get; set; }

            public override string ToString()
            {
                return $"{LEDName} : {Color}";
            }
        }
        public class LEDColor{
            public int Red { get; set; }
            public int Green { get; set; }
            public int Blue { get; set; }

            public string AsString() => Red + "," + Green + "," + Blue;

            public LEDColor(int r, int g, int b)
            {
                Red = r;
                Green = g;
                Blue = b;
            }

            public override string ToString()
            {
                return $"{Red},{Green},{Blue}";
            }
        }

        public void Push()
        {
            Driver.Push(this);
        }

        public void Pull()
        {
            Driver.Pull(this);
        }
    }

   
}
