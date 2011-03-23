using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Logging
{
    interface ILogger
    {
        void Info(string message);
        void Info(string message, params object[] arg);

        void Warn(string message);
        void Warn(string message, params object[] arg);

        void Debug(string message);
        void Debug(string message, params object[] arg);

        void Error(string message);
        void Error(string message, params object[] arg);

        void Fatal(string message);
        void Fatal(string message, params object[] arg);

        void FlushMessageQueues();
        void FlushFileWriters();
    }
}
