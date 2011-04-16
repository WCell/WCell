using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.DynamicAccess
{
	public interface IExecutable
	{
		string Name { get; set; }

		Type[] ParameterTypes { get; }

		void Exec(params object[] args);
	}
}