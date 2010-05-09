using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.Constants.World;
using WCell.RealmServer.Global;
using System;
using WCell.Util.Data;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;

namespace WCell.RealmServer.GameObjects
{
	/// <summary>
	/// Walk-in portal
	/// </summary>
	public class GOPortalEntry : GOCustomEntry
	{
		public static int UpdateDelayMillis = 2000;

		public const GOEntryId PortalId = GOEntryId.Portal;

		public GOPortalEntry()
		{
			Id = (uint) PortalId;
			DisplayId = 4396;
			UseHandler = OnUse;
			DefaultName = "Portal";
			FactionId = FactionTemplateId.Friendly;
			Type = GameObjectType.SpellCaster;
			UseHandler = OnUse;
			GOCreator = () => new Portal();

			//AreaEffectHandler = Teleport;
			//UpdateTicks = UpdateDelayMillis / Region.DefaultUpdateDelay;
		}

		private static void Teleport(GameObject go, Character chr)
		{
			if (go.Handler.CanBeUsedBy(chr))
			{
				// We need to enqueue a message when moving/removing/adding Objects in this method
				var portal = (Portal)go;
				chr.AddMessage(() => chr.TeleportTo(portal.Target));
			}
		}

		private static bool OnUse(GameObject go, Character chr)
		{
			var portal = (Portal)go;
			chr.TeleportTo(portal.Target);
			return true;
		}
	}
}