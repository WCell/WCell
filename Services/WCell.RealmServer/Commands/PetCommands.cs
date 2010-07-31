namespace WCell.RealmServer.Commands
{
	public class PetCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Pet");
			EnglishDescription = "A set of commands to manage pets.";
		}


	}
}