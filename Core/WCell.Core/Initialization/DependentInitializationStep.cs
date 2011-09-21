namespace WCell.Core.Initialization
{
	public class DependentInitializationStep
	{
		public readonly InitializationStep Step;

		public readonly InitializationDependency[] Dependency;

		public DependentInitializationStep(InitializationStep step, InitializationDependency[] dependency)
		{
			Step = step;
			Dependency = dependency;
		}
	}
}