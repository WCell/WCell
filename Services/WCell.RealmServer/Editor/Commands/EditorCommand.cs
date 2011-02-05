using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Editor.Commands
{
	/// <summary>
	/// Is not in the default Commands/ folder because this will be moved into it's own Addon
	/// </summary>
	public class EditorCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Editor", "Edit");
			EnglishDescription = "Allows Staff members to edit spawns, waypoints etc.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var chr = trigger.Args.Target as Character;

			if (!trigger.Text.HasNext)
			{
				// start editing & open the editor menu
				if (chr != null)
				{
					var editor = MapEditorMgr.StartEditing(chr.Map, chr);
					trigger.ShowMenu(editor.Menu);
				}
			}
			else
			{
				// trigger sub commands
				base.Process(trigger);
			}
		}
	}
}
