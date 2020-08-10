using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MadLedFrameworkSDK;

namespace MadLedSLSDriver
{
    public class MadLedDevice : ControlDevice
    {
        public override string DeviceType { get; } = "MadLed LED";
        public int Bank { get; set; }
        public int Pin { get; set; }
    }

    public class MadLedData
    {
        public int LedNumber { get; set; }
    }
}
