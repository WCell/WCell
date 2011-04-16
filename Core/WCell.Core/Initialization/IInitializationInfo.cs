using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Core.Initialization
{
	public interface IInitializationInfo
	{
		string Name { get; }

		bool IsRequired { get; }
	}
}