namespace ProcessKiller
{
    public class ConsoleLogger : ILogger //не нужен, но пусть будет
    {
        public static void Log(string message)
        {
            System.Console.WriteLine(message);
        }
    }
}