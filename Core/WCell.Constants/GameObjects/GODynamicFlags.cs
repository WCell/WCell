using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.GameObjects
{
	[Flags]
	public enum GODynamicLowFlags : ushort
	{
		None,
		Clickable = 0x01,                 
		Animated = 0x02,
		NotClickable = 0x04,
		Sparkle = 0x08,                 // sparkle sparkle
	}
}
