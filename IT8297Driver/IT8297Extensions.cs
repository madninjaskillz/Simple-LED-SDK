using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HidSharp;
using MadLedFrameworkSDK;

namespace IT8297Driver
{
    public static class Extensions
    {

        public static void SetLEDEffect(this HidStream stream, byte led, byte mode, byte r, byte g, byte b)
        {
            
            RGBEffect effect = new RGBEffect();
            effect.ReportId = 0xcc;
            effect.Header = led;

            if (led < 8)
                effect.Header = (byte)(32 + led); // set as default
            else
                effect.Header = led;

            effect.EffectType = mode;

            effect.Zone0 = (UInt32)(1 << (effect.Header - 32));

            effect.SetColor0(r, g, b);

            effect.MaxBrightness = 100;
            effect.MinBrightness = 0;
            //effect.Color0 = 0x00FF2100; //orange
            effect.Period0 = 1200;
            effect.Period1 = 1200;
            effect.Period2 = 200;
            effect.Period3 = 200;
            effect.EffectParam0 = 0; // ex color count to cycle through (max seems to be 7)
            effect.EffectParam1 = 0;
            effect.EffectParam2 = 1; // ex flash repeat count
            effect.EffectParam3 = 0;

            stream.SetFeature(effect.Buffer);
            stream.SendPacket(0x28, 0xFF);
        }

        public static void Init(this HidStream stream)
        {
            byte[] buffer = new byte[64];
            buffer[0] = 0xCC;
            for (int i = 0; i < 8; i++)
            {
                buffer[1] = (byte)(0x20 + i);
                stream.SendPacket(buffer);
            }

            stream.ApplyEffect();
            stream.EnableBeat(false);
            stream.DisableEffect(0); // yeah...
            stream.SendPacket(0x20, 0xFF); //?
            stream.ApplyEffect();


        }

        public static void ForceEffect(this HidStream stream)
        {
            byte[] buffer = new byte[64];
            buffer[0] = 0xCC;
            buffer[11] = 1;
            buffer[12] = 90;
            buffer[14] = 255;
            buffer[15] = 0;
            buffer[16] = 255;
            buffer[30] = 0;
            buffer[31] = 0;
            buffer[32] = 0;


        }

        public static void SaveSettingToMCU(this HidStream stream)
        {
            byte[] save = new byte[64];

            save[0] = 0xCC;
            save[1] = 0x5E;
            stream.SendPacket(save);
        }

        public static void DisableEffect(this HidStream stream, byte disable)
        {
            stream.SendPacket(0x32, disable);
        }

        public static void EnableBeat(this HidStream stream, bool b)
        {
            stream.SendPacket((byte)0x31, (byte)(b ? 1 : 0));
        }

        public static void ApplyEffect(this HidStream stream)
        {
            stream.SendPacket(0x28, 0xFF);
        }

        public class RGBEffect
        {
            public byte[] Buffer = new byte[64];

            public Byte ReportId
            {
                get => Buffer.GetByte(0);
                set => Buffer.SetByte(value, 0);
            }
            public Byte Header
            {
                get => Buffer.GetByte(1);
                set => Buffer.SetByte(value, 1);
            }
            public UInt32 Zone0
            {
                get => Buffer.GetUInt32(2);
                set => Buffer.SetUInt32(value, 2);
            }
            public UInt32 Zone1
            {
                get => Buffer.GetUInt32(6);
                set => Buffer.SetUInt32(value, 6);
            }
            public Byte Reserved0
            {
                get => Buffer.GetByte(10);
                set => Buffer.SetByte(value, 10);
            }
            public Byte EffectType
            {
                get => Buffer.GetByte(11);
                set => Buffer.SetByte(value, 11);
            }
            public Byte MaxBrightness
            {
                get => Buffer.GetByte(12);
                set => Buffer.SetByte(value, 12);
            }
            public Byte MinBrightness
            {
                get => Buffer.GetByte(13);
                set => Buffer.SetByte(value, 13);
            }
            public UInt32 Color0
            {
                get => Buffer.GetUInt32(14);
                set => Buffer.SetUInt32(value, 14);
            }
            public UInt32 Color1
            {
                get => Buffer.GetUInt32(18);
                set => Buffer.SetUInt32(value, 18);
            }
            public UInt16 Period0
            {
                get => Buffer.GetUInt16(22);
                set => Buffer.SetUInt16(value, 22);
            }
            public UInt16 Period1
            {
                get => Buffer.GetUInt16(24);
                set => Buffer.SetUInt16(value, 24);
            }
            public UInt16 Period2
            {
                get => Buffer.GetUInt16(26);
                set => Buffer.SetUInt16(value, 26);
            }
            public UInt16 Period3
            {
                get => Buffer.GetUInt16(28);
                set => Buffer.SetUInt16(value, 28);
            }
            public Byte EffectParam0
            {
                get => Buffer.GetByte(30);
                set => Buffer.SetByte(value, 30);
            }
            public Byte EffectParam1
            {
                get => Buffer.GetByte(31);
                set => Buffer.SetByte(value, 31);
            }
            public Byte EffectParam2
            {
                get => Buffer.GetByte(32);
                set => Buffer.SetByte(value, 32);
            }
            public Byte EffectParam3
            {
                get => Buffer.GetByte(33);
                set => Buffer.SetByte(value, 33);
            }

