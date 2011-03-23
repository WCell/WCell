using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Logging
{
    public static class LogManager
    {
        public static Logger Logger = new Logger();
        public static Dictionary<string, ExplicitFileLogger> ExplicitFileLoggers = new Dictionary<string, ExplicitFileLogger>();

        public enum LogLevels
        {
            Info,
            Warn,
            Debug,
            Error,
            Fatal,
            End
        }

        public static Logger GetCurrentClassLogger()
        {
            return Logger;
        }

        public static ExplicitFileLogger GetLogger(string filename)
        {
            ExplicitFileLogger logger;
            if(ExplicitFileLoggers.TryGetValue(filename, out logger))
                return logger;
            
            logger = new ExplicitFileLogger(filename);
            ExplicitFileLoggers.Add(filename, logger);
            return logger;
        }
    }
}
