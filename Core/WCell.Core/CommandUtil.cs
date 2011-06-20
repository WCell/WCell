using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Variables;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Util.Variables;
using WCell.Core.Addons;

namespace WCell.Core
{
	/// <summary>
	/// Contains methods that are needed for both, Auth- and RealmServer commands
	/// </summary>
	public static class CommandUtil
	{
		public static IConfiguration GetConfig(IConfiguration deflt, ITriggerer trigger)
		{
			IConfiguration cfg;
			if (trigger.Text.NextModifiers() == "a")
			{
				var shortName = trigger.Text.NextWord();
				var addon = WCellAddonMgr.GetAddon(shortName);
				if (addon == null)
				{
					trigger.Reply("Did not find any Addon matching: " + shortName);
					return null;
				}

				cfg = addon.Config;
				if (cfg == null)
				{
					trigger.Reply("Addon does not have a Configuration: " + addon);
					return null;
				}
			}
			else
			{
				cfg = deflt;
			}
			return cfg;
		}

		public static bool SetCfgValue(IConfiguration cfg, ITriggerer trigger)
		{
			if (trigger.Text.HasNext)
			{
				var name = trigger.Text.NextWord();
				var value = trigger.Text.Remainder;
				if (value.Length == 0)
				{
					trigger.Reply("No arguments given.");
					return false;
				}

				if (cfg.Contains(name))
				{
					//if (def.MaySet(trigger.Args.Role))
					if (cfg.Set(name, value))
					{
						trigger.Reply("Variable \"{0}\" is now set to: " + value, name);
						return true;
					}
					else
					{
						trigger.Reply("Unable to set Variable \"{0}\" to value: {1}.", name, value);
					}
					//else
					//{
					//    trigger.Reply("You do not have enough privileges to set Variable \"{0}\".", def.Name);
					//}
				}
				else
				{
					trigger.Reply("Variable \"{0}\" does not exist.", name);
				}
			}
			return false;
		}

		public static bool GetCfgValue(IConfiguration cfg, ITriggerer trigger)
		{
			if (trigger.Text.HasNext)
			{
				var name = trigger.Text.NextWord();

				//if (def.MaySet(trigger.Args.Role))
				var val = cfg.Get(name);
				if (val != null)
				{
					if (cfg is VariableConfiguration<WCellVariableDefinition>)
					{
						var vcfg = (VariableConfiguration<WCellVariableDefinition>)cfg;
						var def = vcfg.GetDefinition(name);
						if (def.IsFileOnly)
						{
							trigger.Reply("Cannot display variable: \"{0}\"", name);
							return false;
						}
					}
					trigger.Reply("Variable {0} = {1}", name, Utility.GetStringRepresentation(val));
					return true;
				}

				trigger.Reply("Variable \"{0}\" does not exist.", name);
				//else
				//{
				//    trigger.Reply("You do not have enough privileges to set Variable \"{0}\".", def.Name);
				//}
				return true;
			}

			trigger.Reply("No arguments given.");
			return false;
		}

		public static void ListCfgValues(IConfiguration cfg, ITriggerer trigger)
		{
			var vars = new List<IVariableDefinition>(50);

			var filter = trigger.Text.Remainder;
			if (filter.Length > 0)
			{
				cfg.Foreach((def) =>
				{
					if (!def.IsFileOnly && def.Name.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0)
					{
						vars.Add(def);
					}
				});

				if (vars.Count == 0)
				{
					trigger.Reply("Could not find any readable globals that contain \"{0}\".", filter);
				}
			}
			else
			{
				cfg.Foreach(def =>
				{
					if (!def.IsFileOnly)
					{
						vars.Add(def);
					}
				});

				if (vars.Count == 0)
				{
					trigger.Reply("No readable variables found.");
				}
			}

			if (vars.Count > 0)
			{
				vars.Sort();

				trigger.Reply("Found {0} globals:", vars.Count);
				foreach (var var in vars)
				{
					trigger.Reply(var.Name + " = " + var.GetFormattedValue());
				}
			}
		}
	}
}