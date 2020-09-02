using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MadLedFrameworkSDK;

namespace CorsairH100iProvider
{
    public class CorsairH100iProvider : ISimpleLEDDriver
    {
        public void Dispose()
        {
            
        }

        public void Configure(DriverDetails driverDetails)
        {
            
        }

        public List<ControlDevice> GetDevices()
        {
            return null;
        }

        public void Push(ControlDevice controlDevice)
        {
            
        }

        public void Pull(ControlDevice controlDevice)
        {
            
        }

        public DriverProperties GetProperties()
        {
            return null;
        }

        public string Name()
        {
            return "Corsair Hydro";
        }
    }
}
