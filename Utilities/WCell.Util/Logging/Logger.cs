using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;

namespace WCell.Util.Logging
{
    public class Logger : LoggerBase
    {
        private readonly StreamWriter[] logFiles = new StreamWriter[(int)LogManager.LogLevels.End];
        private readonly List<Queue<string>> messageQueues = new List<Queue<string>>((int)LogManager.LogLevels.End);

        public Logger()
        {
            IsWarnEnabled = _IsWarnEnabled;
            IsDebugEnabled = _IsDebugEnabled;
            //Get the path where the exe resides
            var location = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            //Get the path to the logs directory
            var logPath = Path.Combine(location, "logs");

            //Check it exists, if not create it
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            for (var i = 0; i < (int)LogManager.LogLevels.End; i++)
            {
                //Name the file as Assembly name + LogLevel e.g WCell.RealmServer.exe.Info
                var fileName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name + "_" + (LogManager.LogLevels) i + ".log";
                var filePath = Path.Combine(logPath, fileName);

                logFiles[i] = new StreamWriter(filePath, true);
                if (!IsMessageQueuingEnabled)
                    logFiles[i].AutoFlush = true;

                messageQueues.Add(new Queue<string>());
            }
        }

        public override void Info(string message)
        {
            message = String.Format("[{0}][{1}]{2}", DateTime.Now, LogManager.LogLevels.Info, message);

            if (IsMessageQueuingEnabled)
                messageQueues[(int)LogManager.LogLevels.Info].Enqueue(message);
            else
                logFiles[(int)LogManager.LogLevels.Info].WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        
        public override void Warn(string message)
        {
            message = String.Format("[{0}][{1}]{2}", DateTime.Now, LogManager.LogLevels.Warn, message);

            if (IsMessageQueuingEnabled)
                messageQueues[(int)LogManager.LogLevels.Warn].Enqueue(message);
            else
                logFiles[(int)LogManager.LogLevels.Warn].WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        
        public override void Debug(string message)
        {
#if DEBUG
            message = String.Format("[{0}][{1}]{2}", DateTime.Now, LogManager.LogLevels.Debug, message);

            if (IsMessageQueuingEnabled)
                messageQueues[(int)LogManager.LogLevels.Debug].Enqueue(message);
            else
                logFiles[(int)LogManager.LogLevels.Debug].WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(message);
            Console.ResetColor();
#endif
        }

        public override void Error(string message)
        {
            message = String.Format("[{0}][{1}]{2}", DateTime.Now, LogManager.LogLevels.Error, message);

            if (IsMessageQueuingEnabled)
                messageQueues[(int)LogManager.LogLevels.Error].Enqueue(message);
            else
                logFiles[(int)LogManager.LogLevels.Error].WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public override void Fatal(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(message);
            Console.ResetColor();

            message = String.Format("[{0}]{1}", DateTime.Now, message);

            if (IsMessageQueuingEnabled)
                messageQueues[(int)LogManager.LogLevels.Fatal].Enqueue(message);
            else
                logFiles[(int)LogManager.LogLevels.Fatal].WriteLine(message);
        }

        public override void FlushMessageQueues()
        {
            for (var i = 0; i > (int)LogManager.LogLevels.End; i++)
            {
                foreach (var message in messageQueues)
                {
                    logFiles[i].WriteLine(message.Dequeue());
                }
            }
        }

        public override void FlushFileWriters()
        {
            for (var i = 0; i > (int)LogManager.LogLevels.End; i++)
            {
                logFiles[i].Flush();
            }
        }

        public override void Dispose()
        {
            if(IsMessageQueuingEnabled)
                FlushMessageQueues();

            foreach(var file in logFiles)
            {
                file.Close();
            }
        }
    }

}