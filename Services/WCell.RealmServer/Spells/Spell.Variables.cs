using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Talents;
using WCell.Util.Data;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;
using World = WCell.RealmServer.Global.World;
using WCell.Constants.World;

namespace WCell.RealmServer.Spells
{
	public partial class Spell
	{
		#region Spell Variables (that may be modified by spell customizations)
		/// <summary>
		/// Wheter this spell can be cast on players (automatically false for all taunts)
		/// </summary>
		public bool CanCastOnPlayer = true;

		/// <summary>
		/// Whether this is a Spell that is only used to prevent other Spells (cannot be cancelled etc)
		/// </summary>
		public bool IsPreventionDebuff;

		/// <summary>
		/// Whether this is an Aura that can override other instances of itself if they have the same rank (true by default).
		/// Else the spell cast will fail when trying to do so.
		/// </summary>
		public bool CanOverrideEqualAuraRank = true;

		/// <summary>
		/// Spells casted whenever this Spell is casted
		/// </summary>
		public Spell[] TargetTriggerSpells, CasterTriggerSpells;

		/// <summary>
		/// Set of Spells which, when used by the caster of this spell, can proc this Spell.
		/// </summary>
		public HashSet<Spell> CasterProcSpells;

		/// <summary>
		/// Set of Spells which, when used by the caster of this spell, can proc this Spell on their targets.
		/// </summary>
		public HashSet<Spell> TargetProcSpells;

		/// <summary>
		/// Custom proc handlers to be added to targets of this Aura (this spell must be an Aura).
		/// If this is != null, the resulting Aura of this Spell will not be added as a Proc handler itself.
		/// </summary>
		public List<ProcHandlerTemplate> ProcHandlers;

		/// <summary>
		/// Used for teleport spells amongst others
		/// </summary>
		public Vector3 SpellTargetLocation;

		/// <summary>
		/// Wheter this Aura can proc
		/// </summary>
		public bool IsProc;

		/// <summary>
		/// Whether this spell is supposed to proc something.
		/// If set to true, this Spell will generate a SpellCast proc event when casted.
		/// Don't use for damage spells, else they will generate 2 events!
		/// </summary>
		public bool GeneratesProcEventOnCast;

		/// <summary>
		/// Amount of millis before this Spell may proc another time (if it is a proc)
		/// </summary>
		public int ProcDelay;

		/// <summary>
		/// Whether this Spell's spell damage is increased by AP
		/// </summary>
		public bool DamageIncreasedByAP;

		/// <summary>
		/// The effect whose value represents the max amount of targets to be selected.
		/// This is a way to boost the max target amount with a simple EffectValue modifier.
		/// (Of course one could just have added a new modifier for this, but well.)
		/// </summary>
		public SpellEffect MaxTargetEffect;

		/// <summary>
		/// Optional set of SpellEffects to be applied, only if certain Auras are applied
		/// </summary>
		public SpellEffect[] AuraConditionalEffects;
		#endregion

		#region AI Spell Casting
		public AISpellSettings AISettings;
		#endregion

		#region Auto generated Spell Fields (will be overridden on initialization)
		/// <summary>
		/// Whether this is a Combat ability that will be triggered on next weapon strike (like Heroic Strike etc)
		/// </summary>
		public bool IsOnNextStrike;

		/// <summary>
		/// whether this is an ability involving any kind of weapon-attack
		/// </summary>
		public bool IsPhysicalAbility;

		/// <summary>
		/// Whether this can trigger an instant Strike
		/// </summary>
		public bool IsStrikeSpell;

		/// <summary>
		/// whether this is actually a passive buff
		/// </summary>
		public bool IsPassive;

		/// <summary>
		/// Whether this is a ranged attack (includes wands)
		/// </summary>
		public bool IsRanged;

		/// <summary>
		/// Whether this is a ranged attack (includes wands), that is not triggered
		/// </summary>
		public bool IsRangedAbility;

		/// <summary>
		/// whether this is a throw (used for any kind of throwing weapon)
		/// </summary>
		public bool IsThrow;

