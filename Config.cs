using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace Fanya.LGDisplaySwitcher
{
    internal class Config
    {
        public static string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json");

        public bool InAutoStart { get; set; } = false;
        public string TVIP { get; set; } = default!;
        public bool LastTVState { get; set; } = false;
        public int TimeToPingMs { get; set; }
        public int PingTimeout { get; set; }
        public bool Debug { get; set; }

        public void LoadDefaults()
        {
            TVIP = "10.2.0.8";
            TimeToPingMs = 2000;
            PingTimeout = 1000;
            Debug = false;
        }

        public static Config Load()
        {
            if (File.Exists(ConfigPath))
            {
                try
                {
                    var json = File.ReadAllText(ConfigPath);
                    var loaded = JsonSerializer.Deserialize<Config>(json);
                    if (loaded != null) return loaded;
                }
                catch
                {
                }
            }

            var @default = new Config();
            @default.LoadDefaults();
            @default.Save();
            return @default;
        }

        public void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this, options));
        }
    }
}