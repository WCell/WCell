using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Commands
{
    public interface ITriggerer
    {
        StringStream Text
        {
            get;
        }

        void Reply(string text);

        void Reply(Object format, params Object[] args);

        void ReplyFormat(string text);

        void ReplyFormat(Object format, params Object[] args);
    }
}
