using System;
using System.IO;
using Microsoft.Win32;

namespace Fanya.LGDisplaySwitcher
{
    internal static class AutoStartManager
    {
        private const string AppName = "Fanya.LGDisplaySwitcher";

        public static void AddToStartup()
        {
            Config config = Config.Load();
            try
            {
                string exePath = Environment.ProcessPath ?? AppDomain.CurrentDomain.BaseDirectory;

                string runCommand = $"\"{exePath}\" --service";

                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true)!;
                key.SetValue(AppName, runCommand);

                Console.WriteLine("Приложение успешно добавлено в автозапуск Windows (--service).");
                config.InAutoStart = true;
                config.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка записи в реестр: {ex.Message}");
                Console.ReadKey();
            }
            Console.WriteLine("Нажмите любую клавишу...");
            Console.ReadKey();
        }

        public static void RemoveFromStartup()
        {
            Config config = Config.Load();
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true)!;
            if (key.GetValue(AppName) != null)
            {
                key.DeleteValue(AppName);
                Console.WriteLine("Приложение удалено из автозапуска.");
                config.InAutoStart = false;
                config.Save();
            }
        }
    }
}