using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Commands
{
	public interface IBufferedCommandResponse
	{
		List<string> Replies
		{
			get;
			set;
		}
	}
}