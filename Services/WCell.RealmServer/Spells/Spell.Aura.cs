using System.Linq;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells
{
	/// <summary>
	/// Aura-related information of a Spell
	/// </summary>
	public partial class Spell
	{
		#region Auto generated Aura Fields
		/// <summary>
		/// Whether this Spell is an Aura
		/// </summary>
		public bool IsAura;

		/// <summary>
		/// AuraUID, the same for all Auras that may not stack
		/// </summary>
		public uint AuraUID;

		public bool HasPeriodicAuraEffects;

		public bool HasNonPeriodicAuraEffects;

		/// <summary>
		/// AuraFlags to be applied to all Auras resulting from this Spell
		/// </summary>
		public AuraFlags DefaultAuraFlags;

		/// <summary>
		/// General Amplitude for Spells that represent Auras (can only have one for the time being)
		/// </summary>
		public int AuraAmplitude;

		/// <summary>
		/// whether this Spell is an AreaAura
		/// </summary>
		public bool IsAreaAura;

		/// <summary>
		/// Modal Auras cannot be updated, but must be replaced
		/// </summary>
		public bool IsModalAura;

		/// <summary>
		/// General Amplitude for Spells that represent AreaAuras (can only have one per spell)
		/// </summary>
		public int AreaAuraAmplitude;

		/// <summary>
		/// All effects that belong to an Aura
		/// </summary>
		public SpellEffect[] AuraEffects;

		/// <summary>
		/// All effects that belong to an AreaAura
		/// </summary>
		public SpellEffect[] AreaAuraEffects;

		/// <summary>
		/// Whether the Aura's effects should be applied once for each of its Applications
		/// </summary>
		public bool CanStack;

		/// <summary>
		/// The amount of initial Aura-Applications
		/// </summary>
		public int StackCount;

		public bool IsPureAura;

		public bool IsPureBuff;

		public bool IsPureDebuff;

		public bool IsProc;

		public bool IsVehicle;

		/// <summary>
		/// whether this Spell applies the death effect
		/// </summary>
		public bool IsGhost;

		/// <summary>
		/// whether this spell applies makes the targets fly
		/// </summary>
		public bool HasFlyEffect;

		/// <summary>
		/// Does this Spell apply a Mount-Aura?
		/// </summary>
		public bool IsMount
		{
			get { return Mechanic == SpellMechanic.Mounted; }
		}

		/// <summary>
		/// Does this Spell apply a Flying-Mount Aura?
		/// </summary>
		public bool IsFlyingMount;

		public bool CanApplyMultipleTimes;

		/// <summary>
		/// 
		/// </summary>
		public AuraCasterGroup AuraCasterGroup;
		#endregion


		private void InitAura()
		{
			IsAura = HasEffectWith(effect =>
			{
				if (effect.AuraType != AuraType.None)
				{
					return true;
				}
				return false;
			});

			if (SpellId == SpellId.TameAdultPlainstrider)
			{
				ToString();
			}

			if (IsAura)
			{
				ForeachEffect(effect =>
				{
					if (effect.IsAuraEffect)
					{
						HasNonPeriodicAuraEffects = HasNonPeriodicAuraEffects || !effect.IsPeriodic;
						HasPeriodicAuraEffects = HasPeriodicAuraEffects || effect.IsPeriodic;
					}
				});
			}

			IsModalAura =
                AttributesExB.HasFlag(SpellAttributesExB.AutoRepeat);

			if (IsAura)
			{
				HasManaShield = HasEffectWith(effect => effect.AuraType == AuraType.ManaShield);
			}

			var auraEffects = GetEffectsWith(effect => effect.AuraEffectHandlerCreator != null);
			if (auraEffects != null)
			{
				AuraEffects = auraEffects.ToArray();
			}

			var areaAuraEffects = GetEffectsWith(effect => effect.IsAreaAuraEffect);

			if (areaAuraEffects != null)
			{
				AreaAuraEffects = areaAuraEffects.ToArray();
			}

			IsAreaAura = AreaAuraEffects != null;

			IsPureAura = !IsDamageSpell && !HasEffectWith(effect => effect.EffectType != SpellEffectType.ApplyAura ||
				effect.EffectType != SpellEffectType.ApplyAuraToMaster || effect.EffectType != SpellEffectType.ApplyStatAura ||
				effect.EffectType != SpellEffectType.ApplyStatAuraPercent);

			IsPureBuff = IsPureAura && HasBeneficialEffects && !HasHarmfulEffects;

			IsPureDebuff = IsPureAura && HasHarmfulEffects && !HasBeneficialEffects;

			IsVehicle = HasEffectWith(effect => effect.AuraType == AuraType.Vehicle);

			CanStack = MaxStackCount > 0;
			// procs and stacking:
			if (ProcCharges > 0)
			{
				// applications will be used up by procs
				StackCount = ProcCharges;
			}
			else
			{
				// applications can be added by re-applying
				StackCount = 1;
			}

			IsGhost = HasEffectWith(effect => effect.AuraType == AuraType.Ghost);


			HasFlyEffect = HasEffectWith(effect => effect.AuraType == AuraType.Fly);

			IsFlyingMount = IsMount &&
							HasEffectWith(effect => effect.AuraType == AuraType.ModSpeedMountedFlight);

			CanApplyMultipleTimes = Attributes == (SpellAttributes.NoVisibleAura | SpellAttributes.Passive) &&
				Skill == null && Talent == null;

			// procs
			if (ProcTriggerFlags != ProcTriggerFlags.None || CasterProcSpells != null)
			{
				ProcTriggerEffects = Effects.Where(effect => effect.IsProc).ToArray();
				if (ProcTriggerEffects.Length == 0)
				{
					// no proc-specific effects -> all effects are triggered on proc
					ProcTriggerEffects = null;
				}
				if (ProcTriggerFlags == (ProcTriggerFlags.MeleeAttackSelf | ProcTriggerFlags.SpellCast))
				{
					// we don't want any SpellCast to trigger on that
					ProcTriggerFlags = ProcTriggerFlags.MeleeAttackSelf;
				}
				IsProc = ProcTriggerEffects != null;
			}

			if (AuraUID == 0)
			{
				CreateAuraUID();
			}
		}

		#region AuraUID Evaluation
		private void CreateAuraUID()
		{
			var count = AuraHandler.AuraIdEvaluators.Count;
			//for (var i = count-1; i >= 0; i--)
			for (var i = 0u; i < count; i++)
			{
				var eval = AuraHandler.AuraIdEvaluators[(int)i];
				if (eval(this))
				{
					AuraUID = (uint)SpellLineId.End + i;
					break;
				}
			}

			if (AuraUID == 0)
			{
				// by default the uid is the id of all spells in one line
				// and single spells get single unique ids
				if (Line != null)
				{
					AuraUID = Line.AuraUID;
				}
				else
				{
					AuraUID = AuraHandler.GetNextAuraUID();
				}
			}
		}
		#endregion

		public bool CanOverride(Spell spell)
		{
			if (CanOverrideEqualAuraRank)
			{
				return Rank >= spell.Rank;
			}
			else
			{
				return Rank > spell.Rank;
			}
		}

		public AuraIndexId GetAuraUID(CasterInfo caster, WorldObject target)
		{
			return GetAuraUID(IsBeneficialFor(caster, target));
		}

		public AuraIndexId GetAuraUID(bool positive)
		{
			return new AuraIndexId
			{
				AuraUID = !CanApplyMultipleTimes ? AuraUID : AuraHandler.lastAuraUid + ++AuraHandler.randomAuraId,
				IsPositive = positive
			};
		}
	}
}