		/// <summary>
		/// whether this is an actual SpellCaster spell
		/// </summary>
		public bool IsProfession;

		/// <summary>
		/// whether this teaches the initial Profession
		/// </summary>
		public bool TeachesApprenticeAbility;

		/// <summary>
		/// whether this is teaching another spell
		/// </summary>
		public bool IsTeachSpell;

		/// <summary>
		/// Whether it has any individual or category cooldown
		/// </summary>
		public bool HasCooldown;

		/// <summary>
		/// Whether this spell has an individual cooldown (unlike a category or "global" cooldown)
		/// </summary>
		public bool HasIndividualCooldown;

		/// <summary>
		/// Tame Beast (Id: 1515) amongst others
		/// </summary>
		public bool IsTame
		{
			get { return AttributesExB.HasFlag(SpellAttributesExB.TamePet); }
		}

		/// <summary>
		/// Tame Beast (Id: 13481) amongst others
		/// </summary>
		public bool IsTameEffect;

		/// <summary>
		/// Whether this spell enchants an Item
		/// </summary>
		public bool IsEnchantment;

		/// <summary>
		/// Fishing spawns a FishingNode which needs to be removed upon canceling
		/// </summary>
		public bool IsFishing;

		/// <summary>
		/// The spell which teaches this spell (if any)
		/// </summary>
		public Spell LearnSpell;

		/// <summary>
		/// whether Spell's effects don't wear off when dead
		/// </summary>
		public bool PersistsThroughDeath
		{
			get { return AttributesExC.HasFlag(SpellAttributesExC.PersistsThroughDeath); }
		}

		/// <summary>
		/// whether this spell is triggered by another one
		/// </summary>
		public bool IsTriggeredSpell;

		/// <summary>
		/// whether its a food effect
		/// </summary>
		public bool IsFood
		{
			get { return SpellCategories.Category == 11; }
		}

		/// <summary>
		/// whether its a drink effect
		/// </summary>
		public bool IsDrink
		{
            get { return SpellCategories.Category == 59; }
		}

		/// <summary>
		/// Indicates whether this Spell has at least one harmful effect
		/// </summary>
		public bool HasHarmfulEffects;

		/// <summary>
		/// Indicates whether this Spell has at least one beneficial effect
		/// </summary>
		public bool HasBeneficialEffects;


		public HarmType HarmType;

		/// <summary>
		/// The SpellEffect of this Spell that represents a PersistentAreaAura and thus a DO (or null if it has none)
		/// </summary>
		public SpellEffect DOEffect;

		/// <summary>
		/// whether this is a Heal-spell
		/// </summary>
		public bool IsHealSpell;

		/// <summary>
		/// Whether this is a weapon ability that attacks with both weapons
		/// </summary>
		public bool IsDualWieldAbility;

		/// <summary>
		/// whether this is a Skinning-Spell
		/// </summary>
		public bool IsSkinning;

		/// <summary>
		/// If this is set for Spells, they will not be casted in the usual manner but instead this Handler will be called.
		/// </summary>
		public SpecialCastHandler SpecialCast;

		/// <summary>
		/// The Talent which this Spell represents one Rank of (every Talent Rank is represented by one Spell)
		/// </summary>
		public TalentEntry Talent;

		public bool IsTalent
		{
			get { return Talent != null; }
		}

		private SkillAbility m_Ability;

		/// <summary>
		/// The SkillAbility that this Spell represents
		/// </summary>
		public SkillAbility Ability
		{
			get { return m_Ability; }
			internal set
			{
				m_Ability = value;
				if (value != null && SpellClassOptions.ClassId == 0)
				{
					var clss = Ability.ClassMask.GetIds();
					if (clss.Length == 1)
					{
                        SpellClassOptions.ClassId = clss[0];
					}
				}
			}
		}

		/// <summary>
		/// The tier of the skill that this spell represents (if this is a Skill spell)
		/// </summary>
		public SkillTierId SkillTier;

		/// <summary>
		/// Whether this represents a tier of a skill
		/// </summary>
		public bool RepresentsSkillTier
		{
			get { return SkillTier != SkillTierId.End; }
		}

