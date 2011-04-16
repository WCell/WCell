using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Core.Commands
{
	public interface ITriggerer
	{
		void Reply(string text);

		void ReplyFormat(string text);
	}
}