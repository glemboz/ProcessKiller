using System.IO;

namespace ProcessKiller
{
    public class TextFileLogger : ILogger
    {
        public static void Log(string message)
        {
            try
            {
                File.AppendAllText("log.log", message);
                //добавить лимит строк и создание нового файла (например)
            }
            catch (System.Exception)
            {
                System.Console.WriteLine("Не удалось записать в файл!");
            }     
        }
    }
}