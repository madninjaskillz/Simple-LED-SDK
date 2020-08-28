using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using MadLedFrameworkSDK;

namespace ICUEDriver
{
    public class CUEDriver : ISimpleLEDDriver
    {

        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets the loaded architecture (x64/x86).
        /// </summary>
        public string LoadedArchitecture => _CUESDK.LoadedArchitecture;

        /// <summary>
        /// Gets the protocol details for the current SDK-connection.
        /// </summary>
        public CorsairProtocolDetails ProtocolDetails { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// Gets whether the application has exclusive access to the SDK or not.
        /// </summary>
        public bool HasExclusiveAccess { get; private set; }

        /// <summary>
        /// Gets the last error documented by CUE.
        /// </summary>
        public CorsairError LastError => _CUESDK.CorsairGetLastError();

        public void Dispose()
        {

        }

        public void Configure(DriverDetails driverDetails)
        {
            _CUESDK.Reload();

            ProtocolDetails = new CorsairProtocolDetails(_CUESDK.CorsairPerformProtocolHandshake());

            CorsairError error = LastError;
            if (error != CorsairError.Success)
                throw new Exception(error.ToString());

            if (ProtocolDetails.BreakingChanges)
                throw new Exception("The SDK currently used isn't compatible with the installed version of CUE.\r\n"
                                    + $"CUE-Version: {ProtocolDetails.ServerVersion} (Protocol {ProtocolDetails.ServerProtocolVersion})\r\n"
                                    + $"SDK-Version: {ProtocolDetails.SdkVersion} (Protocol {ProtocolDetails.SdkProtocolVersion})");


            //if (!_CUESDK.CorsairRequestControl(CorsairAccessMode.ExclusiveLightingControl))
            //{
            //    throw new Exception(LastError.ToString());
            //    HasExclusiveAccess = true;
            //}
            //else
            //{
                HasExclusiveAccess = false;
            //}

            if (!_CUESDK.CorsairSetLayerPriority(127))
            {
                throw new Exception(LastError.ToString());
            }

            // DarthAffe 07.07.2018: 127 is CUE, we want to directly compete with it as in older versions.


        }


        public List<ControlDevice> GetDevices()
        {

            List<ControlDevice> devices = new List<ControlDevice>();
            Dictionary<string, int> modelCounter = new Dictionary<string, int>();

            int deviceCount = _CUESDK.CorsairGetDeviceCount();

            for (int i = 0; i < deviceCount; i++)
            {
                Debug.WriteLine(i);

                var tst = _CUESDK.CorsairGetDeviceInfo(i);
                _CorsairDeviceInfo nativeDeviceInfo = (_CorsairDeviceInfo)Marshal.PtrToStructure(tst, typeof(_CorsairDeviceInfo));
                Debug.WriteLine(nativeDeviceInfo.ledsCount + " leds");
                CorsairRGBDeviceInfo info = new CorsairRGBDeviceInfo(i, DeviceTypes.Other, nativeDeviceInfo, modelCounter);
                if (!info.CapsMask.HasFlag(CorsairDeviceCaps.Lighting))
                {
                    continue; // Everything that doesn't support lighting control is useless
                }

                var nativeLedPositions = (_CorsairLedPositions)Marshal.PtrToStructure(_CUESDK.CorsairGetLedPositionsByDeviceIndex(info.CorsairDeviceIndex), typeof(_CorsairLedPositions));

                int structSize = Marshal.SizeOf(typeof(_CorsairLedPosition));
                IntPtr ptr = nativeLedPositions.pLedPosition;

                List<_CorsairLedPosition> positions = new List<_CorsairLedPosition>();
                for (int ii = 0; ii < nativeLedPositions.numberOfLed; ii++)
                {
                    _CorsairLedPosition ledPosition = (_CorsairLedPosition)Marshal.PtrToStructure(ptr, typeof(_CorsairLedPosition));
                    ptr = new IntPtr(ptr.ToInt64() + structSize);
                    positions.Add(ledPosition);
                }

                Debug.WriteLine(info.CorsairDeviceType);

                CorsairDevice device = new CorsairDevice
                {
                    Driver = this,
                    Name = info.DeviceName,
                    CorsairDeviceIndex = info.CorsairDeviceIndex,
                    DeviceType = GetDeviceType(info.CorsairDeviceType)
                };

                var channelsInfo = (nativeDeviceInfo.channels);

                if (channelsInfo != null)
                {
                    IntPtr channelInfoPtr = channelsInfo.channels;

                    if (channelsInfo.channelsCount > 0)
                    {

                        for (int channel = 0; channel < channelsInfo.channelsCount; channel++)
                        {

                            _CorsairChannelInfo channelInfo = (_CorsairChannelInfo)Marshal.PtrToStructure(channelInfoPtr, typeof(_CorsairChannelInfo));

                            int channelDeviceInfoStructSize = Marshal.SizeOf(typeof(_CorsairChannelDeviceInfo));
                            IntPtr channelDeviceInfoPtr = channelInfo.devices;

                            for (int dev = 0; dev < channelInfo.devicesCount; dev++)
                            {
                                _CorsairChannelDeviceInfo channelDeviceInfo = (_CorsairChannelDeviceInfo)Marshal.PtrToStructure(channelDeviceInfoPtr, typeof(_CorsairChannelDeviceInfo));

                                CorsairLedId channelReferenceLed = GetChannelReferenceId(info.CorsairDeviceType, channel);
                                CorsairLedId referenceLed = channelReferenceLed + (dev * channelDeviceInfo.deviceLedCount);

                                List<ControlDevice.LedUnit> leds = new List<ControlDevice.LedUnit>();

                                string subDeviceName = "Invalid";
                                string subDeviceType = DeviceTypes.Other;

                                switch (channelDeviceInfo.type)
                                {
                                    case CorsairChannelDeviceType.Invalid:
                                        subDeviceName = "Unknown";
                                        subDeviceType = DeviceTypes.Other;
                                        break;
                                    case CorsairChannelDeviceType.FanHD:
                                        subDeviceName = "HD Fan";
                                        subDeviceType = DeviceTypes.Fan;
                                        break;
                                    case CorsairChannelDeviceType.FanSP:
                                        subDeviceName = "SP Fan";
                                        subDeviceType = DeviceTypes.Fan;
                                        break;
                                    case CorsairChannelDeviceType.FanML:
                                        subDeviceName = "ML Fan";
                                        subDeviceType = DeviceTypes.Fan;
                                        break;
                                    case CorsairChannelDeviceType.FanLL:
                                        subDeviceName = "LL Fan";
                                        subDeviceType = DeviceTypes.Fan;
                                        break;
                                    case CorsairChannelDeviceType.Strip:
                                        subDeviceName = "LED Strip";
                                        subDeviceType = DeviceTypes.LedStrip;
                                        break;
                                    case CorsairChannelDeviceType.DAP:
                                        subDeviceName = "Super-Secret DAP thingy";
                                        subDeviceType = DeviceTypes.Other;
                                        break;
                                    case CorsairChannelDeviceType.FanQL:
                                        subDeviceName = "QL Fan";
                                        subDeviceType = DeviceTypes.Fan;
                                        break;
                                    default:
                                         subDeviceName = "Unknown";
                                        break;
                                }

                                CorsairDevice subDevice = new CorsairDevice
                                {
                                    Driver = this,
                                    Name = subDeviceName + " " + (dev + 1).ToString(), //make device id start at 1 not 0 because normal people use this program
                                    CorsairDeviceIndex = info.CorsairDeviceIndex,
                                    DeviceType = subDeviceType
                                };

                                for (int devLed = 0; devLed < channelDeviceInfo.deviceLedCount; devLed++)
                                {


                                    //Fanman's ugly code for LED mapping. Abandon hope all ye who have to troublshoot this dumpster-fire of magic numbers.
                                    CorsairLedId corsairLedId;
                                    if (channelDeviceInfo.deviceLedCount > 30)
                                    {
                                        if ((int)referenceLed > 369 && (int)referenceLed != 350 && (int)referenceLed != 384 && (int)referenceLed != 418 && (int)referenceLed != 452 && (int)referenceLed != 486)
                                        {
                                            corsairLedId = referenceLed + 562 + devLed;
                                        }
                                        else if ((int)referenceLed > 335 && (int)referenceLed != 350 && (int)referenceLed != 384 && (int)referenceLed != 418 && (int)referenceLed != 452 && (int)referenceLed != 486)
                                        {
                                            if (devLed < 14)
                                            {
                                                corsairLedId = referenceLed + devLed;
                                            }
                                            else
                                            {
                                                corsairLedId = referenceLed + 562 + devLed;
                                            }
                                        }
                                        //ch2
                                        else if ((int)referenceLed >= 486)
                                        {
                                            if (devLed < 14)
                                            {
                                                corsairLedId = referenceLed + devLed;
                                            }
                                            else
                                            {
                                                corsairLedId = referenceLed + 562 + devLed;
                                            }
                                        }
                                        else
                                        {
                                            corsairLedId = referenceLed + devLed;
                                        }

                                    } else
                                    {
                                        corsairLedId = referenceLed + devLed;
                                    }
                                     


                                    leds.Add(new ControlDevice.LedUnit()
                                    {
                                        Data = new CorsairLedData
                                        {
                                            LEDNumber = devLed,
                                            CorsairLedId = (int) corsairLedId

                                        },
                                        LEDName = device.Name + " " + devLed  
                                    });
                                }
                                subDevice.LEDs = leds.ToArray();
                                devices.Add(subDevice);
                            }
                        }
                    }
                    else
                    {
                        List<ControlDevice.LedUnit> leds = new List<ControlDevice.LedUnit>();

                        int ctr = 0;
                        foreach (var lp in positions)
                        {
                            leds.Add(new ControlDevice.LedUnit()
                            {
                                Data = new CorsairLedData
                                {
                                    LEDNumber = ctr,
                                    CorsairLedId = lp.LedId

                                },
                                LEDName = device.Name+" "+ ctr
                            });
                        }

                        //for (int l = 0; l < nativeDeviceInfo.ledsCount; l++)
                        //{
                        //    leds.Add(new ControlDevice.LedUnit
                        //    {
                        //        Data = new ControlDevice.LEDData { LEDNumber = l },
                        //        LEDName = "LED " + l,

                        //    });
                        //}

                        device.LEDs = leds.ToArray();

                    }
                    if(info.CorsairDeviceType == CorsairDeviceType.CommanderPro || info.CorsairDeviceType == CorsairDeviceType.LightningNodePro) //filter out pointless devices
                    {
                        continue;
                    }
                    else
                    {
                        devices.Add(device);
                    }
                }

            }

            Debug.WriteLine("Done : " + LastError);
            return devices;
        }

        private string GetDeviceType(CorsairDeviceType t)
        {
            switch (t)
            {
                case CorsairDeviceType.Cooler: return DeviceTypes.Cooler;
                case CorsairDeviceType.CommanderPro: return DeviceTypes.Other;
                case CorsairDeviceType.Headset: return DeviceTypes.Headset;
                case CorsairDeviceType.HeadsetStand: return DeviceTypes.HeadsetStand;
                case CorsairDeviceType.Keyboard: return DeviceTypes.Keyboard;
                case CorsairDeviceType.MemoryModule: return DeviceTypes.Memory;
                case CorsairDeviceType.Motherboard: return DeviceTypes.MotherBoard;
                case CorsairDeviceType.GraphicsCard: return DeviceTypes.GPU;
                case CorsairDeviceType.Unknown:
                    return DeviceTypes.Other;
                case CorsairDeviceType.Mouse:
                    return DeviceTypes.Mouse;
                case CorsairDeviceType.Mousepad:
                    return DeviceTypes.MousePad;
                case CorsairDeviceType.LightningNodePro:
                    return DeviceTypes.Other;
                default:
                    return DeviceTypes.Other;
            }
        }

        private static CorsairLedId GetChannelReferenceId(CorsairDeviceType deviceType, int channel)
        {
            if (deviceType == CorsairDeviceType.Cooler)
                return CorsairLedId.CustomLiquidCoolerChannel1Led1;
            else
            {
                switch (channel)
                {
                    case 0: return CorsairLedId.CustomDeviceChannel1Led1;
                    case 1: return CorsairLedId.CustomDeviceChannel2Led1;
                    case 2: return CorsairLedId.CustomDeviceChannel3Led1;
                }
            }

            return CorsairLedId.Invalid;
        }

        public void Push(ControlDevice controlDevice)
        {
            int deviceIndex = ((CorsairDevice)controlDevice).CorsairDeviceIndex;

            int numberOfLedsToUpdate = controlDevice.LEDs.Length;
            int structSize = Marshal.SizeOf(typeof(_CorsairLedColor));
            int ptrSize = structSize * numberOfLedsToUpdate;
            IntPtr ptr = Marshal.AllocHGlobal(ptrSize);
            IntPtr addPtr = new IntPtr(ptr.ToInt64());


            foreach (var led in controlDevice.LEDs)
            {
                _CorsairLedColor color = new _CorsairLedColor
                {
                    ledId =((CorsairLedData)led.Data).CorsairLedId,
                    r = (byte)led.Color.Red,
                    g = (byte)led.Color.Green,
                    b = (byte)led.Color.Blue
                };

                Marshal.StructureToPtr(color, addPtr, false);
                addPtr = new IntPtr(addPtr.ToInt64() + structSize);

            }


            _CUESDK.CorsairSetLedsColorsBufferByDeviceIndex(deviceIndex, numberOfLedsToUpdate, ptr);

            _CUESDK.CorsairSetLedsColorsFlushBuffer();
            

            Marshal.FreeHGlobal(ptr);
        }

        public void Pull(ControlDevice controlDevice)
        {
            /*
               int structSize = Marshal.SizeOf(typeof(_CorsairLedColor));
               IntPtr ptr = Marshal.AllocHGlobal(structSize * controlDevice.LEDs.Count());
               IntPtr addPtr = new IntPtr(ptr.ToInt64());
               foreach (var led in controlDevice.LEDs)
               {
                   _CorsairLedColor color = new _CorsairLedColor { ledId = (int)(led.Data.LEDNumber) };
                   Marshal.StructureToPtr(color, addPtr, false);
                   addPtr = new IntPtr(addPtr.ToInt64() + structSize);
               }

               _CUESDK.CorsairGetLedsColorsByDeviceIndex(((CorsairDevice)controlDevice).CorsairDeviceIndex, controlDevice.LEDs.Count(), ptr);

               _CorsairLedColorStruct[] derp = MakeArray<_CorsairLedColorStruct>(ptr.ToInt32(), controlDevice.LEDs.Length);

               IntPtr readPtr = ptr;
               for (int i = 0; i < controlDevice.LEDs.Count(); i++)
               {
                   _CorsairLedColor ledColor = (_CorsairLedColor)Marshal.PtrToStructure(readPtr, typeof(_CorsairLedColor));

                   var led= controlDevice.LEDs.FirstOrDefault(x => x.Data is CorsairLedData cled && cled.CorsairLedId == ledColor.ledId);

                   if (led != null)
                   {
                    led.Color = new LEDColor(ledColor.r, ledColor.g, ledColor.b);
                   }

                   readPtr = new IntPtr(readPtr.ToInt64() + structSize);
               }

               Marshal.FreeHGlobal(ptr);*/

            int structSize = Marshal.SizeOf(typeof(_CorsairLedColor));
            IntPtr ptr = Marshal.AllocHGlobal(structSize * controlDevice.LEDs.Count());
            IntPtr addPtr = new IntPtr(ptr.ToInt64());
            foreach (var led in controlDevice.LEDs)
            {
                _CorsairLedColor color = new _CorsairLedColor { ledId = (int)((CorsairLedData)led.Data).CorsairLedId };
                Marshal.StructureToPtr(color, addPtr, false);
                addPtr = new IntPtr(addPtr.ToInt64() + structSize);
            }

            _CUESDK.CorsairGetLedsColorsByDeviceIndex(((CorsairDevice)controlDevice).CorsairDeviceIndex, controlDevice.LEDs.Count(), ptr);

            IntPtr readPtr = ptr;
            for (int i = 0; i < controlDevice.LEDs.Count(); i++)
            {
                _CorsairLedColor ledColor = (_CorsairLedColor)Marshal.PtrToStructure(readPtr, typeof(_CorsairLedColor));

                var setme = controlDevice.LEDs.FirstOrDefault(x =>((CorsairLedData) x.Data).CorsairLedId == ledColor.ledId);
                if (setme != null)
                {
                    setme.Color= new LEDColor(ledColor.r, ledColor.g, ledColor.b);
                }
                
                readPtr = new IntPtr(readPtr.ToInt64() + structSize);
            }

            Marshal.FreeHGlobal(ptr);

        }

        static unsafe T[] MakeArray<T>(int t, int length) where T : struct
        {
            int tSizeInBytes = Marshal.SizeOf(typeof(T));
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                IntPtr p = new IntPtr((byte*)t + (i * tSizeInBytes));
                result[i] = (T)System.Runtime.InteropServices.Marshal.PtrToStructure(p, typeof(T));
            }

            return result;
        }

        public DriverProperties GetProperties()
        {
            return new DriverProperties
            {
                SupportsPull = true,
                SupportsPush = true,
                IsSource = false
            };
        }

        public string Name()
        {
            return "Corsair Driver";
        }

        public class CorsairDevice : ControlDevice
        {
            public int CorsairDeviceIndex { get; set; }
        }

        public class CorsairLedData : ControlDevice.LEDData
        {
            public int CorsairLedId { get; set; }
        }
    }
}
