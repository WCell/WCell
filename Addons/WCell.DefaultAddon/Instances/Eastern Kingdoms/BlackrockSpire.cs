using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Instances;
///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 6/11/2009
///
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.Util.Graphics;

namespace WCell.Addons.Default.Instances
{
	public class BlackrockSpire : BaseInstance
	{
		[Initialization]
		[DependentInitialization(typeof (GOMgr))]
		public static void InitGOs()
		{
			var rookeryEgg = GOMgr.GetEntry(GOEntryId.RookeryEgg) as GOTrapEntry;
			if (rookeryEgg != null)
			{
				var rookerySpell = SpellHandler.Get(SpellId.SummonRookeryWhelp);

				rookeryEgg.Fields[3] = 15745; // GO Entry Rookery Egg Id: 175124 -- Spell: Summon Rookery Whelp Note: Unsure this has any affect but best update it anyway.
				//rookeryEgg.Type = GameObjectType.SpellCaster;
				rookeryEgg.Spell = rookerySpell;
			}
		}

		[Initialization]
		[DependentInitialization(typeof(GOMgr))]
		public static void InitSpells()
		{
			SpellHandler.Apply(spell =>
			{
				spell.GetEffect(SpellEffectType.Summon).SpellEffectHandlerCreator = (cast, effect) => new SummonRookeryWhelpHandler(cast, effect);
			}, SpellId.SummonRookeryWhelp);
		}

		public class SummonRookeryWhelpHandler : SpellEffectHandler
		{
			public SummonRookeryWhelpHandler(SpellCast cast, SpellEffect effect) : base(cast,effect)
			{
			}

			protected override void Apply(WorldObject target)
			{
				var whelp = NPCMgr.GetEntry(NPCId.RookeryWhelp);
				whelp.UnitFlags = UnitFlags.Combat;
				whelp.SpawnAt(target.Map, target.Position);
			}

			public override ObjectTypes CasterType
			{
				get { return ObjectTypes.GameObject; }
			}
		}
	}
}