		/// <summary>
		/// Tools that are required by this spell (is set during Initialization of Items)
		/// </summary>
		public ItemTemplate[] RequiredTools;

		public Spell NextRank, PreviousRank;

		/// <summary>
		/// Indicates whether this Spell has any targets at all
		/// </summary>
		public bool HasTargets;

		/// <summary>
		/// Indicates whether this Spell has at least one effect on the caster
		/// </summary>
		public bool CasterIsTarget;

		/// <summary>
		/// Indicates whether this Spell teleports the Uni back to its bound location
		/// </summary>
		public bool IsHearthStoneSpell;

		public bool IsAreaSpell;

		public bool IsDamageSpell;

		public SpellEffect TotemEffect;

		public SpellEffect[] ProcTriggerEffects;

		/// <summary>
		/// The equipment slot where to look for a required item
		/// </summary>
		public EquipmentSlot EquipmentSlot = EquipmentSlot.End;

		public bool GeneratesComboPoints;

		public bool IsFinishingMove;

		public bool RequiresDeadTarget;

		/// <summary>
		/// whether this is a channel-spell
		/// </summary>
		public bool IsChanneled;

		public int ChannelAuraPeriod;

		public bool RequiresCasterOutOfCombat;

		/// <summary>
		/// Whether this spell costs default power (does not include Runes)
		/// </summary>
		public bool CostsPower;

		/// <summary>
		/// Whether this Spell has any Rune costs
		/// </summary>
		public bool CostsRunes;

		/// <summary>
		/// Auras with modifier effects require existing Auras to be re-evaluated
		/// </summary>
		public bool HasModifierEffects;

		/// <summary>
		/// All affecting masks of all Effects
		/// </summary>
		public uint[] AllAffectingMasks = new uint[3];

		public bool HasManaShield;

		public bool IsEnhancer;

		private bool init1, init2;

		public SpellLine Line;

		/// <summary>
		/// Whether this spell has effects that require other Auras to be active to be activated
		/// </summary>
		public bool HasAuraDependentEffects;
		#endregion

		#region Spell Targets
		[Persistent]
		public RequiredSpellTargetType RequiredTargetType = RequiredSpellTargetType.Default;

		public bool MatchesRequiredTargetType(WorldObject obj)
		{
			if (RequiredTargetType == RequiredSpellTargetType.GameObject)
			{
				return obj is GameObject;
			}
			return obj is NPC && ((NPC)obj).IsAlive == (RequiredTargetType == RequiredSpellTargetType.NPCAlive);
		}

		[Persistent]
		public uint RequiredTargetId;

		[Persistent]
		public SpellTargetLocation TargetLocation;

		[Persistent]
		public float TargetOrientation;

		#endregion

		#region Loading
		public void FinalizeDataHolder()
		{
		}
		#endregion
	}

	public enum RequiredSpellTargetType
	{
		Default = -1,
		GameObject = 0,
		NPCAlive,
		NPCDead
	}

	#region SpellTargetLocation
	public class SpellTargetLocation : IWorldLocation
	{
		private Vector3 m_Position;

		public SpellTargetLocation()
		{
			Phase = WorldObject.DefaultPhase;
		}

		public SpellTargetLocation(MapId map, Vector3 pos, uint phase = WorldObject.DefaultPhase)
		{
			Position = pos;
			MapId = map;
			Phase = phase;
		}

		public Vector3 Position
		{
			get { return m_Position; }
			set { m_Position = value; }
		}

		[Persistent]
		public MapId MapId
		{
			get;
			set;
		}

		[Persistent]
		public float X
		{
			get { return m_Position.X; }
			set { m_Position.X = value; }
		}

		[Persistent]
		public float Y
		{
			get { return m_Position.Y; }
			set { m_Position.Y = value; }
		}

		[Persistent]
		public float Z
		{
			get { return m_Position.Z; }
			set { m_Position.Z = value; }
		}

		public Map Map
		{
			get { return World.GetNonInstancedMap(MapId); }
		}

		public uint Phase
		{
			get;
			set;
		}
	}
	#endregion
}