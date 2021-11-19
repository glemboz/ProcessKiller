using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace ProcessKiller
{
    partial class Program
    {
        private static System.Timers.Timer aTimer;

        static void Main(string[] args)
        {
            string processName = args[0];
            //минутка defensive programming
            if (!int.TryParse(args[1], out int timeToLive))
                ShowErrorAndExit();
            if (!int.TryParse(args[2], out int checkFrequency))
                ShowErrorAndExit();

            Dictionary<int, int> ProcessIdsWithTtl = new(); //словарь <PID процесса, количество пройденных проверок>

            SetTimerForProcess(checkFrequency, timeToLive, processName, ProcessIdsWithTtl);
            Console.WriteLine($"Поиск запущен в фоновом режиме!\n" +
                $"Чтобы прервать выполнение программы, нажмите Enter!\nПоиск {processName}");
            Console.ReadLine();
        }

        private static void SetTimerForProcess(int checkFrequency, int timeToLive, string processName, Dictionary<int, int> ProcessIdsWithTtl)
        {
            aTimer = new System.Timers.Timer(checkFrequency * 1000 * 60);
            aTimer.Elapsed += (sender, e) => OnTimedEvent(timeToLive, processName, ProcessIdsWithTtl);
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(int timeToLive, string processName, Dictionary<int, int> ProcessIdsWithTtl)
        {
            Process[] CurrentProcessCollection = Process.GetProcessesByName(processName);
            if (CurrentProcessCollection.Length != 0)
            {
                foreach (var process in CurrentProcessCollection)
                {
                    // Увеличиваем счётчик пройденных проверок
                    if (ProcessIdsWithTtl.ContainsKey(process.Id))
                        ProcessIdsWithTtl[process.Id] += 1;
                    else
                    {                    
                        Console.WriteLine($"Новый {processName} найден! PID={process.Id}");
                        ProcessIdsWithTtl.Add(process.Id, 0); //значение 0, когда процесс только что найден
                    }
                }
                foreach (var process in ProcessIdsWithTtl)
                {
                    if (!Array.Exists(CurrentProcessCollection, element => element.Id == process.Key))
                        ProcessIdsWithTtl.Remove(process.Key); //удаляем значения, которых нет в процессах в данный момент (на случай, если мы закрыли процесс самостоятельно)
                    if (process.Value >= timeToLive)
                        try
                        {
                            Process.GetProcessById(process.Key).Kill();
                            Console.WriteLine($"{processName} c PID {process.Key} закрыт!");
                            TextFileLogger.Log($"{DateTime.Now:dd.MM.yyyy HH:mm:ss} - {processName} c PID {process.Key} закрыт!\n");
                            ProcessIdsWithTtl.Remove(process.Key);
                        }
                        catch (Exception e)
                        {
                            aTimer.Stop();
                            Console.WriteLine($"Не удалось завершить {processName}\n{e.Message}\nНажмите ENTER для выхода");
                            Console.ReadLine();
                            Environment.Exit(0);
                            throw;
                        }
                }
            }
        }

        private static void ShowErrorAndExit()
        {
            aTimer.Stop();
            Console.WriteLine("Вводите параметры в правильном формате! (Название процесса, допустимое время жизни (в минутах) и частота проверки (в минутах))\nНажмите ENTER для выхода");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}