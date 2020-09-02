using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MadLedFrameworkSDK
{
    public class SLSManager
    {
        private string configPath;
        public List<ISimpleLEDDriver> Drivers = new List<ISimpleLEDDriver>();

        public SLSManager(string cfgPath)
        {
            configPath = cfgPath;
        }

        public List<ControlDevice> GetDevices()
        {
            List<ControlDevice> controlDevices = new List<ControlDevice>();
            foreach (var simpleLedDriver in Drivers)
            {
                try
                {
                    var devices = simpleLedDriver.GetDevices();
                    if (devices != null)
                    {
                        controlDevices.AddRange(devices);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            return controlDevices;
        }

        public void Init()
        {
            foreach (var simpleLedDriver in Drivers)
            {
                simpleLedDriver.Configure(null);
            }
        }

        public void LoadConfigs()
        {
            foreach (ISimpleLEDDriverWithConfig simpleLedDriver in Drivers.OfType<ISimpleLEDDriverWithConfig>())
            {
                LoadConfig(simpleLedDriver);
            }
        }

        public void SaveConfigs()
        {
            foreach (ISimpleLEDDriverWithConfig simpleLedDriver in Drivers.OfType<ISimpleLEDDriverWithConfig>())
            {
                SaveConfig(simpleLedDriver);
            }
        }

        public void LoadConfig(ISimpleLEDDriverWithConfig simpleLedDriver)
        {
            string path = configPath + "\\" + simpleLedDriver.GetProperties().Id + "_config.json";
            string json = File.ReadAllText(path);
            SLSConfigData data = JsonConvert.DeserializeObject<SLSConfigData>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                // $type no longer needs to be first
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
            });
            simpleLedDriver.PutConfig(data);
        }

        public void SaveConfig(ISimpleLEDDriverWithConfig simpleLedDriver)
        {
            SLSConfigData data = simpleLedDriver.GetConfig<SLSConfigData>();
            string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                // $type no longer needs to be first
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
            });
            string path = configPath + "\\" + simpleLedDriver.GetProperties().Id + "_config.json";
            File.WriteAllText(path, json);
        }

    }
}