            public void SetColor0(byte r, byte g, byte b)
            {
                UInt32 clr = (UInt32)(r << 16 | g << 8 | b);
                Color0 = clr;
            }
        }

        public class RGBPacket
        {
            public byte[] Buffer = new byte[64];

            public Byte ReportId
            {
                get => Buffer.GetByte(0);
                set => Buffer.SetByte(value, 0);
            }

            public Byte Header
            {
                get => Buffer.GetByte(1);
                set => Buffer.SetByte(value, 1);
            }

            public UInt16 ByteOffset
            {
                get => Buffer.GetUInt16(2);
                set => Buffer.SetUInt16(value, 2);
            }

            public byte ByteCount
            {
                get => Buffer.GetByte(4);
                set => Buffer.SetByte(value, 4);
            }

            public Extensions.RAWLED GetLED(int index) => Buffer.GetLed(5 + (index * 3));
            public void SetLed(Extensions.RAWLED value, int index) => Buffer.SetLed(value, 5 + (index * 3));

            public void Reset(byte headerPort)
            {
                ReportId = 0xCC;
                Header = headerPort; //sending as 0x53, or was it 0x54, screws with color cycle effect
                ByteOffset = 0;
                ByteCount = 0;

                for (int i = 5; i < (19 * 3) + 5; i++)
                {
                    Buffer[i] = 0;
                }
            }
        }

        public static int effect_disabled = 0;
        public static void DisableBuiltinEffect(this HidStream stream, int enable_bit, int mask)
        {
            if ((effect_disabled & enable_bit) == 1)
            {
                return;
            }

            effect_disabled &= ~mask;
            effect_disabled |= enable_bit;

            byte ed = (byte)effect_disabled;
            stream.SendPacket(0x32, ed);
        }

        public static void SendRGB(this HidStream stream, List<LEDColor> ledlist, byte header, int? single_led = null)
        {
            RGBPacket packet = new RGBPacket();

            int sent_data = 0, res, k = 0;
            int leds = 19;
            int left_leds = ledlist.Count;

            packet.ReportId = 0xcc;
            packet.Header = header;
            packet.ByteOffset = 0;
            packet.ByteCount = (byte)(leds * 3);

            if (single_led.HasValue)
            {
                left_leds = 1;
                k = single_led.Value;
                sent_data = k * 3;
                left_leds = 1;
            }


            while (left_leds > 0)
            {
                packet.Reset(header);
                leds = Math.Min(leds, left_leds);
                left_leds -= leds;

                packet.ByteCount = (byte)(leds * 3);
                packet.ByteOffset = (ushort)sent_data;
                sent_data += packet.ByteCount;

                for (int i = 0; i < leds; i++)
                {
                    RAWLED l = new RAWLED();
                    if (single_led.HasValue)
                    {
                        l.Red = (byte)ledlist[0].Red;
                        l.Green = (byte)ledlist[0].Green;
                        l.Blue = (byte)ledlist[0].Blue;
                    }
                    else
                    {
                        l.Red = (byte)ledlist[k].Red;
                        l.Green = (byte)ledlist[k].Green;
                        l.Blue = (byte)ledlist[k].Blue;
                    }

                    packet.SetLed(l, i);

                    k++;
                }

                stream.SendPacket(packet.Buffer);
                // stream.ApplyEffect();
            }

            //  stream.SaveSettingToMCU();


            if (header == HDR_D_LED1_RGB)
                stream.DisableBuiltinEffect(0x01, 0x01);
            else
                stream.DisableBuiltinEffect(0x02, 0x02);

        }

