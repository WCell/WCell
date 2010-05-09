using WCell.Constants.GameObjects;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	public class SummonObjectSlot1Handler : SummonObjectEffectHandler
	{
		public SummonObjectSlot1Handler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Apply()
		{
			var caster = m_cast.CasterUnit as Character;
			if (caster != null)
			{
				// Remove previously existing GOs
				var oldGO = caster.GetOwnedGO(Slot);
				if (oldGO != null)
				{
					oldGO.Delete();
				}
				base.Apply();
			    GO.Entry.SummonSlotId = Slot;
				caster.AddOwnedGO(GO);
			}
			else
			{
				base.Apply();
			}
		}

		public virtual uint Slot
		{
			get { return 1; }
		}
	}

	public class SummonObjectSlot2Handler : SummonObjectSlot1Handler
	{
		public SummonObjectSlot2Handler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{

		}

		public override uint Slot
		{
			get { return 2; }
		}
	}

	public class SummonObjectSlot3Handler : SummonObjectSlot1Handler
	{
		public SummonObjectSlot3Handler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{

		}

		public override uint Slot
		{
			get { return 3; }
		}
	}

	public class SummonObjectSlot4Handler : SummonObjectSlot1Handler
	{
		public SummonObjectSlot4Handler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{

		}

		public override uint Slot
		{
			get { return 4; }
		}
	}
}
