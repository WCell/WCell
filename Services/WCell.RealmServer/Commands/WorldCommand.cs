using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Global;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	public class WorldCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("World");
			EnglishDescription = "Provides command to macro-manage the ingame World.";
		}

		#region Save
		public class SaveWorldCommand : SubCommand
		{
			public override RoleStatus DefaultRequiredStatus
			{
				get
				{
					return RoleStatus.Admin;
				}
			}

			protected override void Initialize()
			{
				Init("Save", "S");
				EnglishDescription = "Saves all current progress in the ingame world.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				World.Broadcast("Saving world...");
				World.Save(false);
				World.Broadcast("World saved.");
			}
		}
		#endregion

		public override bool RequiresCharacter
		{
			get
			{
				return false;
			}
		}

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.None;
			}
		}
	}
}