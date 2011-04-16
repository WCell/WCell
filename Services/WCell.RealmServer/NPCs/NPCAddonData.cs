using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs
{
	public class NPCAddonData
	{
		[NotPersistent]
		public INPCDataHolder DataHolder;

		public uint Bytes;
		public uint Bytes2;
		public EmoteType EmoteState;
		public uint MountModelId;
		public string AuraIdStr;

		[NotPersistent]
		public List<Spell> Auras;

		public uint SheathType
		{
			get { return (byte)(Bytes2 >> (NPCConstants.SheathTypeIndex * 8)); }
			set
			{
				const int pos = NPCConstants.SheathTypeIndex * 8;
				Bytes2 = (Bytes2 & ~(0xFFu << pos)) | (value << pos);
			}
		}

		public uint PvPState
		{
			get { return (byte)(Bytes2 >> (NPCConstants.PvpStateIndex * 8)); }
			set
			{
				const int pos = NPCConstants.PvpStateIndex * 8;
				Bytes2 = (Bytes2 & ~(0xFFu << pos)) | (value << pos);
			}
		}

		public void AddAura(SpellId spellId)
		{
			var spell = SpellHandler.Get(spellId);
			if (spell == null)
			{
				LogManager.GetCurrentClassLogger().Warn("Tried to add invalid Aura-Spell \"{0}\" to NPCEntry: {1}", spellId, this);
			}
			else
			{
				Auras.Add(spell);
			}
		}

		#region Init
		internal void InitAddonData(INPCDataHolder dataHolder)
		{
			DataHolder = dataHolder;

			var entry = dataHolder.Entry;
			if (!string.IsNullOrEmpty(AuraIdStr))
			{
				var auraIds = AuraIdStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).TransformArray(idStr =>
				{
					uint id;
					if (!uint.TryParse(idStr.Trim(), out id))
					{
						LogManager.GetCurrentClassLogger().Warn("Invalidly formatted Aura ({0}) in AuraString for SpawnEntry: {1}", idStr, this);
					}
					return (SpellId)id;
				});
				if (auraIds != null)
				{
					Auras = new List<Spell>(auraIds.Length);
					foreach (var auraId in auraIds)
					{
						var spell = SpellHandler.Get(auraId);
						if (spell != null)
						{
							if (!spell.IsAura || (spell.Durations.Min > 0 && spell.Durations.Min < int.MaxValue))
							{
								// not permanent -> cast as Spell
								if (entry.Spells == null || !entry.Spells.ContainsKey(spell.SpellId))
								{
									entry.AddSpell(spell);
								}
							}
							else
							{
								Auras.Add(spell);
							}
						}
					}
				}
			}
			if (Auras == null)
			{
				Auras = new List<Spell>(2);
			}
		}
		#endregion
	}
}
