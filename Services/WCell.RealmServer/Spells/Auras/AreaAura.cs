using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Spells;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.Spells.Auras
{
	/// <summary>
	/// An AreaAura either (if its using an AreaAura spell) applies AreaAura-Effects to everyone in a radius around its center or
	/// triggers its spell on everyone around it.
	/// Can be persistant (Blizzard, Hurricane etc.) or mobile (Paladin Aura)
	/// </summary>
	public class AreaAura : IUpdatable, IAura
	{
		/// <summary>
		/// Delay in milliseconds to wait before revalidating effected targets (Default: 1000 ms).
		/// This is mostly used for Paladin Auras because they don't have an amplitude on their own.
		/// </summary>
		[Variable("DefaultAreaAuraAmplitude")]
		public static int DefaultAmplitude = 1000;

		WorldObject m_holder;
		Spell m_spell;
		Dictionary<Unit, Aura> m_targets;
		float m_radius;
		ITickTimer m_controller;
		TimerEntry m_timer;
		ObjectReference m_CasterReference;
		float m_duration;
		float m_elapsed;
		ISpellParameters m_params;
		private int m_remainingCharges;

		///// <summary>
		///// The two filters for the two target types of a SpellEffect (assuming all area aura effects share the same target type)
		///// </summary>
		//private TargetFilter m_targetFilter, m_targetFilter2;

		/// <summary>
		/// Creates a new AreaAura that will auto-trigger the given Spell on everyone, according
		/// to the given SpellParameters.
		/// </summary>
		/// <param name="distributedSpell"></param>
		public AreaAura(WorldObject holder, Spell distributedSpell, ISpellParameters prms)
		{
			Init(holder, distributedSpell);
			m_params = prms;
			m_remainingCharges = m_params.MaxCharges;
			m_radius = prms.Radius;

			Start(null, true);
		}

		public AreaAura(WorldObject holder, Spell spell)
		{
			Init(holder, spell);
			m_radius = spell.Effects[0].GetRadius(holder.SharedReference);
		}

		/// <summary>
		/// Creates a new AreaAura which applies its effects to everyone in its radius of influence
		/// </summary>
		protected void Init(WorldObject holder, Spell spell)
		{
			m_holder = holder;
			if (holder is DynamicObject)
			{
				m_CasterReference = holder.Master.SharedReference;
			}
			else
			{
				m_CasterReference = holder.SharedReference;
			}

			m_spell = spell;
			if (spell.IsAreaAura)
			{
				m_targets = new Dictionary<Unit, Aura>();
			}

			holder.AddAreaAura(this);
		}

		#region Properties
		/// <summary>
		/// The Holder of this AreaAura.
		/// </summary>
		public WorldObject Holder
		{
			get { return m_holder; }
			internal set { m_holder = value; }
		}

		public Spell Spell
		{
			get { return m_spell; }
		}

		/// <summary>
		/// The Position of the holder is also the Center of the Aura.
		/// </summary>
		public Vector3 Center
		{
			get { return m_holder.Position; }
		}

		/// <summary>
		/// Radius of the Aura
		/// </summary>
		public float Radius
		{
			get { return m_radius; }
			set { m_radius = value; }
		}

		/// <summary>
		/// Milliseconds until this expires
		/// </summary>
		public int TimeLeft
		{
			get
			{
				if (m_controller == null)
				{
					return (int)((m_duration - m_elapsed) * 1000);
				}

				return m_controller.TimeLeft;
			}
		}

		/// <summary>
		/// Aura is active if its still applied to a <c>Holder</c>
		/// </summary>
		public bool IsAdded
		{
			get { return m_holder != null; }
		}

		/// <summary>
		/// Called by a SpellChannel when channeling
		/// </summary>
		public void Apply()
		{
			RevalidateTargetsAndApply(0);
		}

		public void TryRemove(bool cancelled)
		{

		}

		/// <summary>
		/// Remove and dispose AreaAura.
		/// </summary>
		public void Remove(bool cancelled)
		{
			m_holder.CancelAreaAura(this);
			m_holder = null;
			m_remainingCharges = 0;		// make sure Remove will not be called again

			if (m_timer != null)
			{
				m_timer.Dispose();
			}

			if (m_targets != null)
			{
				RemoveEffects(m_targets);
				m_targets.Clear();
			}
		}
		#endregion

		/// <summary>
		/// Initializes this AreaAura with the given controller. 
		/// If no controller is given, the AreaAura controls timing and disposal itself.
		/// </summary>
		/// <param name="controller">A controller controls timing and disposal of this AreaAura</param>
		/// <param name="noTimeout">whether the Aura should not expire (ignore the Spell's duration).</param>
		public void Start(ITickTimer controller, bool noTimeout)
		{
			if (m_radius == 0)
			{
				m_radius = 5;
			}
			if (m_timer == null)
			{
				m_controller = controller;

				if (m_controller == null)
				{
					if (m_params != null)
					{
						m_timer = new TimerEntry(m_params.StartDelay,
							m_params.Amplitude != 0 ? m_params.Amplitude : DefaultAmplitude, RevalidateTargetsAndApply);
					}
					else
					{
						m_timer = new TimerEntry(DefaultAmplitude, DefaultAmplitude, RevalidateTargetsAndApply);
					}
					m_timer.Start();
				}

				if (noTimeout)
				{
					m_duration = int.MaxValue;
				}
				else
				{
					m_duration = m_spell.GetDuration(m_CasterReference) / 1000f;
					if (m_duration < 1)
					{
						m_duration = int.MaxValue;
					}
				}
			}
		}

		/// <summary>
		/// Check for all targets in radius, kick out invalid ones and add new ones
		/// </summary>
		protected internal void RevalidateTargetsAndApply(float timeElapsed)
		{
			if (m_controller == null)
			{
				m_elapsed += m_timer.Interval;
				if (m_elapsed >= m_duration)
				{
					Remove(false);
					return;
				}
			}

			//if (m_spell.AuraInterruptFlags.Has(AuraInterruptFlags.OnLeaveArea))
			//{
			//    RemoveInvalidTargets();
			//}
			RemoveInvalidTargets();

			var auraEffects = m_spell.AreaAuraEffects != null;

			// find new targets
			var newTargets = new List<WorldObject>();
			var exclMobs = m_holder.Faction.Id == 0;	// neutral aura holders, for events etc

			m_holder.IterateEnvironment(m_radius,
				obj =>
				{
					if (obj != m_holder &&
						((exclMobs && obj.IsOwnedByPlayer) || (!exclMobs && obj is Unit)) &&
						(m_spell.HasHarmfulEffects == m_holder.MayAttack(obj)) &&
						m_spell.CheckValidTarget(m_holder, obj) == SpellFailedReason.Ok)
					{
						if (!auraEffects || !m_targets.ContainsKey((Unit)obj))
						{
							newTargets.Add(obj);
						}
					}
					return true;
				}
			);

			for (var i = 0; i < newTargets.Count; i++)
			{
				var target = (Unit)newTargets[i];

				if (!IsAdded)
				{
					// got cancelled
					return;
				}

				if (auraEffects)
				{
					// apply aura effects
					ApplyAuraEffects(target);
				}
				else
				{
					// trigger the spell
					m_holder.SpellCast.Trigger(m_spell, target);
				}

				if (m_holder.IsTrap)
				{
					OnTrapTriggered(target);
				}

				if (m_remainingCharges != 0)
				{
					m_remainingCharges--;
					if (m_remainingCharges == 0)
					{
						Remove(false);
					}
				}
			}
		}

		/// <summary>
		/// Called when the holder is a trap and the given triggerer triggered it.
		/// </summary>
		/// <param name="triggerer"></param>
		private void OnTrapTriggered(Unit triggerer)
		{
			// trap trigger proc
			var owner = ((GameObject)m_holder).Owner;
			if (owner != null)
			{
				triggerer.Proc(ProcTriggerFlags.TrapTriggered, triggerer,
					new TrapTriggerAction { Attacker = owner, Spell = m_spell, Victim = triggerer },
					false);
			}
		}

		private void RemoveInvalidTargets()
		{
			if (m_targets != null)
			{
				var toRemove = m_targets.Where(target => !target.Key.IsInRadius(m_holder, m_radius)).ToArray();

				foreach (var target in toRemove)
				{
					if (target.Value.IsAdded)
					{
						var auras = target.Key.Auras;
						if (auras != null)
						{
							target.Value.Remove(false);
						}
					}
					m_targets.Remove(target.Key);
				}
			}
		}

		/// <summary>
		/// Applies this AreaAura's effects to the given target
		/// </summary>
		protected void ApplyAuraEffects(Unit target)
		{
			var beneficial = m_spell.IsBeneficialFor(m_CasterReference, target);

			// checks
			var missReason = SpellCast.CheckDebuffResist(target, m_spell, m_CasterReference.Level, !beneficial);
			if (missReason != CastMissReason.None)
			{
				// TODO: Flash message ontop of the head
				return;
			}

			// try to stack/apply aura
			var aura = target.Auras.CreateAura(m_CasterReference, m_spell, false);
			if (aura != null)
			{
				m_targets.Add(target, aura);
			}
		}

		/// <summary>
		/// Removes all auras from the given targets
		/// </summary>
		protected static void RemoveEffects(IEnumerable<KeyValuePair<Unit, Aura>> targets)
		{
			foreach (var pair in targets)
			{
				pair.Value.Remove(false);
			}
		}

		#region IUpdatable

		public void Update(float dt)
		{
			if (m_timer != null)
			{
				m_timer.Update(dt);
			}
		}

		#endregion
	}
}