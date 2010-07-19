using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.DynamicAccess
{
	/// <summary>
	/// All properties with this attribute should not be copied automatically, when using PropertyAccessMgr.CopyAll
	/// </summary>
	public class DontCopyAttribute : Attribute
	{
	}
}