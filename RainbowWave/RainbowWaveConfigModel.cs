using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MadLedFrameworkSDK;
using Newtonsoft.Json;


namespace RainbowWave
{
    public class RainbowWaveConfigModel : SLSConfigData
    {
        private int speed = 3;
        public int Speed
        {
            get => speed;
            set
            {
                SetProperty(ref speed, value);
                DataIsDirty = true;
            }
        }

        private ControlDevice controlDevice;

        [JsonIgnore]
        public ControlDevice CurrentControlDevice
        {
            get => controlDevice; 
            set => SetProperty(ref controlDevice, value);
        }
    }
}
