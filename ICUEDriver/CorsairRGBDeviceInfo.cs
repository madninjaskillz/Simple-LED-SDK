using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ICUEDriver
{
    public class CorsairRGBDeviceInfo
    {


        /// <summary>
        /// Gets the corsair specific device type.
        /// </summary>
        public CorsairDeviceType CorsairDeviceType { get; }

        /// <summary>
        /// Gets the index of the <see cref="CorsairRGBDevice{TDeviceInfo}"/>.
        /// </summary>
        public int CorsairDeviceIndex { get; }

        /// <inheritdoc />
        public string DeviceType { get; }

        /// <inheritdoc />
        public string DeviceName { get; }

        /// <inheritdoc />
        public string Manufacturer => "Corsair";

        /// <inheritdoc />
        public string Model { get; }

        /// <inheritdoc />
        public Uri Image { get; set; }

        /// <inheritdoc />
        public bool SupportsSyncBack => true;

        /// <inheritdoc />
        public RGBDeviceLighting Lighting => RGBDeviceLighting.Key;

        /// <summary>
        /// Gets a flag that describes device capabilities. (<see cref="CorsairDeviceCaps" />)
        /// </summary>
        public CorsairDeviceCaps CapsMask { get; }


        /// <summary>
        /// Internal constructor of managed <see cref="CorsairRGBDeviceInfo"/>.
        /// </summary>
        /// <param name="deviceIndex">The index of the <see cref="CorsairRGBDevice{TDeviceInfo}"/>.</param>
        /// <param name="deviceType">The type of the <see cref="IRGBDevice"/>.</param>
        /// <param name="nativeInfo">The native <see cref="_CorsairDeviceInfo" />-struct</param>
        /// <param name="modelCounter">A dictionary containing counters to create unique names for equal devices models.</param>
        internal CorsairRGBDeviceInfo(int deviceIndex, string deviceType, _CorsairDeviceInfo nativeInfo, Dictionary<string, int> modelCounter)
        {
            this.CorsairDeviceIndex = deviceIndex;
            this.DeviceType = deviceType;
            this.CorsairDeviceType = nativeInfo.type;
            this.Model = nativeInfo.model == IntPtr.Zero
                ? null
                : Regex.Replace(Marshal.PtrToStringAnsi(nativeInfo.model) ?? string.Empty, " ?DEMO", string.Empty,
                    RegexOptions.IgnoreCase);
            this.CapsMask = (CorsairDeviceCaps) nativeInfo.capsMask;

            DeviceName = GetUniqueModelName(modelCounter);
        }

        internal CorsairRGBDeviceInfo(int deviceIndex, string deviceType, _CorsairDeviceInfo nativeInfo, string modelName, Dictionary<string, int> modelCounter)
        {
            this.CorsairDeviceIndex = deviceIndex;
            this.DeviceType = deviceType;
            this.CorsairDeviceType = nativeInfo.type;
            this.Model = modelName;
            this.CapsMask = (CorsairDeviceCaps)nativeInfo.capsMask;

            DeviceName = GetUniqueModelName(modelCounter);
        }


        private string GetUniqueModelName(Dictionary<string, int> modelCounter)
        {
            if (modelCounter.TryGetValue(Model, out int counter))
            {
                counter = ++modelCounter[Model];
                return $"{Manufacturer} {Model} {counter}";
            }
            else
            {
                modelCounter.Add(Model, 1);
                return $"{Manufacturer} {Model}";
            }
        }
    }

    public enum RGBDeviceLighting
    {
        /// <summary>
        /// The <see cref="IRGBDevice"/> doesn't support lighting,
        /// </summary>
        None = 0,

        /// <summary>
        /// The <see cref="IRGBDevice"/> supports per-key-lightning.
        /// </summary>
        Key = 1,

        /// <summary>
        /// The <see cref="IRGBDevice"/> supports per-device-lightning.
        /// </summary>
        Device = 2,
    }

    [Flags]
    public enum CorsairDeviceCaps
    {
        /// <summary>
        /// For devices that do not support any SDK functions.
        /// </summary>
        None = 0,

        /// <summary>
        /// For devices that has controlled lighting.
        /// </summary>
        Lighting = 1,

        /// <summary>
        /// For devices that provide current state through set of properties.
        /// </summary>
        PropertyLookup = 2
    };

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// CUE-SDK: contains information about separate channel of the DIY-device.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class _CorsairChannelInfo
    {
        /// <summary>
        /// CUE-SDK: total number of LEDs connected to the channel;
        /// </summary>
        internal int totalLedsCount;

        /// <summary>
        /// CUE-SDK: number of LED-devices (fans, strips, etc.) connected to the channel which is controlled by the DIY device
        /// </summary>
        internal int devicesCount;

        /// <summary>
        /// CUE-SDK: array containing information about each separate LED-device connected to the channel controlled by the DIY device.
        /// Index of the LED-device in array is same as the index of the LED-device connected to the DIY-device.
        /// </summary>
        internal IntPtr devices;
    }

    // ReSharper disable once InconsistentNaming    
    /// <summary>
    /// CUE-SDK: contains information about led and its color
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class _CorsairLedColor
    {
        /// <summary>
        /// CUE-SDK: identifier of LED to set
        /// </summary>
        internal int ledId;

        /// <summary>
        /// CUE-SDK: red   brightness[0..255]
        /// </summary>
        internal int r;

        /// <summary>
        /// CUE-SDK: green brightness[0..255]
        /// </summary>
        internal int g;

        /// <summary>
        /// CUE-SDK: blue  brightness[0..255]
        /// </summary>
        internal int b;
    };


}
