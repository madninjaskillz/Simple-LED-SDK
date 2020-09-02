using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MadLedFrameworkSDK;

namespace MSIProvider
{
    public class MSIDriver : ISimpleLEDDriver
    {
        public string Name()
        {
            return "MSI";
        }
        public MSIDriver()
        {
            _MsiSDK.Reload();

            var errorCode = _MsiSDK.Initialize();
        }
        public void Dispose()
        {
            _MsiSDK.UnloadMsiSDK();
        }

        public void Configure(DriverDetails driverDetails)
        {

        }

        public List<ControlDevice> GetDevices()
        {
            Bitmap gpu;

            Assembly myAssembly = Assembly.GetExecutingAssembly();
            using (Stream myStream = myAssembly.GetManifestResourceStream("MSIProvider.msi_gpu.png"))
            {
                gpu = (Bitmap)Image.FromStream(myStream);
            }

            List<ControlDevice> returnValue = new List<ControlDevice>();
            int tmp;

            try
            {
                tmp = _MsiSDK.GetDeviceInfo(out string[] deviceTypes, out int[] ledCounts);
                
                if (tmp != 0 && deviceTypes!=null)
                {

                    for (int i = 0; i < deviceTypes.Length; i++)
                    {
                        try
                        {
                            string deviceType = deviceTypes[i];
                            int ledCount = ledCounts[i];

                            if (deviceType.Equals("MSI_MB"))
                            {
                                var mbdeivce = new MSIControlDevice
                                {
                                    Driver = this,
                                    Name = "MSI Motherboard",
                                    LEDs = new ControlDevice.LedUnit[ledCount],
                                    MSIDeviceType = deviceType,
                                    DeviceType = DeviceTypes.MotherBoard
                                };

                                for (int l = 0; l < ledCount; l++)
                                {
                                    mbdeivce.LEDs[l] = new ControlDevice.LedUnit
                                    {
                                        Data = new ControlDevice.LEDData() { LEDNumber = l },
                                        Color = new LEDColor(0, 0, 0),
                                        LEDName = "Motherboard LED " + l,
                                    };
                                }

                                returnValue.Add(mbdeivce);
                            }
                            else if (deviceType.Equals("MSI_VGA"))
                            {

                                var gpuDevice = new MSIControlDevice
                                {
                                    Driver = this,
                                    Name = "MSI GPU",
                                    LEDs = new ControlDevice.LedUnit[ledCount],
                                    MSIDeviceType = deviceType,
                                    DeviceType = DeviceTypes.GPU,
                                    ProductImage = gpu
                                };

                                for (int l = 0; l < ledCount; l++)
                                {
                                    gpuDevice.LEDs[l] = new ControlDevice.LedUnit
                                    {
                                        Data = new ControlDevice.LEDData { LEDNumber = l },
                                        Color = new LEDColor(0, 0, 0),
                                        LEDName = "GPU LED " + l
                                    };
                                }

                                returnValue.Add(gpuDevice);
                            }
                        }
                        catch
                        {

                        }
                    }

                }

            }
            catch

            {
            }

            return returnValue;
        }

        public void Push(ControlDevice controlDevice)
        {
            MSIControlDevice msiDevice = controlDevice as MSIControlDevice;


            for (int i = 0; i < controlDevice.LEDs.Length; i++)
            {
                _MsiSDK.SetLedStyle(msiDevice.MSIDeviceType, i, "Steady");
                _MsiSDK.SetLedColor(msiDevice.MSIDeviceType, i, controlDevice.LEDs[i].Color.Red,
                    controlDevice.LEDs[i].Color.Green, controlDevice.LEDs[i].Color.Blue);
            }
        }

        public void Pull(ControlDevice controlDevice)
        {
            
        }

        public DriverProperties GetProperties()
        {
            return new DriverProperties
            {
                SupportsPull = false,
                SupportsPush = true,
                IsSource = false
            };
        }
    }
}
