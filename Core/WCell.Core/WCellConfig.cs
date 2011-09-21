using System;
using WCell.Core.Variables;
using WCell.Util.Variables;

namespace WCell.Core
{
	public class WCellConfig<C> : VariableConfiguration<C, WCellVariableDefinition>
		where C : VariableConfiguration<WCellVariableDefinition>
	{
		public WCellConfig(Action<string> onError) : base(onError)
		{
		}
	}
}