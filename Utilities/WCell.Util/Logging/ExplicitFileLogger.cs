using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace WCell.Util.Logging
{
    public class ExplicitFileLogger : LoggerBase
    {
        private readonly StreamWriter _logFile;
        private readonly Queue<string> _messageQueue = new Queue<string>();

        public ExplicitFileLogger(string fileName)
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

            var filePath = Path.Combine(logPath, fileName);

            _logFile = new StreamWriter(filePath, true);

            if (!IsMessageQueuingEnabled)
                _logFile.AutoFlush = true;
        }

        public override void Info(string message)
        {
            message = String.Format("[{0}][{1}]{2}", DateTime.Now, LogManager.LogLevels.Info, message);

            if (IsMessageQueuingEnabled)
                _messageQueue.Enqueue(message);
            else
                _logFile.WriteLine(message);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public override void Warn(string message)
        {
            message = String.Format("[{0}][{1}]{2}", DateTime.Now, LogManager.LogLevels.Warn, message);

            if (IsMessageQueuingEnabled)
                _messageQueue.Enqueue(message);
            else
                _logFile.WriteLine(message);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public override void Debug(string message)
        {
#if DEBUG
            message = String.Format("[{0}][{1}]{2}", DateTime.Now, LogManager.LogLevels.Debug, message);

            if (IsMessageQueuingEnabled)
                _messageQueue.Enqueue(message);
            else
                _logFile.WriteLine(message);

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(message);
            Console.ResetColor();
#endif
        }

        public override void Error(string message)
        {
            message = String.Format("[{0}][{1}]{2}", DateTime.Now, LogManager.LogLevels.Error, message);

            if (IsMessageQueuingEnabled)
                _messageQueue.Enqueue(message);
            else
                _logFile.WriteLine(message);

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
                _messageQueue.Enqueue(message);
            else
                _logFile.WriteLine(message);
        }

        public override void FlushMessageQueues()
        {
            foreach (var message in _messageQueue)
            {
                _logFile.WriteLine(message);
            }
        }

        public override void FlushFileWriters()
        {
            _logFile.Flush();
        }

        public override void Dispose()
        {
            if (IsMessageQueuingEnabled)
                FlushMessageQueues();

            _logFile.Close();
        }
    }
}
