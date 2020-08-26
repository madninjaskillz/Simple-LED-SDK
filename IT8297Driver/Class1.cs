using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidApiAdapter;

using MadLedFrameworkSDK;

namespace IT8297Driver
{
    public class IT8296Provider : ISimpleLEDDriver
    {
        private const int VENDOR_ID = 0x048D;
        public void Dispose()
        {
            
        }

        public void Configure(DriverDetails driverDetails)
        {
            //var otherdevices = HIDDevice.getConnectedDevices().Where(x=>x.VID == (ushort)VENDOR_ID).ToList();

            //foreach (var interfaceDetailse in otherdevices)
            //{
            //    HIDDevice device = new HIDDevice(interfaceDetailse.devicePath, false);

            //    Debug.WriteLine(device);
            //}


            //var devices = DeviceList.Local.GetHidDevices(VENDOR_ID);
            //Debug.WriteLine(devices);

            //foreach (var hidDevice in devices)
            //{
            //    var fr = hidDevice.GetMaxFeatureReportLength();

            //}

            var manager= HidDeviceManager.GetManager();
            var devices = manager.SearchDevices(0, 0);

            var founddevices = devices.Where(x => (int)x.VendorId == (int)VENDOR_ID).ToList();

            if (founddevices.Any())
            {
                foreach (var device in founddevices)
                {
                    device.Connect();
                    if (device.IsConnected)
                    {
                        ShowDeviceInfo(device);
                    }

                    device.Disconnect();
                }
            }
            else
            {
                Console.WriteLine("no devices found");
            }

            Debug.WriteLine(devices);
        }

        private static void ShowDeviceInfo(HidDevice device)
        {
            Console.WriteLine(
                $"device: {device.Path()}\n" +
                $"manufacturer: {device.Manufacturer()}\n" +
                $"product: {device.Product()}\n" +
                $"serial number: {device.SerialNumber()}\n");
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
            return "IT8297 Provider";
        }


    }
}
