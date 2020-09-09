using System.Collections.Generic;
using System.Linq;
using System;
using HidSharp;
using System.Diagnostics;
using MadLedFrameworkSDK;

namespace AMDWraith
{

    public enum LedChannel
    {
        R_STATIC = 0,
        R_BREATHE = 1,
        R_CYCLE = 2,
        LOGO = 5,
        FAN = 6,
        R_RAINBOW = 7,
        R_SWIRL = 10,
        OFF = 254,
    }

    public enum LedMode
    {
        OFF = 0,
        STATIC = 1,
        CYCLE = 2,
        BREATHE = 3,
        R_RAINBOW = 5,
        R_SWIRL = 74,
        R_DEFAULT = 255,
    }

    public static class CMRGBExtensions
    {
        public static bool SetFrame(this CMRGBController cmrgb, LEDColor[] leds)
        {
            LEDColor[] lowres = new LEDColor[leds.Length];
            int lrf = 64;
            for (int i = 0; i < leds.Length; i++)
            {
                lowres[i] = new LEDColor((byte)(leds[i].Red / lrf), (byte)(leds[i].Green / lrf), (byte)(leds[i].Blue / lrf));
            }

            List<LEDColor> uniqueColours = new List<LEDColor>();
            List<LEDColor> uniqueColoursLowRed = new List<LEDColor>();
    
            for (int i = 0; i < leds.Length; i++)
            {
                if (lowres[i].Red > 0 || lowres[i].Green > 0 || lowres[i].Blue > 0)
                {
                    if (uniqueColoursLowRed.All(x =>
                        x.Red != lowres[i].Red || x.Blue != lowres[i].Blue || x.Green != lowres[i].Blue))
                    {
                        uniqueColoursLowRed.Add(lowres[i]);
                        uniqueColours.Add(leds[i]);
                    }
                }
            }

            var ring_leds = new LedChannel[15];
            bool resetRequired = true;

            List<LedChannel> chnList = new List<LedChannel> { LedChannel.R_STATIC};//, LedChannel.R_BREATHE, LedChannel.R_SWIRL };//, LedChannel.R_BREATHE };
            List<LedMode> modeList = new List<LedMode> { LedMode.STATIC};//, LedMode.BREATHE , LedMode.R_SWIRL};//, LedMode.BREATHE };

            int chnListIndex = 0;
            for (int u=0;u<uniqueColours.Count;u++)
            {
                LEDColor uc = uniqueColours[u];
                LEDColor ucl = uniqueColoursLowRed[u];

                if (resetRequired)
                {
                    for (int x = 0; x < 15; x++)
                    {
                        ring_leds[x] = LedChannel.OFF;
                    }

                    resetRequired = false;
                }

                int speed = 1;
                if (chnList[chnListIndex] == LedChannel.R_SWIRL) speed = 255;
                cmrgb.set_channel(chnList[chnListIndex], modeList[chnListIndex], 255, (byte)uc.Red, (byte)uc.Green, (byte)uc.Blue,(byte)speed);
                for (int i = 0; i < leds.Length; i++)
                {
                    if (lowres[i].Red == ucl.Red && lowres[i].Green == ucl.Green && lowres[i].Blue == ucl.Blue)
                    {
                        ring_leds[i] = chnList[chnListIndex];
                    }
                }


                chnListIndex++;

                if (chnListIndex == chnList.Count || u+1 == uniqueColours.Count)
                {
                    chnListIndex = 0;
                    resetRequired = true;
                    cmrgb.assign_leds_to_channels(AMDWraith.LedChannel.OFF, AMDWraith.LedChannel.OFF, ring_leds);
                }
                
            }

            return !(uniqueColours.Count <= chnList.Count);
        }

        public static string DecodeString(this byte[] reply)
        {
            var fv = new byte[16];
            var i = 0;
            while (i < 16)
            {
                if (reply[i + 9] != 0)
                {
                    fv[(i / 2)] = (byte)(fv[(i / 2)] + reply[i + 0x09]);
                }
                else
                {
                    break;
                }
                i += 2;
            }

            var rplytxt = System.Text.Encoding.UTF8.GetString(fv);
            return rplytxt;
        }

