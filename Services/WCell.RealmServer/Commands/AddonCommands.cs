using System.IO;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;
using WCell.RealmServer.Addons;
using WCell.Core.Addons;

namespace WCell.RealmServer.Commands
{
    public class AddonCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            Init("Addon");
			EnglishDescription = "Provides commands for managing Addons";
        }

        public class ListAddonsCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("List", "L");
                EnglishParamInfo = "[-l]";
                EnglishDescription = "Lists all active Addons. -l to also list libraries.";
            }
            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var i = 0;
                var mod = trigger.Text.NextModifiers();
                var lib = mod.Contains("l");
                foreach (var context in WCellAddonMgr.Contexts)
                {
                    ++i;
                    if (context.Addon != null)
                    {
						trigger.Reply(i + ". " + trigger.Translate(RealmLangKey.Addon) + " " + context.Addon);
                    }
                    else if (lib)
                    {
						trigger.Reply(i + ". " + trigger.Translate(RealmLangKey.Library) + " " + context.Assembly);
                    }
                }
            }
		}

		#region Load
		public class LoadAddonCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Load");
                EnglishParamInfo = "<Path>";
                EnglishDescription = "Loads a new Addon from the given file.";
            }

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                //var mod = trigger.Text.NextModifiers();

                var path = trigger.Text.NextWord();
                if (path.Length == 0)
                {
                    trigger.Reply("No Path given.");
                }
                else
                {
					trigger.Reply("Loading addon from " + path + "...");
					var context = RealmAddonMgr.Instance.TryLoadAddon(path);
					if (context == null)
					{
						trigger.Reply("File does not exist or has invalid format: " + path);
					}
					else
					{
						trigger.Reply("Done: " + context);
					}
                }
            }
		}
		#endregion
	}
}