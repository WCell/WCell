using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

/**
 * AI SpellCast behavior
 */


namespace WCell.RealmServer.Spells
{
	public partial class SpellCast
	{
		#region Prepare
		/// <summary>
		/// Called during Preparation
		/// </summary>
		SpellFailedReason PrepareAI()
		{
			var caster = CasterUnit;
			SourceLoc = caster.Position;

			if (caster.Target != null)
			{
				caster.SpellCast.TargetLoc = caster.Target.Position;
			}

			// init handlers pre-maturely, to make sure we got any targets
			// revalidate handlers again, later
			var err = PrepareHandlers();

			if (err == SpellFailedReason.Ok && Targets.Count == 0)
			{
				// NPC must have targets
				err = SpellFailedReason.NoValidTargets;
			}

			if (Targets.Count == 1)
			{
				// look at single target
				var target = Targets.First() as Unit;
				if (target != null)
				{
					caster.Target = target;
				}
			}

			return err;
		}
		#endregion

		#region Perform
		bool PrePerformAI()
		{
			// in case of instant spells, we just collected the targets, and they could not have gone anywhere
			if (!IsInstant)
			{
				RevalidateAllTargets();

				return Targets.Count > 0;
			}
			return true;
		}

		void RevalidateAllTargets()
		{
			// clear original target set
			Targets.Clear();

			// find all SpellTargetCollections to revalidate
			var uniqueTargets = new HashSet<SpellTargetCollection>();
			foreach (var handler in Handlers)
			{
				uniqueTargets.Add(handler.Targets);
			}

			// remove from each collection
			foreach (var targets in uniqueTargets)
			{
				// revalidate and then re-add to unique target set
				targets.RevalidateAll();
				Targets.AddRange(targets);
			}
		}

		#endregion

		/// <summary>
		/// Called when finished casting
		/// </summary>
		void OnAICasted()
		{
			if (Spell.AISettings.IdleTimeAfterCastMillis > 0)
			{
				CasterUnit.Idle(Spell.AISettings.IdleTimeAfterCastMillis);
			}
		}
	}
}
