using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace WCell.Util.Logging
{
    public abstract class LoggerBase : ILogger, IDisposable
    {

        public static bool IsMessageQueuingEnabled;

        public static bool _IsWarnEnabled = true;
        public static bool _IsDebugEnabled = true;

        public bool IsWarnEnabled;
        public bool IsDebugEnabled;

        public static Action<Action<string>> SystemInfoLogger;

        public static event Action<string, Exception> ExceptionRaised;

        public void ErrorException(Exception e)
        {
            ErrorException(e, false);
        }

        public void ErrorException(Exception e, bool addSystemInfo)
        {
            ErrorException(e, addSystemInfo, "");
        }

        public void ErrorException(string msg, params object[] format)
        {
            ErrorException(false, msg, format);
        }

        public void ErrorException(bool addSystemInfo, string msg, params object[] format)
        {
            LogException(Error, null, addSystemInfo, msg, format);
        }

        public void ErrorException(Exception e, string msg, params object[] format)
        {
            ErrorException(e, true, msg, format);
        }

        public void ErrorException(Exception e, bool addSystemInfo, string msg, params object[] format)
        {
            LogException(Error, e, addSystemInfo, msg, format);
        }

        public void WarnException(Exception e)
        {
            WarnException(e, false);
        }

        public void WarnException(Exception e, bool addSystemInfo)
        {
            WarnException(e, addSystemInfo, "");
        }

        public void WarnException(string msg, params object[] format)
        {
            WarnException(false, msg, format);
        }

        public void WarnException(bool addSystemInfo, string msg, params object[] format)
        {
            LogException(Warn, null, addSystemInfo, msg, format);
        }

        public void WarnException(Exception e, string msg, params object[] format)
        {
            WarnException(e, true, msg, format);
        }

        public void WarnException(Exception e, bool addSystemInfo, string msg, params object[] format)
        {
            LogException(Warn, e, addSystemInfo, msg, format);
        }

        public void FatalException(string msg, Exception e, params object[] format)
        {
            FatalException(e, true, msg, format);
        }

        public void FatalException(Exception e, string msg, params object[] format)
        {
            FatalException(e, true, msg, format);
        }

        public void FatalException(Exception e, bool addSystemInfo)
        {
            FatalException(e, addSystemInfo, "");
        }

        public void FatalException(Exception e, bool addSystemInfo, string msg, params object[] format)
        {
            LogException(Fatal, e, addSystemInfo, msg, format);
        }

        public static void LogException(Action<string> logger, Exception e, bool addSystemInfo, string msg, params object[] format)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                msg = string.Format(msg, format);
                logger(msg);
            }

            if (e != null)
            {
                LogStacktrace(logger);
                logger("");
                logger(e.ToString());
                //logger(new StackTrace(e, true));
            }

            if (addSystemInfo)
            {
                logger("");
                SystemInfoLogger(logger);
            }

            if (e != null)
            {
                logger("");
                logger(e.GetAllMessages().ToString("\n\t"));
            }

            var evt = ExceptionRaised;
            if (evt != null)
            {
                evt(msg, e);
            }
        }

        public static void LogStacktrace(Action<string> logger)
        {
            logger(new StackTrace(Thread.CurrentThread, true).GetFrames().ToString("\n\t", frame => frame.ToString().Trim()));
        }

        public abstract void Info(string message);
        public void Info(string message, params object[] arg)
        {
            message = String.Format(message, arg);
            Info(message);
        }

        public abstract void Warn(string message);
        public void Warn(string message, params object[] arg)
        {
            message = String.Format(message, arg);
            Warn(message);
        }

        public abstract void Debug(string message);
        public void Debug(string message, params object[] arg)
        {
            message = String.Format(message, arg);
            Debug(message);
        }

        public abstract void Error(string message);
        public void Error(string message, params object[] arg)
        {
            message = String.Format(message, arg);
            Error(message);
        }

        public abstract void Fatal(string message);
        public void Fatal(string message, params object[] arg)
        {
            message = String.Format(message, arg);
            Fatal(message);
        }

        public abstract void FlushMessageQueues();
        public abstract void FlushFileWriters();

        public abstract void Dispose();
    }
}
