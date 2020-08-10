using System.Collections.Generic;

namespace MadLedFrameworkSDK
{
    public interface ISimpleLEDDriver
    {
        void Configure(DriverDetails driverDetails);
        List<ControlDevice> GetDevices();
        void Push(ControlDevice controlDevice);
        void Pull(ControlDevice controlDevice);
        DriverProperties GetProperties();
    }

    public class DriverProperties
    {
        public bool SupportsPull { get; set; }
    }
    
    public  class DriverDetails
    {
    public string Name { get; set; }
        public virtual string Platform { get; } = "Unknown";
    }

    public class MadLedDriverDetails: DriverDetails
    {
        public override string Platform { get; } = "MadLed";
        public string ComPort { get; set; }
    }
}
