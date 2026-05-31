using MartinGC94.DisplayConfig.API;
using MartinGC94.DisplayConfig.Native.Enums;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;

namespace Fanya.LGDisplaySwitcher
{
    internal class Utilities
    {
        public static bool IsTvReachable(string ipAddress, int timeoutMs = 1000, int port = 3001)
        {
            using var client = new TcpClient();
            try
            {
                var result = client.BeginConnect(ipAddress, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(timeoutMs);
                if (!success) return false;
                client.EndConnect(result);
                return true;
            }
            catch { return false; }
        }

        public static void SwitchDisplay(bool isTvOn)
        {
            try
            {
                string profileName = isTvOn ? "tvProfile.json" : "workProfile.json";
                string config = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, profileName);
                JsonSerializerOptions options = new()
                {
                    IncludeFields = true
                };

                DisplayConfig displayConfig = JsonSerializer.Deserialize<DisplayConfig>(File.ReadAllText(config), options);
                displayConfig.UpdateAdapterIds();

                displayConfig.ApplyConfig(SetDisplayConfigFlags.SDC_APPLY | SetDisplayConfigFlags.SDC_USE_SUPPLIED_DISPLAY_CONFIG | SetDisplayConfigFlags.SDC_VIRTUAL_MODE_AWARE | SetDisplayConfigFlags.SDC_SAVE_TO_DATABASE);
            } catch(Exception ex) { 
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
        }
    }
}