        public static string PrettyBytes(this byte[] reply)
        {
            string r = "";
            for (int i = 0; i < reply.Length; i++)
            {
                r = r + reply[i].ToString("X2") + " ";
            }

            return r.Trim();
        }
    }
    public class CMRGBController
    {
        HidDevice device = null;
        HidStream stream = null;
        public static byte[] new_packet(params byte[] pms)
        {
            byte[] buf = new byte[64];
            Buffer.BlockCopy(pms, 0, buf, 0, pms.Length);
            return buf;
        }

        public static byte[] new_ffpacket(params byte[] pms)
        {
            byte[] buf = new byte[64];
            for (int i = 0; i < 64; i++)
            {
                buf[i] = 0xff;
            }

            Buffer.BlockCopy(pms, 0, buf, 0, pms.Length);
            return buf;
        }

        public int VENDOR_ID = 9494;

        public int PRODUCT_ID = 81;

        public string PRODUCT_STR = "CYRM02p0303h00E0r0100";

        public int IFACE_NUM = 1;

        public static object new_packet(byte fill, params object[] args)
        {
            byte[] pkt = new byte[65];
            for (int i = 0; i < 65; i++)
            {
                pkt[i] = fill;
            }

            pkt[0] = 0;
            foreach (var _tup_1 in args.Select((_p_1, _p_2) => Tuple.Create(_p_2, _p_1)))
            {
                var i = _tup_1.Item1;
                var v = _tup_1.Item2;
                pkt[i + 1] = (byte)v;
            }
            return pkt;
        }

        public byte[] P_POWER_ON = new_packet(0, 65, 128);
        public byte[] P_POWER_OFF = new_packet(0, 65, 3);
        public byte[] P_RESTORE = new_packet(0, 65);
        public byte[] P_LED_LOAD = new_packet(0, 80);
        public byte[] P_LED_SAVE = new_packet(0, 80, 85);
        public byte[] P_APPLY = new_packet(0, 81, 40, 0, 0, 224);
        public byte[] P_MAGIC_2 = new_packet(0, 81, 150);
        public byte[] P_MIRAGE_OFF = new_packet(0, 81, 113, 0, 0, 1, 0, 255, 74, 2, 0, 255, 74, 3, 0, 255, 74, 4, 0, 255, 74);
        public byte[] P_GET_VER = new_packet(0, 18, 32);

        public CMRGBController()
        {
            this.init_hid_device();

        }

