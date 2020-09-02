using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MadLedFrameworkSDK;


namespace RainbowWave
{
    public class RainbowWaveConfigModel : SLSConfigData
    {
        private int speed = 3;
        public int Speed
        {
            get => speed;
            set => SetProperty(ref speed, value);
        }

        private ControlDevice controlDevice;

        public ControlDevice CurrentControlDevice
        {
            get => controlDevice; 
            set => SetProperty(ref controlDevice, value);
        }
    }
}
