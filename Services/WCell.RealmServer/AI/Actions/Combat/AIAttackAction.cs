using System;
using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.AI.Actions.Combat
{
	/// <summary>
	/// Attack with the main weapon
	/// </summary>
	public class AIAttackAction : AITargetMoveAction
	{
		/// <summary>
		/// Every x Region-Ticks shuffle Spells
		/// </summary>
		public static int SpellShuffleTicks = 50;

		/// <summary>
		/// Every x Region-Ticks try to cast a random active spell
		/// </summary>
		public static int SpellCastTicks = 1;

		protected float maxDist, desiredDist;

		public AIAttackAction(NPC owner)
			: base(owner)
		{
		}

		public bool UsesSpells
		{
			get { return m_owner.HasSpells; }
		}

		public bool HasSpellReady
		{
			get { return ((NPC)m_owner).NPCSpells.ReadySpells.Count > 0; }
		}

		public override float DistanceMin
		{
			get { return m_owner.BoundingRadius; }
		}

		public override float DistanceMax
		{
			get { return maxDist; }
		}

		public override float DesiredDistance
		{
			get { return desiredDist; }
		}

		/// <summary>
		/// Called when starting to attack a new Target
		/// </summary>
		public override void Start()
		{
			m_owner.IsFighting = true;
			if (UsesSpells)
			{
				((NPC)m_owner).NPCSpells.ShuffleReadySpells();
			}
			m_target = m_owner.Target;
			if (m_target != null)
			{
				maxDist = m_owner.GetBaseAttackRange(m_target) - 1;
				if (maxDist < 0.5f)
				{
					maxDist = 0.5f;
				}
				desiredDist = maxDist / 2;
			}
			if (m_owner.CanMelee)
			{
				base.Start();
			}
		}

		/// <summary>
		/// Called during every Brain tick
		/// </summary>
		public override void Update()
		{
			if (UsesSpells && HasSpellReady && m_owner.CanCastSpells)
			{
				if (!m_owner.CanMelee || m_owner.CheckTicks(SpellCastTicks))
				{
					if (TryCastSpell())
					{
						m_owner.Movement.Stop();
						return;
					}
				}
			}
			if (m_owner.CanMelee)
			{
				base.Update();
			}
		}

		/// <summary>
		/// Called when we stop attacking a Target
		/// </summary>
		public override void Stop()
		{
			m_owner.IsFighting = false;
			base.Stop();
		}

		/// <summary>
		/// Tries to cast a Spell that is ready and allowed in the current context.
		/// </summary>
		/// <returns></returns>
		protected bool TryCastSpell()
		{
			var owner = (NPC)m_owner;

			if (owner.CheckTicks(SpellShuffleTicks))
			{
				owner.NPCSpells.ShuffleReadySpells();
			}

			for (var i = 0; i < owner.NPCSpells.ReadySpells.Count; i++)
			{
				var spell = owner.NPCSpells.ReadySpells[i];

				if (spell.CanCast(owner))
				{
					if (!ShouldCast(spell))
					{
						continue;
					}

					Cast(spell);
				}
			}
			return false;
		}

		private bool ShouldCast(Spell spell)
		{
			if (spell.IsAura)
			{
				if (spell.CasterIsTarget)
				{
					if (m_owner.Auras[new AuraIndexId(spell.AuraUID, true)] != null)
					{
						// caster already has Aura
						return false;
					}
				}
				else
				{
					if (m_target.Auras[spell] != null)
					{
						// target already has Aura
						return true;
					}
				}
			}
			return true;
		}

		private bool Cast(Spell spell)
		{
			if (spell.HasHarmfulEffects)
			{
				return CastHarmfulSpell(spell);
			}
			else
			{
				return CastBeneficialSpell(spell);
			}
		}

		/// <summary>
		/// Casts the given harmful Spell
		/// </summary>
		/// <param name="spell"></param>
		protected bool CastHarmfulSpell(Spell spell)
		{
			if (m_owner.IsInSpellRange(spell, m_target))
			{
				m_owner.SpellCast.TargetLoc = m_target.Position;
				return m_owner.SpellCast.Start(spell, false) == SpellFailedReason.Ok;
			}
			return false;
		}

		/// <summary>
		/// Casts the given beneficial spell on a friendly Target
		/// </summary>
		/// <param name="spell"></param>
		protected bool CastBeneficialSpell(Spell spell)
		{
			// TODO: Cast beneficial spell
			return false;
		}

		public override UpdatePriority Priority
		{
			get { return UpdatePriority.Active; }
		}
	}
}