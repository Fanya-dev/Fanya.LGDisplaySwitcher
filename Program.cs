using MartinGC94.DisplayConfig.API;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fanya.LGDisplaySwitcher
{
    public static class Program
    {
        static Config config = null!;
        static bool isServiceMode = false;
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();
        public static async Task Main(string[] args)
        {

            config = Config.Load();

            if (args.Length > 0 && args[0] == "--service")
            {
                try
                {
                    Thread.Sleep(3000);
                    isServiceMode = true;
                    Task.Run(async () => await RunBackgroundServiceAsync()).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.ReadLine();
                }
                return;
            }

            AllocConsole();
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            ShowMainMenu();
        }

        private static void ShowMainMenu()
        {
            config = Config.Load();

            while (!isServiceMode)
            {
                Console.Clear();
                Console.WriteLine("=============================================");
                Console.WriteLine("           Fanya.LGDisplaySwitcher           ");
                Console.WriteLine("=============================================");
                Console.WriteLine($"Текущий ТВ IP: {config.TVIP}");
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("1. Сохранить текущую конфигурацию как РАБОЧИЙ профиль");
                Console.WriteLine("2. Сохранить текущую конфигурацию как ИГРОВОЙ профиль");
                if (config.InAutoStart) Console.WriteLine("4. Убрать утилиту из автозапуска");
                else Console.WriteLine("3. Добавить утилиту в автозапуск");
                Console.WriteLine("4. ЗАПУСТИТЬ В ФОНОВОМ РЕЖИМЕ (Воркер)");
                Console.WriteLine("5. Изменить IP-адрес телевизора");
                Console.WriteLine("6. Поставить РАБОЧИЙ профиль");
                Console.WriteLine("7. Поставить ИГРОВОЙ профиль");
                Console.WriteLine("0. Выход");
                Console.WriteLine("=============================================");
                Console.Write("Выберите действие (0-5): ");

                string? input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        SaveDisplayProfile("workProfile.json");
                        break;
                    case "2":
                        SaveDisplayProfile("tvProfile.json");
                        break;
                    case "3":
                        if (config.InAutoStart) AutoStartManager.RemoveFromStartup();
                        else AutoStartManager.AddToStartup();
                        break;
                    case "4":
                        Console.WriteLine("\nПереход в фоновый режим. Консоль можно будет свернуть...");
                        isServiceMode = true;
                        Task.Run(async () => await RunBackgroundServiceAsync()).GetAwaiter().GetResult();
                        break;
                    case "5":
                        ChangeTvIp();
                        break;
                    case "6":
                        Utilities.SwitchDisplay(false);
                        break;
                    case "7":
                        Utilities.SwitchDisplay(true);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неверный ввод. Нажмите любую клавишу...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void SaveDisplayProfile(string profileName)
        {
            string config = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, profileName);
            DisplayConfig displayConfig = DisplayConfig.GetConfig(MartinGC94.DisplayConfig.Native.Enums.DisplayConfigFlags.QDC_ALL_PATHS | MartinGC94.DisplayConfig.Native.Enums.DisplayConfigFlags.QDC_VIRTUAL_MODE_AWARE);

            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                IncludeFields = true
            };

            string json = JsonSerializer.Serialize(displayConfig, options);
            File.WriteAllText(config, json);

            Console.WriteLine("Профиль успешно сохранен! Нажмите любую клавишу...");
            Console.ReadKey();
        }

        private static async Task RunBackgroundServiceAsync()
        {
            try
            {
                if(!isServiceMode) Console.WriteLine("[Service] Фоновый мониторинг запущен...");

                while (true)
                {
                    if(!isServiceMode) Console.WriteLine($"[Service] Работаю 1");
                    bool isTvDisplayOn = Utilities.IsTvReachable(config.TVIP, config.PingTimeout);

                    if (isTvDisplayOn != config.LastTVState)
                    {
                        if (!isServiceMode) Console.WriteLine($"[Service] Переключение режима. Экран ТВ активен: {isTvDisplayOn}");
                        Utilities.SwitchDisplay(isTvDisplayOn);
                        config.LastTVState = isTvDisplayOn;
                        config.Save();
                    }
                    if (!isServiceMode) Console.WriteLine($"[Service] Работаю 4");

                    await Task.Delay(config.TimeToPingMs);
                }
            } catch(Exception ex)
            {
                if (!isServiceMode) Console.WriteLine(ex.ToString());
                if (!isServiceMode) Console.ReadLine();
            }
            }
        private static void ChangeTvIp()
        {
            Console.Clear();
            Console.WriteLine("=============================================");
            Console.WriteLine("        Смена IP-адреса телевизора           ");
            Console.WriteLine("=============================================");
            Console.WriteLine($"Текущий IP: {config.TVIP}");
            Console.Write("Введите новый IP-адрес: ");

            string? newIp = Console.ReadLine()?.Trim();

            if (!string.IsNullOrEmpty(newIp))
            {
                if (System.Net.IPAddress.TryParse(newIp, out _))
                {
                    config.TVIP = newIp;

                    config.Save();

                    Console.WriteLine($"\nIP успешно изменен на {newIp}!");
                    Console.WriteLine("Старый токен авторизации сброшен. Не забудь зарегистрировать ТВ заново (Пункт 3).");
                }
                else
                {
                    Console.WriteLine("\nОшибка: Введен некорректный формат IP-адреса.");
                }
            }
            else
            {
                Console.WriteLine("\nВвод отменен.");
            }

            Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
            Console.ReadKey();
        }
    }
}