using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Constants.Quests;

namespace WCell.RealmServer.Spells
{
	public class QuestCompleteEffectHandler : SpellEffectHandler
	{
		public QuestCompleteEffectHandler(SpellCast cast, SpellEffect effect) : base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			var chr = (Character)target;
			var quest = chr.QuestLog.GetActiveQuest((uint) Effect.MiscValue);
			if (quest != null)
			{
				quest.SignalSpellCasted(m_cast.Spell.SpellId);
			}
		}

		public override ObjectTypes TargetType
		{
			get
			{
				return ObjectTypes.Player;
			}
		}
	}
}
