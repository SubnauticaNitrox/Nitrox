using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NitroxClient
{
    class FileLogger
    {
        private static Boolean loggingEnabled = true;
        protected static readonly object lockObj = new object();
        public static string filePath = @"C:\Users\Sunrunner\Desktop\t\logs.txt";

        public static void LogInfo(string message)
        {
            Log("[INFO] " + message);
        }

        public static void LogError(string message, Exception ex)
        {
            Log("[ERROR] " + message + " " + ex.ToString());
        }

        private static void Log(string message)
        {
            Console.WriteLine(message);

            if (loggingEnabled)
            {
                lock (lockObj)
                {
                    try
                    {
                        File.AppendAllText(filePath, message + "\r\n");
                    }
                    catch (Exception ex)
                    {
                        loggingEnabled = false;
                    }
                }
            }
        }
    }
    
}
