using WCell.Constants;
using WCell.Constants.Factions;
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
		[DependentInitialization(typeof(GOMgr))]
		public static void InitSpells()
		{
			SpellHandler.Apply(spell =>
			{
				spell.HasHarmfulEffects = true;
				spell.GetEffect(SpellEffectType.SummonObjectWild).SpellEffectHandlerCreator = (cast, effect) => new SummonRookeryWhelpHandler(cast, effect);
			},SpellId.CreateRookerySpawner);
		}

		public class SummonRookeryWhelpHandler : SpellEffectHandler
		{
			public SummonRookeryWhelpHandler(SpellCast cast, SpellEffect effect) : base(cast,effect)
			{
			}

			protected override void Apply(WorldObject target)
			{
				var whelp = NPCMgr.GetEntry(NPCId.RookeryWhelp);
				whelp.AllianceFactionId = FactionTemplateId.Enemy;
				whelp.HordeFactionId = FactionTemplateId.Enemy;
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