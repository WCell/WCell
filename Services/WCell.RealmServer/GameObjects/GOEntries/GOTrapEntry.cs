using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.GameObjects.GOEntries
{
	public class GOTrapEntry : GOEntry, ISpellParameters
	{
	    public uint Open
	    {
            get { return Fields[0]; }
	    }

	    public uint Level
	    {
            get { return Fields[1]; }
	    }

		/// <summary>
		/// The explosion radius of this Trap in yards (Assume default if 0)
		/// </summary>
		public uint Radius
		{
			get
			{
                return Fields[2];
			}
			set { }
		}

        public SpellId SpellId
        {
            get { return (SpellId)Fields[3]; }
        }

		public Spell Spell
		{
			get;
			set;
		}
		
		/// <summary>
		/// Probably maximum charges (trap disappears after all charges have been used)
		/// </summary>
		public uint MaxCharges
		{
            get { return Fields[4]; }
		}

		public int Amplitude
		{
			get
			{
                // change from seconds to millis
                return (int) (Fields[5] * 1000);	
			}
			set { }
		}

        public uint AutoClose
        {
            get { return Fields[6]; }
        }

		/// <summary>
		/// Trigger-delay in seconds
		/// </summary>
		public uint StartDelay
		{
			get
			{
                // change from seconds to millis
			    return Fields[7] * 1000;
			}
			set { }
		}

        public uint ServerOnly
        {
            get { return Fields[8]; }
        }
		/// <summary>
		/// Whether this trap is stealthed
		/// </summary>
        public bool Stealthed
        {
            get { return Fields[9] > 0; }
        }
        public uint Large
        {
            get { return Fields[10]; }
        }
        public uint StealthAffected
        {
            get { return Fields[11]; }
        }
	    public uint OpenTextID
	    {
            get { return Fields[12]; }
	    }
	    public uint CloseTextID
	    {
            get { return Fields[13]; }
	    }

	    public uint IgnoreTotems
	    {
            get { return Fields[14]; }
	    }

		protected internal override void InitEntry()
		{
			if (Radius < 1)
			{
				Radius = 5;
			}

			Spell = SpellHandler.Get(SpellId);

			//if (Spell == null)
			//{
			//    ContentHandler.OnInvalidData("Trap {0} had invalid Spell: {1}", this, SpellId);
			//    Fields = null;
			//    return;
			//}
		}

		protected internal override void InitGO(GameObject trap)
		{
			// init Trap
			if (trap.AreaAura == null)
			{
				if (Spell != null)
				{
					new AreaAura(trap, Spell, this);
				}
				trap.IsTrap = true;
			}
		}
	}
}