        const byte HDR_BACK_IO = 0x20;
        const byte HDR_CPU = 0x21;
        const byte HDR_PCIE = 0x23;
        const byte HDR_LED_C1C2 = 0x24;
        const byte HDR_D_LED1 = 0x25;
        const byte HDR_D_LED2 = 0x26;
        const byte HDR_D_LED1_RGB = 0x58;
        const byte HDR_D_LED2_RGB = 0x59;

        public class RAWLED
        {
            public byte[] Buffer = new byte[3];

            public Byte Green
            {
                get => Buffer.GetByte(0);
                set => Buffer.SetByte(value, 0);
            }

            public Byte Red
            {
                get => Buffer.GetByte(1);
                set => Buffer.SetByte(value, 1);
            }

            public Byte Blue
            {
                get => Buffer.GetByte(2);
                set => Buffer.SetByte(value, 2);
            }
        }

        public static byte GetByte(this byte[] buffer, int offset) => buffer[offset];
        public static UInt16 GetUInt16(this byte[] buffer, int offset)
        {
            UInt16[] output = new ushort[1];
            Buffer.BlockCopy(buffer, offset, output, 0, 2);
            return output[0];
        }

        public static UInt32 GetUInt32(this byte[] buffer, int offset)
        {
            UInt32[] output = new UInt32[1];
            Buffer.BlockCopy(buffer, offset, output, 0, 4);
            return output[0];
        }

        public static RAWLED GetLed(this byte[] buffer, int offset)
        {
            RAWLED output = new RAWLED();
            Buffer.BlockCopy(buffer, offset, output.Buffer, 0, 4);
            return output;
        }

        public static void SetByte(this byte[] buffer, byte vl, int offset) => Buffer.BlockCopy(new byte[] { vl }, 0, buffer, offset, 1);
        public static void SetUInt16(this byte[] buffer, UInt16 vl, int offset) => Buffer.BlockCopy(new UInt16[] { vl }, 0, buffer, offset, 2);
        public static void SetUInt32(this byte[] buffer, UInt32 vl, int offset) => Buffer.BlockCopy(new UInt32[] { vl }, 0, buffer, offset, 4);
        public static void SetLed(this byte[] buffer, RAWLED vl, int offset) => Buffer.BlockCopy(vl.Buffer, 0, buffer, offset, 3);

        public static T ReadStruct<T>(this byte[] bytes)
        {
            var pinned = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var stt = (T)Marshal.PtrToStructure(pinned.AddrOfPinnedObject(), typeof(T));
            pinned.Free();
            return stt;
        }

        public static void SetLedCount(this HidStream stream, LEDCount s0 = LEDCount.LEDS_32, LEDCount s1 = LEDCount.LEDS_32)
        {
            byte x = (byte)(((byte)s1) << (byte)4);
            SendPacket(stream, 0x34, (byte)(((byte)s0) | x));
        }
        public static void SendPacket(this HidStream stream, byte a, byte b, byte c = 0)
        {
            byte[] buffer = new byte[64];
            buffer[0] = 0xCC;
            buffer[1] = a;
            buffer[2] = b;
            buffer[3] = c;

            stream.SetFeature(buffer);

        }

        public static void SendPacket(this HidStream stream, byte[] packet)
        {
            stream.SetFeature(packet);
        }

        public static void SetCalibration(this HidStream stream)
        {
            byte[] buffer = new byte[64];
            buffer[0] = 0xcc;
            buffer[1] = 0x33;

            // D_LED1 WS2812 GRB, 0x00RRGGBB to 0x00GGRRBB
            buffer[2] = 0x02; // B
            buffer[3] = 0x00; // G
            buffer[4] = 0x01; // R
            buffer[5] = 0x00;

            // D_LED2 WS2812 GRB
            buffer[6] = 0x02;
            buffer[7] = 0x00;
            buffer[8] = 0x01;
            buffer[9] = 0x00;

            // LED C1/C2 12vGRB, seems pins already connect to LEDs correctly
            buffer[10] = 0x00;
            buffer[11] = 0x01;
            buffer[12] = 0x02;
            buffer[13] = 0x00;

            // Spare set seen in some Motherboard models
            buffer[14] = 0x00;
            buffer[15] = 0x01;
            buffer[16] = 0x02;
            buffer[17] = 0x00;

            stream.SendPacket(buffer);
        }


        public enum LEDCount
        {
            LEDS_32 = 0,
            LEDS_64,
            LEDS_256,
            LEDS_512,
            LEDS_1024,
        };

    }
}
