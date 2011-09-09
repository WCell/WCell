using System.Collections.Generic;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Content;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs.Trainers
{
    [DataHolder]
    public class TrainerSpellTemplate : TrainerSpellEntry, IDataHolder
    {
        public uint TrainerTemplateId;

        #region IDataHolder Members

        public new void FinalizeDataHolder()
		{
			if ((Spell = SpellHandler.Get(SpellId)) == null)
			{
				ContentMgr.OnInvalidDBData("SpellId is invalid in " + this);
			}
			else if (RequiredSpellId != SpellId.None && SpellHandler.Get(RequiredSpellId) == null)
			{
				ContentMgr.OnInvalidDBData("RequiredSpellId is invalid in " + this);
			}
            else if (RequiredSkillId != SkillId.None && SkillHandler.Get(RequiredSkillId) == null)
            {
                ContentMgr.OnInvalidDBData("RequiredSkillId is invalid in " + this);
            }
            else
            {
                if (RequiredLevel == 0)
                {
                    RequiredLevel = Spell.Level;
                }

                if (!NPCMgr.TrainerSpellTemplates.ContainsKey(TrainerTemplateId))
                    NPCMgr.TrainerSpellTemplates.Add(TrainerTemplateId, new List<TrainerSpellEntry>());

                NPCMgr.TrainerSpellTemplates[TrainerTemplateId].Add(this);

            }
		}

		#endregion
    }
}
