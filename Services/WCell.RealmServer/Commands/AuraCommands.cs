using System;
using WCell.Constants.Updates;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	public class AuraCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Aura", "Auras");
			EnglishDescription = "Provides commands to manage Auras (ie Buffs, Debuffs, passive effects).";
		}

		public class ClearAurasCommand : SubCommand
		{
			protected ClearAurasCommand() { }

			protected override void Initialize()
			{
				Init("Clear");
				EnglishDescription = "Removes all visible Auras";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				trigger.Args.Target.Auras.ClearVisibleAuras();
				trigger.Reply("All visible Auras removed.");
			}
		}

		#region Dump
		public class DumpAurasCommand : SubCommand
		{
			protected DumpAurasCommand() { }

			protected override void Initialize()
			{
				base.Init("Dump");
				EnglishParamInfo = "[<alsoPassive>]";
				EnglishDescription = "Dumps all currently active Auras, also shows passive effects if second param is specified";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var auras = trigger.Args.Target.Auras;
				if (auras != null && auras.Count > 0)
				{
					var alsoPassive = trigger.Text.NextBool();
					trigger.Reply("{0}'s Auras:", auras.Owner.Name);
					foreach (var aura in auras)
					{
						if (alsoPassive || !aura.Spell.IsPassive)
						{
							trigger.Reply("	{0}{1}", aura.Spell, aura.HasTimeout ? " [" + TimeSpan.FromMilliseconds(aura.TimeLeft).Format() + "]" : "");
						}
					}
				}
				else
				{
					trigger.Reply("{0} has no active Auras.", trigger.Args.Target.Name);
				}
			}
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.Unit; }
		}
		#endregion
	}
}