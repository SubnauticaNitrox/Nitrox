using System;
using System.IO;

namespace NitroxModel.Logger
{
    public class LogFiles
    {
        public const string DIRECTORY = "Nitrox Logs";
        public const string FILENAME = "nitroxlog";
        public const string EXTENSION = ".txt";
        private static LogFiles instance;

        public static LogFiles Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LogFiles();
                }

                return instance;
            }
        }

        private LogFiles()
        {
        }

        public StreamWriter CreateNew()
        {
            Directory.CreateDirectory(DIRECTORY);

            int num = 0;
            string name = GetCurrentFileName();
            while (File.Exists(name))
            {
                name = GetCurrentFileName(++num);
            }

            return File.CreateText(GetCurrentFileName(num));
        }

        private string GetCurrentFileName(int num = 0)
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            if (num > 0)
            {
                return Path.Combine(DIRECTORY, $"{FILENAME} {date} ({num + 1}){EXTENSION}");
            }

            return Path.Combine(DIRECTORY, $"{FILENAME} {date}{EXTENSION}");
        }
    }
}
