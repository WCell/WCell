using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCell.Database
{
	//TODO: Find a better name than this rubbish
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class ShouldAutoMap : Attribute
	{
	}
}
