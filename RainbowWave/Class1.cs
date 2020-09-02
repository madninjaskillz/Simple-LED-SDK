using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Drawing;
using MadLedFrameworkSDK;
using System.Windows.Controls;

namespace RainbowWave
{
    public class DummyControlDevice : ControlDevice
    {
		
    }
	// Token: 0x02000002 RID: 2
	public class RainbowWaveDriver : ISimpleLEDDriverWithConfig
	{

		public static Assembly assembly = Assembly.GetExecutingAssembly();
		public static Stream imageStream = assembly.GetManifestResourceStream("RainbowWave.rainbowwave.png");


		public RainbowWaveConfigModel configModel = new RainbowWaveConfigModel();

		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public RainbowWaveDriver()
		{
			for (int i = 0; i < 20; i++)
			{
				this.leds[i] = new ControlDevice.LedUnit
				{
					LEDName = "LED " + i.ToString(),
					Data = new ControlDevice.LEDData
					{
						LEDNumber = i
					}
				};
			}
			this.timer = new Timer(new TimerCallback(this.TimerCallback), null, 0, 33);
		}

		// Token: 0x06000002 RID: 2 RVA: 0x000020E0 File Offset: 0x000002E0
		private void TimerCallback(object state)
		{
			LEDColor ledcolor = LEDColor.FromHSL(105, 100, 100);
			for (int i = 0; i < 20; i++)
			{
				float num = (float)((i + this.cycle) % 20);
				double num2 = (double)num / 20.0;
				int num3 = (int)(num2 * 360.0);
				this.leds[i].Color = LEDColor.FromHSL(num3, 100, 100);
			}
			this.cycle++;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x0000215E File Offset: 0x0000035E
		public void Configure(DriverDetails driverDetails)
		{
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002164 File Offset: 0x00000364
		public List<ControlDevice> GetDevices()
		{
			return new List<ControlDevice>
			{
				new ControlDevice
				{
					Name = "Rainbow Wave",
					Driver = this,
					ProductImage = (Bitmap) System.Drawing.Image.FromStream(imageStream),
			        LEDs = this.leds,
					DeviceType = "Effect"
				}
			};
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000021B4 File Offset: 0x000003B4
		public void Push(ControlDevice controlDevice)
		{
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000021B7 File Offset: 0x000003B7
		public void Pull(ControlDevice controlDevice)
		{
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000021BC File Offset: 0x000003BC
		public DriverProperties GetProperties()
		{
			return new DriverProperties
			{
				SupportsPull = false,
				SupportsPush = false,
				IsSource = true,
				SupportsCustomConfig = true,
				Id = Guid.Parse("e8c93cac-2379-4f8f-a4c8-8933e77e5c44")
			};
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000021EB File Offset: 0x000003EB
		public string Name()
		{
			return "Rainbow Wave";
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000021F2 File Offset: 0x000003F2
		public void Dispose()
		{
			this.timer.Dispose();
		}

        public UserControl GetCustomConfig(ControlDevice controlDevice)
		{
            var config = new CustomConfig
            {
                DataContext = configModel
            };

            configModel.CurrentControlDevice = controlDevice;

            return config;
        }

		public T GetConfig<T>() where T : SLSConfigData
		{
			RainbowWaveConfigModel data = new RainbowWaveConfigModel();
			SLSConfigData proxy = data;
			return (T)proxy;
		}


		public void PutConfig<T>(T config) where T : SLSConfigData
		{
			RainbowWaveConfigModel proxy = config as RainbowWaveConfigModel;
		}

		// Token: 0x04000001 RID: 1
		private const int LEDCount = 20;

		// Token: 0x04000002 RID: 2
		private Timer timer;

		// Token: 0x04000003 RID: 3
		private ControlDevice.LedUnit[] leds = new ControlDevice.LedUnit[20];

		// Token: 0x04000004 RID: 4
		private int cycle = 5;
	}
}
