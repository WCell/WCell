using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOSpellCasterEntry : GOEntry
    {
		private static readonly Logger sLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The SpellId
        /// </summary>
        public SpellId SpellId
        {
            get { return (SpellId)Fields[0]; }
        }

		public Spell Spell;

        /// <summary>
        /// The number of times this can cast the Spell
        /// </summary>
        public int Charges
        {
            get
            {
                return Fields[1] == 0 ? 1 : Fields[1];
            }
        }

        /// <summary>
        /// Whether you must be in the same group as the caster to recieve the Spell effect.
        /// </summary>
		public override bool IsPartyOnly
		{
			get
			{
                return Fields[2] > 0;
			}
		}

        public bool Large
        {
            get { return Fields[4] != 0; }
        }

		protected internal override void InitEntry()
		{
			Spell = SpellHandler.Get(SpellId);

		    AllowMounted = Fields[3] != 0;
		}
	}
}