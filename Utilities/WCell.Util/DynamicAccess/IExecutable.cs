using System;

namespace WCell.Util.DynamicAccess
{
	public interface IExecutable
	{
		string Name { get; set; }

		Type[] ParameterTypes { get; }

		void Exec(params object[] args);
	}
}