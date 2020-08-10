using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadLedFrameworkSDK
{
    public class SLSManager
    {
        public List<ISimpleLEDDriver> Drivers = new List<ISimpleLEDDriver>();

        public SLSManager()
        {
            
        }

        public List<ControlDevice> GetDevices()
        {
            List<ControlDevice> controlDevices = new List<ControlDevice>();
            foreach (var simpleLedDriver in Drivers)
            {
                controlDevices.AddRange(simpleLedDriver.GetDevices());
            }

            return controlDevices;
        }

    }
}
