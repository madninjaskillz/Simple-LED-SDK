using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                try
                {
                    var devices = simpleLedDriver.GetDevices();
                    if (devices != null)
                    {
                        controlDevices.AddRange(devices);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            return controlDevices;
        }

        public void Init()
        {
            foreach (var simpleLedDriver in Drivers)
            {
                simpleLedDriver.Configure(null);
            }
        }

    }
}
