using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
	public class SpellSummonHandler
	{
		public virtual bool CanSummon(SpellCast cast, NPCEntry entry)
		{
			return true;
		}

		public virtual NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
		{
			var caster = cast.CasterUnit;
			var pet = caster.SpawnMinion(entry, ref targetLoc, cast.Spell.GetDuration(caster.CasterInfo));
			pet.Summoner = caster;
			pet.Creator = caster.EntityId;
			return pet;
		}
	}

	/// <summary>
	/// Non-combat pets
	/// </summary>
	public class SpellSummonCritterHandler : SpellSummonHandler
	{
		public override NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
		{
			var pet = base.Summon(cast, ref targetLoc, entry);
			return pet;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class SpellSummonPetHandler : SpellSummonHandler
	{
		public override NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
		{
			var caster = cast.CasterUnit;
			var pet = ((Character)caster).SpawnPet(entry, ref targetLoc, cast.Spell.GetDuration(caster.CasterInfo));
			return pet;
		}
	}

	public class SpellSummonImmovableHandler : SpellSummonHandler
	{
		public override NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
		{
			var npc = base.Summon(cast, ref targetLoc, entry);
			npc.MayMove = false;
			return npc;
		}
	}

	public class SpellSummonTotemHandler : SpellSummonHandler
	{
		public SpellSummonTotemHandler(uint index)
		{
			Index = index;
		}

		public uint Index
		{
			get;
			private set;
		}
	}

    public class SpellSummonDoomguardHandler : SpellSummonHandler
    {
        public override NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
        {
            var npc = entry.Create(cast.CasterUnit.Region, targetLoc);
			npc.RemainingDecayDelay = cast.Spell.GetDuration(cast.Caster.CasterInfo);
            return npc;
        }
    }
}
