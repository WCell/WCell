using System;
using WCell.Util.Strings;

namespace WCell.Util.Commands
{
    public interface ITriggerer
    {
        StringStream Text
        {
            get;
        }

        void Reply(string text);

        void Reply(string format, params Object[] args);

        void ReplyFormat(string text);

		void ReplyFormat(string format, params Object[] args);
    }
}