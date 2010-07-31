using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace WCell.Core.Initialization
{
	public class InitializationDependency
	{
		public readonly InitializationStep Step;

		public readonly DependentInitializationInfo[] Info;

		public InitializationDependency(InitializationStep step, DependentInitializationInfo[] info)
		{
			Step = step;
			Info = info;
		}
	}
}