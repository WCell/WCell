using System.Collections.Generic;

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