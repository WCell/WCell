using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Spells
{
	public enum RuneType : byte
	{
		Blood = 0,
		Unholy,
		Frost,
		Death,
		End
	}

	[Flags]
	public enum RuneMask : byte
	{
		Blood = 1,
		Unholy = 2,
		Frost = 4,
		Death = 8,
		End
	}
}
