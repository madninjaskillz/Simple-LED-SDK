using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MadLedFrameworkSDK;
using Timer = System.Threading.Timer;

namespace ScreenShotSource
{
    public class ScreenShotSourceProvider : ISimpleLEDDriver
    {
        public static int[] RemapKeyboard = new int[122];
        private void Remappy()
        {
            int[,] mappy = new int[22, 6];
            int startx = 0;
            int starty = 0;
            int x = 0;
            int y = 0;
            for (int i = 0; i < 122; i++)
            {
                Debug.WriteLine($"{x},{y}={i}");
                mappy[x, y] = i;

                y--;
                x++;
                if (y < 0)
                {
                    starty++;
                    if (starty > 5)
                    {
                        starty = 5;
                        startx++;
                    }

                    x = startx;
                    y = starty;
                }


            }

            int ct = 0;
            for (int yy = 0; yy < 6; yy++)
            {
                for (int xx = 0; xx < 22; xx++)
                {
                    RemapKeyboard[ct] = mappy[xx, yy];
                }
            }
        }
        private Timer timer;
        ControlDevice.LedUnit[] fullLeds = new ControlDevice.LedUnit[122];
        ControlDevice.LedUnit[] ringLeds = new ControlDevice.LedUnit[46];
        ControlDevice.LedUnit[] oneLeds = new ControlDevice.LedUnit[1];
        public void Dispose()
        {
            
        }

        public void Configure(DriverDetails driverDetails)
        {
            Remappy();

            for (int i = 0; i < fullLeds.Length; i++)
            {
                fullLeds[i] = new ControlDevice.LedUnit
                {
                    Color = new LEDColor(0, 0, 0),
                    LEDName = "Pixel " + i,
                    Data = new ControlDevice.LEDData
                    {
                        LEDNumber = i
                    }
                };
            }

            for (int i = 0; i < ringLeds.Length; i++)
            {
                ringLeds[i] = new ControlDevice.LedUnit
                {
                    Color = new LEDColor(0, 0, 0),
                    LEDName = "Pixel " + i,
                    Data = new ControlDevice.LEDData
                    {
                        LEDNumber = i
                    }
                };
            }

            oneLeds[0] = new ControlDevice.LedUnit
            {
                Color = new LEDColor(0, 0, 0),
                LEDName = "Pixel " + 0,
                Data = new ControlDevice.LEDData
                {
                    LEDNumber = 0
                }
            };

            timer = new Timer(TimerCallback, null, 0, 33);
        }

        public List<ControlDevice> GetDevices()
        {
            return new List<ControlDevice>
            {
                new ControlDevice
                {
                    Name = "Screenshot Source Full",
                    Driver = this,
                    LEDs = fullLeds,
                    DeviceType = DeviceTypes.Effect
                },
                new ControlDevice
                {
                    Name = "Screenshot Source Ring",
                    Driver = this,
                    LEDs = ringLeds,
                    DeviceType = DeviceTypes.Effect
                },
                new ControlDevice
                {
                    Name = "Screenshot Source Pixel",
                    Driver = this,
                    LEDs = oneLeds,
                    DeviceType = DeviceTypes.Effect
                }
            };
        }

        public void Push(ControlDevice controlDevice)
        {
            
        }

        private bool locked = false;
        private System.Drawing.Color CalculateAverageColor(Bitmap bm)
        {
            if (!locked)
            {
                locked = true;
                int width = bm.Width;
                int height = bm.Height;
                int red = 0;
                int green = 0;
                int blue = 0;
                int minDiversion = 0; // drop pixels that do not differ by at least minDiversion between color values (white, gray or black)
                int dropped = 0; // keep track of dropped pixels
                long[] totals = new long[] { 0, 0, 0 };
                int bppModifier = bm.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb ? 3 : 4; // cutting corners, will fail on anything else but 32 and 24 bit images
                BitmapData srcData = bm.LockBits(new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height),
                    ImageLockMode.ReadOnly, bm.PixelFormat);
                
                int stride = srcData.Stride;
                IntPtr Scan0 = srcData.Scan0;
                int ringptr = 0;
                int fullptr = 0;
                unsafe
                {
                    byte* p = (byte*) (void*) Scan0;


                    //full ++ pixel
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int idx = (y * stride) + x * bppModifier;
                            red = p[idx + 2];
                            green = p[idx + 1];
                            blue = p[idx];

                            int rmp = RemapKeyboard[fullptr];

                            fullLeds[rmp].Color = new LEDColor(red, green, blue);

                            if (Math.Abs(red - green) > minDiversion || Math.Abs(red - blue) > minDiversion ||
                                Math.Abs(green - blue) > minDiversion)
                            {
                                totals[2] += red;
                                totals[1] += green;
                                totals[0] += blue;
                            }
                            else
                            {
                                dropped++;
                            }
                        }
                    }

