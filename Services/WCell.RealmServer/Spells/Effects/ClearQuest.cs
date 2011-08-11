using WCell.Constants.Updates;

namespace WCell.RealmServer.Spells.Effects
{
	public class ClearQuestEffectHandler : SpellEffectHandler
	{
		public ClearQuestEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override ObjectTypes CasterType
		{
			get
			{
				return ObjectTypes.Player;
			}
		}

		public override void Apply()
		{
			var questId = (uint)Effect.MiscValue;
			var chr = Cast.CasterChar;
			if (chr != null)
			{
				var quest = chr.QuestLog.GetActiveQuest(questId);
				if (quest != null)
				{
					quest.Cancel(false);
				}
			}
		}
	}
}