        public void init_hid_device()
        {
            //var device_list = (from x in hid.enumerate(this.VENDOR_ID, this.PRODUCT_ID)
            //                   where x["interface_number"] == this.IFACE_NUM
            //                   select x).ToList();
            //if (device_list.Count == 0)
            //{
            //    throw Exception("No devices found. See: https://github.com/gfduszynski/cm-rgb/issues/9");
            //}
            //this.device = hid.device();
            //try
            //{
            //    this.device.open_path(device_list[0]["path"]);
            //}
            //catch (OSError)
            //{
            //    Console.WriteLine("Failed to access usb device. See: https://github.com/gfduszynski/cm-rgb/wiki/1.-Installation-&-Configuration#3-configuration");
            //    Console.WriteLine("Also check if other process is not using the device.\n");
            //    throw;
            //}




            var terp = new OpenConfiguration();
            terp.SetOption(OpenOption.Exclusive, true);

            var loader = new HidDeviceLoader();

            var devices = loader.GetDevices(VENDOR_ID, PRODUCT_ID).ToArray();

            foreach (var tdevice in devices)
            {
                try
                {
                    stream = tdevice.Open(terp);
                    this.init_controller();
                    Debug.WriteLine("Well, that seemed to work");
                    break;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

        }

        public void init_controller()
        {
            // Without this controller wont accept changes
            this.send_packet(this.P_POWER_ON);
            // No idea what this does but it's in original startup sequence
            this.send_packet(this.P_MAGIC_2);
            // Some sort of apply / flush op
            this.apply();
        }

        public byte[] send_packet(byte[] packet)
        {
            this.stream.Write(packet);
            //this.device.write(packet);
            byte[] buf = new byte[64];
            this.stream.Read(buf, 0, 64);
            return buf;
        }

        public void headless_send_packet(byte[] packet)
        {
            this.stream.Write(packet);
        }

        public virtual object apply()
        {
            return this.send_packet(this.P_APPLY);
        }

        public virtual object save()
        {
            return this.send_packet(this.P_LED_SAVE);
        }

        public void restore()
        {
            //       this.send_packet(this.enableMirage(0, 0, 0));
            this.send_packet(this.P_LED_LOAD);
            this.send_packet(this.P_POWER_OFF);
            this.send_packet(this.P_RESTORE);
            this.apply();
        }

        //public virtual object disableMirage()
        //{
        //    this.send_packet(this.enableMirage(0, 0, 0));
        //}

        //public virtual object enableMirage(object rHz, object gHz, object bHz)
        //{
        //    Func<object, object> hzToBytes = hz =>
        //    {
        //        if (hz == 0)
        //        {
        //            return new List<object> {
        //                0,
        //                255,
        //                74
        //            };
        //        }
        //        var v = 187498.0 / hz;
        //        var vMul = math.floor(v / 256.0);
        //        var vRem = v / (vMul + 1);
        //        return new List<object> {
        //            min(vMul, 255),
        //            math.floor(vRem % 1 * 256),
        //            math.floor(vRem)
        //        };
        //    };
        //    var rBytes = hzToBytes(rHz);
        //    var gBytes = hzToBytes(gHz);
        //    var bBytes = hzToBytes(bHz);
        //    var pkt = this.new_packet(0, 81, 113, 0, 0, 1, 0, 255, 74, 2, rBytes[0], rBytes[1], rBytes[2], 3, gBytes[0], gBytes[1], gBytes[2], 4, bBytes[0], bBytes[1], bBytes[2]);
        //    return this.send_packet(pkt);
        //}

        public virtual object getVersion()
        {
            var reply = this.send_packet(this.P_GET_VER);
            var fv = new byte[16];
            var i = 0;
            while (i < 16)
            {
                if (reply[i + 9] != 0)
                {
                    fv[(i / 2)] = (byte)(fv[(i / 2)] + reply[i + 0x09]);
                }
                else
                {
                    break;
                }
                i += 2;
            }

            var rplytxt = System.Text.Encoding.UTF8.GetString(fv);
            return rplytxt;
        }

        // color_source 0x20 takes supplied color for breathe mode
        public byte[] set_channel(
            LedChannel channel,
            LedMode mode,
            byte brightness,
            byte r,
            byte g,
            byte b,
            byte speed = 255,
            byte altr = 0,
            byte altg = 0,
            byte altb = 0
            )
        {
            byte color_source = 32;
            var pkt = new_ffpacket(255, 81, 44, 1, 0, (byte)channel, speed, color_source, (byte)mode, 255, brightness, r, g, b, altr, altg, altb);
            return this.send_packet(pkt);
            //headless_send_packet(pkt);
        }

        public byte[] assign_leds_to_channels(AMDWraith.LedChannel logo, AMDWraith.LedChannel fan, byte[] ring)
        {
            byte[] pkt = new_packet(0, 81, 160, 1, 0, 0, 3, 0, 0, (byte)logo, (byte)fan);
            var j = 0;
            // Ring LED's
            //foreach (var i in range(11, 26))
            for (int i = 11; i <= 26; i++)
            {
                if (j < ring.Length)
                {
                    pkt[i] = ring[j];
                    j += 1;
                }
                else
                {
                    pkt[i] = (byte)LedChannel.OFF;
                }
            }
            return this.send_packet(pkt);
            //headless_send_packet(pkt);
        }

        public byte[] assign_leds_to_channels(AMDWraith.LedChannel logo, AMDWraith.LedChannel fan, LedChannel[] ring)
        {
            byte[] pkt = new_packet(0, 81, 160, 1, 0, 0, 3, 0, 0, (byte)logo, (byte)fan);
            var j = 0;
            // Ring LED's
            //foreach (var i in range(11, 26))
            for (int i = 11; i <= 26; i++)
            {
                if (j < ring.Length)
                {
                    pkt[i] = (byte)ring[j];
                    j += 1;
                }
                else
                {
                    pkt[i] = (byte)LedChannel.OFF;
                }
            }

            return this.send_packet(pkt);
        }
    }
}