                    //ring
                    for (int x = 0; x < width; x++)
                    {
                        int idx = (0 * stride) + x * bppModifier;
                        red = p[idx + 2];
                        green = p[idx + 1];
                        blue = p[idx];

                        ringLeds[ringptr].Color = new LEDColor(red, green, blue);
                        ringptr++;
                    }

                    for (int y = 1; y < height-1; y++)
                    {
                        int x = width-1;
                        int idx = (0 * stride) + x * bppModifier;
                        red = p[idx + 2];
                        green = p[idx + 1];
                        blue = p[idx];

                        ringLeds[ringptr].Color = new LEDColor(red, green, blue);
                        ringptr++;
                    }

                    for (int x = width-1; x >= 0; x--)
                    {
                        int y = height - 1;
                        int idx = (y * stride) + x * bppModifier;
                        red = p[idx + 2];
                        green = p[idx + 1];
                        blue = p[idx];

                        ringLeds[ringptr].Color = new LEDColor(red, green, blue);
                        ringptr++;
                    }

                    for (int y = height-1; y > 0; y--)
                    {
                        int x = 0;
                        int idx = (0 * stride) + x * bppModifier;
                        red = p[idx + 2];
                        green = p[idx + 1];
                        blue = p[idx];

                        ringLeds[ringptr].Color = new LEDColor(red, green, blue);
                        ringptr++;
                    }
                }

                int count = width * height - dropped;

                if (count == 0)
                {
                    return Color.Black;
                }

                int avgR = (int) (totals[2] / count);
                int avgG = (int) (totals[1] / count);
                int avgB = (int) (totals[0] / count);
                bm.UnlockBits(srcData);
                locked = false;
                return System.Drawing.Color.FromArgb(avgR, avgG, avgB);
            }
            else
            {
                return System.Drawing.Color.FromArgb(oneLeds[0].Color.Red, oneLeds[0].Color.Green, oneLeds[0].Color.Blue);
            }
        }

        public class WinAPI
        {
            [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetDC(IntPtr hWnd);

            [DllImport("user32.dll", ExactSpelling = true)]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

            [DllImport("gdi32.dll", ExactSpelling = true)]
            public static extern IntPtr BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

            [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
            public static extern IntPtr GetDesktopWindow();
        }

        private void TakingScreenshotEx2()
        {
            try
            {
                int screenWidth = Screen.PrimaryScreen.Bounds.Width;
                int screenHeight = Screen.PrimaryScreen.Bounds.Height;

                if (screenshot == null)
                {
                    screenshot = new Bitmap(screenWidth, screenHeight);
                }

                Graphics g = Graphics.FromImage(screenshot);

                IntPtr dc1 = WinAPI.GetDC(WinAPI.GetDesktopWindow());
                IntPtr dc2 = g.GetHdc();

                //Main drawing, copies the screen to the bitmap
                //last number is the copy constant
                WinAPI.BitBlt(dc2, 0, 0, screenWidth, screenHeight, dc1, 0, 0, 13369376);

                //Clean up
                WinAPI.ReleaseDC(WinAPI.GetDesktopWindow(), dc1);
                g.ReleaseHdc(dc2);
                g.Dispose();

                scaledScreenshot = new Bitmap(screenshot, new Size(20, 6));
            }
            catch
            {
            }
        }

        private Bitmap scaledScreenshot = null;
        private Bitmap screenshot = null;
        DateTime lastScreenShot = DateTime.MinValue;
        public void Pull(ControlDevice controlDevice)
        {
            if ((DateTime.Now - lastScreenShot).TotalMilliseconds > 30)
            {
                TakingScreenshotEx2();
                Color avg = CalculateAverageColor(screenshot);
                oneLeds[0].Color = new LEDColor(avg);
                lastScreenShot=DateTime.Now;
                
            }
        }

        private bool atIt = false;
        private int olc = 0;
        private void TimerCallback(object state)
        {
            if (!atIt)
            {
                atIt = true;
                if (!locked)
                {
                    if ((DateTime.Now - lastScreenShot).TotalMilliseconds > 30)
                    {
                        lastScreenShot = DateTime.Now;
                        try
                        {
                            TakingScreenshotEx2();
                            Color avg = CalculateAverageColor(scaledScreenshot);
                            oneLeds[0].Color = new LEDColor(avg);

                        }
                        catch
                        {
                        }

                    }

                    olc = 0;
                }
                else
                {
                    olc++;
                    if (olc > 60)
                    {
                        olc = 0;
                        locked = false;
                    }
                }

                atIt = false;
            }
        }

        public DriverProperties GetProperties()
        {
            return new DriverProperties
            {
                SupportsPull = false,
                SupportsPush = false,
                IsSource = true
            };
        }

        public string Name()
        {
            return "Screenshot Source";
        }
    }
}
