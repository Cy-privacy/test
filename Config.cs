using System;
using System.IO;
using System.Text.Json;

namespace LunarAimbot
{
    public class Config
    {
        public float XYSensitivity { get; set; }
        public float TargetingSensitivity { get; set; }
        public float XYScale { get; set; }
        public float TargetingScale { get; set; }

        public static Config Load()
        {
            string configPath = Path.Combine("config", "config.json");
            
            if (!File.Exists(configPath))
            {
                var config = new Config
                {
                    XYSensitivity = 6.9f,
                    TargetingSensitivity = 6.9f,
                    XYScale = 10f/6.9f,
                    TargetingScale = 1000f/(6.9f * 6.9f)
                };
                
                Directory.CreateDirectory("config");
                File.WriteAllText(configPath, JsonSerializer.Serialize(config));
                return config;
            }

            return JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath));
        }

        public void Save()
        {
            string configPath = Path.Combine("config", "config.json");
            File.WriteAllText(configPath, JsonSerializer.Serialize(this));
        }
    }
}