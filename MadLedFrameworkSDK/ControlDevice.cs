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

        public void MapLEDs(ControlDevice otherDevice)
        {
            float ratio = (float)otherDevice.LEDs.Length/LEDs.Length;

            Debug.WriteLine(ratio);

            for (int i = 0; i < LEDs.Length; i++)
            {
                int index = ((int) Math.Floor(i * ratio));
                var copyLED = otherDevice.LEDs[index];

                LEDs[i].Color.Red = copyLED.Color.Red;
                LEDs[i].Color.Green = copyLED.Color.Green;
                LEDs[i].Color.Blue = copyLED.Color.Blue;
            }

        }

        public class LedUnit
        {
            public string LEDName { get; set; }
            public LEDColor Color { get; set; } = new LEDColor(0,0,0);
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
