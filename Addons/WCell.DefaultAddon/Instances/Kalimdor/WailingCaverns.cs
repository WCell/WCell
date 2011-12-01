using System;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.AI;
using WCell.RealmServer.AI.Actions.Combat;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Targeting;

///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 6/11/2009
///

namespace WCell.Addons.Default.Instances
{
	public class WailingCaverns : BaseInstance
	{
		#region General (Content)

		private static NPCEntry _deviateStinglashEntry;
		private static NPCEntry _deviateCreeperEntry;
		private static NPCEntry _deviateSlayerEntry;
		private static NPCEntry _mutanustheDevourerEntry;
		private static NPCEntry _madMagglishEntry;
		private static NPCEntry _lordCobrahnEntry;
		private static NPCEntry _lordPythasEntry;
		private static NPCEntry _ladyAnacondraEntry;
		private static NPCEntry _boahnEntry;
		private static NPCEntry _lordSerpentisEntry;
		private static NPCEntry _skumEntry;
		private static NPCEntry _druidoftheFangEntry;
		private static NPCEntry _deviateAdderEntry;
		private static NPCEntry _deviateCrocoliskEntry;
		private static NPCEntry _deviateLasherEntry;
		private static NPCEntry _deviateDreadfangEntry;
		private static NPCEntry _deviateViperEntry;
		private static NPCEntry _deviateVenomwingEntry;
		private static NPCEntry _deviateShamblerEntry;
		private static NPCEntry _verdanTheEverlivingEntry;

		private static readonly Random Random = new Random();

		[Initialization]
		[DependentInitialization(typeof (NPCMgr))]
		public static void InitNPCs()
		{
			_deviateStinglashEntry = NPCMgr.GetEntry(NPCId.DeviateStinglash);
			_deviateStinglashEntry.AddSpell(SpellId.Lash);
			var lash = SpellHandler.Get(SpellId.Lash);
			lash.AISettings.SetCooldownRange(17000, 20000);

			_deviateCreeperEntry = NPCMgr.GetEntry(NPCId.DeviateCreeper);
			_deviateCreeperEntry.AddSpell(SpellId.InfectedWound);
			SpellHandler.Apply(spell => SpellCooldowns.SetCooldownTime(spell, Random.Next(12000, 18000)), SpellId.InfectedWound);

			_deviateSlayerEntry = NPCMgr.GetEntry(NPCId.DeviateSlayer);
			_deviateSlayerEntry.Activated +=
				deviateSlayer => { ((BaseBrain) deviateSlayer.Brain).DefaultCombatAction.Strategy = new DeviateSlayerAttackAction(deviateSlayer); };

			_mutanustheDevourerEntry = NPCMgr.GetEntry(NPCId.MutanusTheDevourer);
			var mutanustheDevourerSpells = new[] {SpellId.NaralexsNightmare, SpellId.Terrify, SpellId.ThundercrackRank1};
			_mutanustheDevourerEntry.AddSpells(mutanustheDevourerSpells);
			SpellHandler.Apply(spell => SpellCooldowns.SetCooldownTime(spell, 30000), mutanustheDevourerSpells[0]);
			SpellHandler.Apply(spell => SpellCooldowns.SetCooldownTime(spell, 50000), mutanustheDevourerSpells[1]);
			SpellHandler.Apply(spell =>
			                   	{
			                   		spell.SpellTargetRestrictions.TargetFlags = SpellTargetFlags.Self;
			                   		SpellCooldowns.SetCooldownTime(spell, 10000);
			                   	}, mutanustheDevourerSpells[2]);

			_madMagglishEntry = NPCMgr.GetEntry(NPCId.MadMagglish);
			_madMagglishEntry.AddSpell(SpellId.SmokeBomb);
			SpellHandler.Apply(spell =>
			                   	{
			                   		spell.SpellTargetRestrictions.TargetFlags = SpellTargetFlags.Self;
			                   		SpellCooldowns.SetCooldownTime(spell, 9000);
			                   	}, SpellId.SmokeBomb);

			_lordCobrahnEntry = NPCMgr.GetEntry(NPCId.LordCobrahn);
			_lordCobrahnEntry.BrainCreator = lordCobrahn => new LordCobrahnBrain(lordCobrahn);
			_lordCobrahnEntry.Activated +=
				lordCobrahn => { ((BaseBrain) lordCobrahn.Brain).DefaultCombatAction.Strategy = new LordCobrahnAttackAction(lordCobrahn); };

			_lordPythasEntry = NPCMgr.GetEntry(NPCId.LordPythas);
			var lordPythasSpells = new[] {SpellId.ThunderclapRank1, SpellId.SleepRank1};
			_lordPythasEntry.AddSpells(lordPythasSpells);
			SpellHandler.Apply(spell =>
			                   	{
			                   		spell.SpellTargetRestrictions.TargetFlags = SpellTargetFlags.Self;
			                   		SpellCooldowns.SetCooldownTime(spell, Random.Next(6000, 11000));
			                   	}, lordPythasSpells[0]);
			SpellHandler.Apply(spell => SpellCooldowns.SetCooldownTime(spell, Random.Next(12000, 20000)), lordPythasSpells[1]);
			_lordPythasEntry.BrainCreator = lordPythas => new LordPythasBrain(lordPythas);
			_lordPythasEntry.Activated +=
				lordPythas => { ((BaseBrain) lordPythas.Brain).DefaultCombatAction.Strategy = new LordPythasAttackAction(lordPythas); };

			_ladyAnacondraEntry = NPCMgr.GetEntry(NPCId.LadyAnacondra);
			_ladyAnacondraEntry.AddSpell(SpellId.SleepRank1);
			SpellHandler.Apply(spell => SpellCooldowns.SetCooldownTime(spell, Random.Next(12000, 25000)), SpellId.SleepRank1);
			_ladyAnacondraEntry.BrainCreator = ladyAnacondra => new LadyAnacondraBrain(ladyAnacondra);
			_ladyAnacondraEntry.Activated +=
				ladyAnacondra => { ((BaseBrain) ladyAnacondra.Brain).DefaultCombatAction.Strategy = new LadyAnacondraAttackAction(ladyAnacondra); };

			_boahnEntry = NPCMgr.GetEntry(NPCId.Boahn);
			_boahnEntry.BrainCreator = boahn => new BoahnBrain(boahn);
			_boahnEntry.Activated +=
				boahn => { ((BaseBrain) boahn.Brain).DefaultCombatAction.Strategy = new BoahnAttackAction(boahn); };

			_lordSerpentisEntry = NPCMgr.GetEntry(NPCId.LordSerpentis);
			_lordSerpentisEntry.AddSpell(SpellId.SleepRank1);
			SpellHandler.Apply(spell => SpellCooldowns.SetCooldownTime(spell, Random.Next(10000, 19000)), SpellId.SleepRank1);
			_lordSerpentisEntry.BrainCreator = lordSerpentis => new LordSerpentisBrain(lordSerpentis);
			_lordSerpentisEntry.Activated +=
				lordSerpentis => { ((BaseBrain) lordSerpentis.Brain).DefaultCombatAction.Strategy = new LordSerpentisAttackAction(lordSerpentis); };

			_skumEntry = NPCMgr.GetEntry(NPCId.Skum);
			_skumEntry.AddSpell(SpellId.ChainedBolt);
			SpellHandler.Apply(spell => SpellCooldowns.SetCooldownTime(spell, Random.Next(4000, 6000)), SpellId.ChainedBolt);

			_druidoftheFangEntry = NPCMgr.GetEntry(NPCId.DruidOfTheFang);
			_druidoftheFangEntry.BrainCreator = druidoftheFang => new DruidoftheFangBrain(druidoftheFang);
			_druidoftheFangEntry.Activated +=
				druidoftheFang => { ((BaseBrain) druidoftheFang.Brain).DefaultCombatAction.Strategy = new DruidoftheFangAttackAction(druidoftheFang); };

			_deviateAdderEntry = NPCMgr.GetEntry(NPCId.DeviateAdder);
			_deviateAdderEntry.AddSpell(SpellId.EffectPoison);
			SpellHandler.Apply(spell => SpellCooldowns.SetCooldownTime(spell, Random.Next(15000, 25000)), SpellId.EffectPoison);

			_deviateCrocoliskEntry = NPCMgr.GetEntry(NPCId.DeviateCrocolisk);
			_deviateCrocoliskEntry.AddSpell(SpellId.TendonRip);
			SpellHandler.Apply(spell => SpellCooldowns.SetCooldownTime(spell, Random.Next(10000, 12000)), SpellId.TendonRip);

			_deviateLasherEntry = NPCMgr.GetEntry(NPCId.DeviateLasher);
			_deviateLasherEntry.AddSpell(SpellId.WideSlashRank1);
			SpellHandler.Apply(spell => SpellCooldowns.SetCooldownTime(spell, Random.Next(8000, 12000)), SpellId.WideSlashRank1);

			_deviateDreadfangEntry = NPCMgr.GetEntry(NPCId.DeviateDreadfang);
			_deviateDreadfangEntry.AddSpell(SpellId.Terrify);
			SpellHandler.Apply(spell =>SpellCooldowns.SetCooldownTime(spell, Random.Next(20000, 25000)), SpellId.Terrify);

			_deviateViperEntry = NPCMgr.GetEntry(NPCId.DeviateViper);
			_deviateViperEntry.AddSpell(SpellId.LocalizedToxin);
			SpellHandler.Apply(spell => SpellCooldowns.SetCooldownTime(spell, Random.Next(10000, 15000)), SpellId.LocalizedToxin);

			_deviateVenomwingEntry = NPCMgr.GetEntry(NPCId.DeviateVenomwing);
			_deviateVenomwingEntry.AddSpell(SpellId.ToxicSpit);
			SpellHandler.Apply(spell => SpellCooldowns.SetCooldownTime(spell, Random.Next(8000, 10000)), SpellId.ToxicSpit);

			_deviateShamblerEntry = NPCMgr.GetEntry(NPCId.DeviateShambler);
			_deviateShamblerEntry.Activated +=
				deviateShambler => { ((BaseBrain) deviateShambler.Brain).DefaultCombatAction.Strategy = new DeviateShamblerAttackAction(deviateShambler); };

			_verdanTheEverlivingEntry = NPCMgr.GetEntry(NPCId.VerdanTheEverliving);
			_verdanTheEverlivingEntry.AddSpell(SpellId.GraspingVines);
			SpellHandler.Apply(spell =>
			                   	{
			                   		spell.SpellTargetRestrictions.TargetFlags = SpellTargetFlags.Self;
			                   		SpellCooldowns.SetCooldownTime(spell, Random.Next(10000, 13000));
			                   	}, SpellId.GraspingVines);
		}

		#endregion

		#region Deviate Slayer

		public class DeviateSlayerAttackAction : AIAttackAction
		{
			private const int Interval = 1;
			internal static Spell FatalBite;
			private int _fatalBiteTick;
			private DateTime _timeSinceLastInterval;

			public DeviateSlayerAttackAction(NPC deviateSlayer)
				: base(deviateSlayer)
			{
			}

			[Initialization(InitializationPass.Second)]
			public static void InitDeviateSlayer()
			{
				FatalBite = SpellHandler.Get(SpellId.FatalBite);
				FatalBite.AISettings.IdleTimeAfterCastMillis = 1000;
			}

			public override void Start()
			{
				_timeSinceLastInterval = DateTime.Now;
				base.Start();
			}

			public override void Update()
			{
				DateTime timeNow = DateTime.Now;
				TimeSpan timeBetween = timeNow - _timeSinceLastInterval;

				if (timeBetween.TotalSeconds >= Interval)
				{
					_timeSinceLastInterval = timeNow;
					if (m_owner.HealthPct <= 40 && _fatalBiteTick >= 10)
					{
						_fatalBiteTick = 0;
						m_owner.SpellCast.Start(FatalBite, false);
					}
				}

				base.Update();
			}
		}

		#endregion

		#region Lord Cobrahn

		#region Nested type: LordCobrahnAttackAction

		public class LordCobrahnAttackAction : AIAttackAction
		{
			internal static Spell CobrahnSerpentForm, DruidsSlumber, LightningBolt, Poison;
			private int _phase;

			public LordCobrahnAttackAction(NPC lordCobrahn)
				: base(lordCobrahn)
			{
			}

			[Initialization(InitializationPass.Second)]
			public static void InitLordCobhahn()
			{
				CobrahnSerpentForm = SpellHandler.Get(SpellId.CobrahnSerpentForm);

				DruidsSlumber = SpellHandler.Get(SpellId.DruidsSlumber);
				DruidsSlumber.AISettings.SetCooldownRange(13000, 20000);
				DruidsSlumber.AISettings.IdleTimeAfterCastMillis = 1000;

				LightningBolt = SpellHandler.Get(SpellId.LightningBolt);
				LightningBolt.AISettings.SetCooldownRange(2000, 4000);
				LightningBolt.AISettings.IdleTimeAfterCastMillis = 1000;

				Poison = SpellHandler.Get(SpellId.EffectPoison);
				Poison.AISettings.SetCooldownRange(12000, 20000);
				Poison.AISettings.IdleTimeAfterCastMillis = 1000;
			}
			
			public override void Update()
			{
				if (m_owner.HealthPct <= 30 && _phase != 3)
				{
					if (m_owner.Auras[CobrahnSerpentForm] == null)
					{
						m_owner.SpellCast.TriggerSelf(CobrahnSerpentForm);
					}
					m_owner.Spells.Clear();
					m_owner.Spells.AddSpell(DruidsSlumber);
					_phase = 3;
				}
				else if (m_owner.PowerPct <= 15 && _phase != 2)
				{
					_phase = 2;
					m_owner.Spells.Clear();
					m_owner.Spells.AddSpell(Poison);
				}
				else if (m_owner.PowerPct >= 30 && _phase != 1)
				{
					_phase = 1;
					m_owner.Spells.Clear();
					m_owner.Spells.AddSpell(LightningBolt);
					m_owner.Spells.AddSpell(Poison);
				}

				if (m_owner.IsEvading)
				{
					_phase = 0;
				}

				base.Update();
			}
		}

		#endregion

		#region Nested type: LordCobrahnBrain

		public class LordCobrahnBrain : MobBrain
		{
			public LordCobrahnBrain(NPC lordCobrahn)
				: base(lordCobrahn)
			{
			}

			public override bool ScanAndAttack()
			{
				return base.ScanAndAttack();
			}

			public override void OnEnterCombat()
			{
				NPCAiText[] texts = NPCAiTextMgr.GetEntries("You will never wake the dreamer!");
				NPCAiText currentNpcText = texts[0];
				m_owner.PlaySound((uint) currentNpcText.Sound);
				var npc = m_owner as NPC;
				if (npc != null) npc.Yell(currentNpcText.Texts);

				m_owner.SpellCast.Start(SpellHandler.Get(SpellId.LightningBolt), false, m_owner.GetNearbyRandomHostileCharacter());

				base.OnEnterCombat();
			}
		}

		#endregion

		#endregion

		#region Lord Pythas

		#region Nested type: LordPythasAttackAction

		public class LordPythasAttackAction : AIAttackAction
		{
			private const int Interval = 1;
			internal static Spell HealingTouch, LightningBolt;

			private int _healingTouchTick;
			private int _lightningBoltTick;
			private int _phase = 1;
			private DateTime _timeSinceLastInterval;

			public LordPythasAttackAction(NPC lordPythas)
				: base(lordPythas)
			{
			}

			[Initialization(InitializationPass.Second)]
			public static void InitLordPythas()
			{
				HealingTouch = SpellHandler.Get(SpellId.ClassSkillHealingTouch);
				HealingTouch.AISettings.SetCooldownRange(20000, 25000);
				HealingTouch.AISettings.IdleTimeAfterCastMillis = 1000;
				HealingTouch.OverrideAITargetDefinitions(
					DefaultTargetAdders.AddSelf, 									// Adder
					DefaultTargetFilters.IsWoundedEnough);							// Filters

				LightningBolt = SpellHandler.Get(SpellId.LightningBolt);
				LightningBolt.AISettings.SetCooldownRange(2000, 4000);
				LightningBolt.AISettings.IdleTimeAfterCastMillis = 1000;
			}

			public override void Start()
			{
				_timeSinceLastInterval = DateTime.Now;
				base.Start();
			}

			public override void Update()
			{
				DateTime timeNow = DateTime.Now;
				TimeSpan timeBetween = timeNow - _timeSinceLastInterval;

				if (m_owner.IsEvading)
				{
					_phase = 0;
				}

				if (timeBetween.TotalSeconds >= Interval)
				{
					_timeSinceLastInterval = timeNow;
					if (CheckSpellCast())
					{
						// idle a little after casting a spell
						m_owner.Idle(1000);
						return;
					}
				}

				base.Update();
			}

			private bool CheckSpellCast()
			{
				_healingTouchTick++;
				_lightningBoltTick++;

				if (_phase == 1)
				{
					if (m_owner.PowerPct <= 15)
					{
						_phase = 2;
					}

					if (_lightningBoltTick >= Random.Next(2, 4))
					{
						Character chr = m_owner.GetNearbyRandomHostileCharacter();
						if (chr != null)
						{
							_lightningBoltTick = 0;
							m_owner.SpellCast.Start(LightningBolt, false, chr);
							return true;
						}
					}
				}

				if (_phase == 2)
				{
					if (m_owner.PowerPct >= 30)
					{
						_phase = 1;
					}
				}

				if (m_owner.HealthPct <= 50 && _healingTouchTick >= Random.Next(20, 25))
				{
					if (m_owner != null)
					{
						_healingTouchTick = 0;
						m_owner.SpellCast.Start(HealingTouch, false, m_owner);
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region Nested type: LordPythasBrain

		public class LordPythasBrain : MobBrain
		{
			public LordPythasBrain(NPC lordPythas)
				: base(lordPythas)
			{
			}

			public override void OnEnterCombat()
			{
				NPCAiText[] texts = NPCAiTextMgr.GetEntries("The coils of death... Will crush you.");
				NPCAiText currentNpcText = texts[0];
				m_owner.PlaySound((uint) currentNpcText.Sound);
				var npc = m_owner as NPC;
				if (npc != null) npc.Yell(currentNpcText.Texts);

				m_owner.SpellCast.Start(SpellHandler.Get(SpellId.LightningBolt), false, m_owner.GetNearbyRandomHostileCharacter());

				base.OnEnterCombat();
			}
		}

		#endregion

		#endregion

		#region Lady Anacondra

		#region Nested type: LadyAnacondraAttackAction

		public class LadyAnacondraAttackAction : AIAttackAction
		{
			private const int Interval = 1;
			internal static Spell HealingTouch, LightningBolt;

			private int _lightiningBoltTick;
			private bool _isCastHealingTouch;
			private int _phase = 1;
			private DateTime _timeSinceLastInterval;

			public LadyAnacondraAttackAction(NPC ladyAnacondra)
				: base(ladyAnacondra)
			{
			}

			[Initialization(InitializationPass.Second)]
			public static void InitLadyAnacondra()
			{
				HealingTouch = SpellHandler.Get(SpellId.ClassSkillHealingTouch);
				LightningBolt = SpellHandler.Get(SpellId.LightningBolt);
			}

			public override void Start()
			{
				_timeSinceLastInterval = DateTime.Now;
				base.Start();
			}

			public override void Update()
			{
				DateTime timeNow = DateTime.Now;
				TimeSpan timeBetween = timeNow - _timeSinceLastInterval;

				if (m_owner.HealthPct <= 15)
				{
					_phase = 3;
				}
				if (m_owner.IsEvading)
				{
					_phase = 0;
				}

				if (timeBetween.TotalSeconds >= Interval)
				{
					_timeSinceLastInterval = timeNow;
					if (CheckSpellCast())
					{
						// idle a little after casting a spell
						m_owner.Idle(1000);
						return;
					}
				}

				base.Update();
			}

			private bool CheckSpellCast()
			{
				_lightiningBoltTick++;

				if (_phase == 1)
				{
					if (m_owner.PowerPct <= 15)
					{
						_phase = 2;
					}

					if (_lightiningBoltTick >= Random.Next(2, 4))
					{
						Character chr = m_owner.GetNearbyRandomHostileCharacter();
						if (chr != null)
						{
							_lightiningBoltTick = 0;
							m_owner.SpellCast.Start(LightningBolt, false, chr);
							return true;
						}
					}
				}

				if (_phase == 2)
				{
					if (m_owner.PowerPct >= 30)
					{
						_phase = 1;
					}
				}

				if (_phase == 3)
				{
				}

				if (!_isCastHealingTouch && m_owner.HealthPct <= 50)
				{
					if (m_owner != null)
					{
						_isCastHealingTouch = true;
						m_owner.SpellCast.Trigger(HealingTouch);
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region Nested type: LadyAnacondraBrain

		public class LadyAnacondraBrain : MobBrain
		{
			public LadyAnacondraBrain(NPC ladyAnacondra)
				: base(ladyAnacondra)
			{
			}

			public override void OnEnterCombat()
			{
				NPCAiText[] texts = NPCAiTextMgr.GetEntries("None can stand against the serpent lords!");
				NPCAiText currentNpcText = texts[0];
				m_owner.PlaySound((uint) currentNpcText.Sound);
				var npc = m_owner as NPC;
				if (npc != null) npc.Yell(currentNpcText.Texts);

				m_owner.SpellCast.Start(SpellHandler.Get(SpellId.ThornsAura_2), false, m_owner.GetNearbyRandomHostileCharacter());
				m_owner.SpellCast.Start(SpellHandler.Get(SpellId.LightningBolt), false, m_owner.GetNearbyRandomHostileCharacter());

				base.OnEnterCombat();
			}
		}

		#endregion

		#endregion

		#region Boahn

		#region Nested type: BoahnAttackAction

		public class BoahnAttackAction : AIAttackAction
		{
			private const int Interval = 1;
			internal static Spell LightningBolt, HealingTouch;

			private int _healingTouchTick;
			private int _lightningBoltTick;
			private int _phase = 1;
			private DateTime _timeSinceLastInterval;

			public BoahnAttackAction(NPC boahn)
				: base(boahn)
			{
			}

			[Initialization(InitializationPass.Second)]
			public static void InitBoahn()
			{
				LightningBolt = SpellHandler.Get(SpellId.LightningBolt);
				HealingTouch = SpellHandler.Get(SpellId.ClassSkillHealingTouch);
			}

			public override void Start()
			{
				_timeSinceLastInterval = DateTime.Now;
				base.Start();
			}

			public override void Update()
			{
				DateTime timeNow = DateTime.Now;
				TimeSpan timeBetween = timeNow - _timeSinceLastInterval;

				if (m_owner.HealthPct <= 15)
				{
					_phase = 3;
				}
				if (m_owner.IsEvading)
				{
					_phase = 0;
				}

				if (timeBetween.TotalSeconds >= Interval)
				{
					_timeSinceLastInterval = timeNow;
					if (CheckSpellCast())
					{
						// idle a little after casting a spell
						m_owner.Idle(1000);
						return;
					}
				}

				base.Update();
			}

			private bool CheckSpellCast()
			{
				_lightningBoltTick++;
				_healingTouchTick++;

				if (_phase == 1)
				{
					if (m_owner.PowerPct <= 15)
					{
						_phase = 2;
					}

					if (_lightningBoltTick >= Random.Next(2, 4))
					{
						Character chr = m_owner.GetNearbyRandomHostileCharacter();
						if (chr != null)
						{
							_lightningBoltTick = 0;
							m_owner.SpellCast.Start(LightningBolt, false, chr);
							return true;
						}
					}
				}

				if (_phase == 2)
				{
					if (m_owner.PowerPct >= 30)
					{
						_phase = 1;
					}
				}

				if (m_owner.HealthPct <= 40 && _healingTouchTick >= Random.Next(30, 40))
				{
					if (m_owner != null)
					{
						_healingTouchTick = 0;
						m_owner.SpellCast.Trigger(HealingTouch);
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region Nested type: BoahnBrain

		public class BoahnBrain : MobBrain
		{
			public BoahnBrain(NPC boahn)
				: base(boahn)
			{
			}

			public override void OnEnterCombat()
			{
				m_owner.SpellCast.Start(SpellHandler.Get(SpellId.LightningBolt), false, m_owner.GetNearbyRandomHostileCharacter());

				base.OnEnterCombat();
			}
		}

		#endregion

		#endregion

		#region Lord Serpentis

		#region Nested type: LordSerpentisAttackAction

		public class LordSerpentisAttackAction : AIAttackAction
		{
			private const int Interval = 1;
			internal static Spell HealingTouch, LightningBolt;

			private int _lightningBoltTick;
			private bool _isCastHealingTouch;
			private int _phase = 1;
			private DateTime _timeSinceLastInterval;

			public LordSerpentisAttackAction(NPC lordSerpentis)
				: base(lordSerpentis)
			{
			}

			[Initialization(InitializationPass.Second)]
			public static void InitLordSerpentis()
			{
				HealingTouch = SpellHandler.Get(SpellId.ClassSkillHealingTouch);
				LightningBolt = SpellHandler.Get(SpellId.LightningBolt);
			}

			public override void Start()
			{
				_timeSinceLastInterval = DateTime.Now;
				base.Start();
			}

			public override void Update()
			{
				DateTime timeNow = DateTime.Now;
				TimeSpan timeBetween = timeNow - _timeSinceLastInterval;

				if (m_owner.IsEvading)
				{
					_phase = 0;
				}

				if (timeBetween.TotalSeconds >= Interval)
				{
					_timeSinceLastInterval = timeNow;
					if (CheckSpellCast())
					{
						// idle a little after casting a spell
						m_owner.Idle(1000);
						return;
					}
				}

				base.Update();
			}

			private bool CheckSpellCast()
			{
				_lightningBoltTick++;

				if (_phase == 1)
				{
					if (m_owner.PowerPct <= 15)
					{
						_phase = 2;
					}

					if (_lightningBoltTick >= Random.Next(2, 4))
					{
						Character chr = m_owner.GetNearbyRandomHostileCharacter();
						if (chr != null)
						{
							_lightningBoltTick = 0;
							m_owner.SpellCast.Start(LightningBolt, false, chr);
							return true;
						}
					}
				}

				if (_phase == 2)
				{
					if (m_owner.PowerPct >= 30)
					{
						_phase = 1;
					}
				}

				if (!_isCastHealingTouch && m_owner.HealthPct <= 50)
				{
					if (m_owner != null)
					{
						_isCastHealingTouch = true;
						m_owner.SpellCast.Trigger(HealingTouch);
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region Nested type: LordSerpentisBrain

		public class LordSerpentisBrain : MobBrain
		{
			public LordSerpentisBrain(NPC lordSerpentis)
				: base(lordSerpentis)
			{
			}

			public override void OnEnterCombat()
			{
				NPCAiText[] texts = NPCAiTextMgr.GetEntries("I am the serpent king, I can do anything.");
				NPCAiText currentNpcText = texts[0];
				m_owner.PlaySound((uint) currentNpcText.Sound);
				var npc = m_owner as NPC;
				if (npc != null) npc.Yell(currentNpcText.Texts);

				m_owner.SpellCast.Start(SpellHandler.Get(SpellId.LightningBolt), false, m_owner.GetNearbyRandomHostileCharacter());

				base.OnEnterCombat();
			}
		}

		#endregion

		#endregion

		#region Druid of the Fang

		#region Nested type: DruidoftheFangAttackAction

		public class DruidoftheFangAttackAction : AIAttackAction
		{
			internal static Spell LightningBolt, DruidsSlumber, HealingTouch, SerpentForm;
			private int _phase = 0;

			public DruidoftheFangAttackAction(NPC druidoftheFang)
				: base(druidoftheFang)
			{
			}

			[Initialization(InitializationPass.Second)]
			public static void InitDruidoftheFang()
			{
                SerpentForm = SpellHandler.Get(SpellId.SerpentFormShapeshift);

                DruidsSlumber = SpellHandler.Get(SpellId.DruidsSlumber);
                DruidsSlumber.AISettings.SetCooldownRange(10000, 20000);
                DruidsSlumber.AISettings.IdleTimeAfterCastMillis = 1000;

                LightningBolt = SpellHandler.Get(SpellId.LightningBolt);
                LightningBolt.AISettings.SetCooldownRange(2000, 4000);
                LightningBolt.AISettings.IdleTimeAfterCastMillis = 1000;

                HealingTouch = SpellHandler.Get(SpellId.ClassSkillHealingTouch);
                HealingTouch.AISettings.SetCooldownRange(12000, 18000);
                HealingTouch.AISettings.IdleTimeAfterCastMillis = 1000;
			}

			public override void Update()
			{
                if (_phase == 0)
                {
                    _phase = 1;
                    m_owner.Spells.AddSpell(LightningBolt);
                    m_owner.Spells.AddSpell(DruidsSlumber);
                    m_owner.Spells.AddSpell(HealingTouch);
                }
                
                if (m_owner.PowerPct <= 15 && _phase == 1)
                {
                    _phase = 2;
#if PEPSI_DEBUG
                    m_owner.Say("Moving to Phase {0}", _phase);
#endif
                }
                else if(m_owner.PowerPct >= 30 && _phase == 2)
                {
                    _phase = 1;
#if PEPSI_DEBUG
                    m_owner.Say("Moving to Phase {0}", _phase);
#endif
                }
                
                if (m_owner.HealthPct <= 50)
                {
                    if (m_owner.Auras[SerpentForm] == null)
                    {
                        m_owner.SpellCast.TriggerSelf(SerpentForm);
                    }
                }

                if(_phase == 2 && m_owner.Brain.State != BrainState.Evade)
                {
                    m_owner.Brain.State = BrainState.Evade;
                }
                
                //if (m_owner.IsEvading)
                //{
                //    _phase = 4;
#if PEPSI_DEBUG
                    //m_owner.Say("Moving to Phase {0}", _phase);
#endif
                //    m_owner.Spells.Clear();
                //}

				base.Update();
			}
		}

		#endregion

		#region Nested type: DruidoftheFangBrain

		public class DruidoftheFangBrain : MobBrain
		{
			public DruidoftheFangBrain(NPC druidoftheFang)
				: base(druidoftheFang)
			{
			}

			public override void OnEnterCombat()
			{
				m_owner.SpellCast.Start(SpellHandler.Get(SpellId.LightningBolt));

				base.OnEnterCombat();
			}
		}

		#endregion

		#endregion

		#region Deviate Shambler

		public class DeviateShamblerAttackAction : AIAttackAction
		{
			private const int Interval = 1;
			internal static Spell WildRegeneration;
			private int _wildRegenerationTick;
			private DateTime _timeSinceLastInterval;

			public DeviateShamblerAttackAction(NPC deviateShambler)
				: base(deviateShambler)
			{
			}

			[Initialization(InitializationPass.Second)]
			public static void InitDeviateShambler()
			{
				WildRegeneration = SpellHandler.Get(SpellId.WildRegeneration);
			}

			public override void Start()
			{
				_timeSinceLastInterval = DateTime.Now;
				base.Start();
			}

			public override void Update()
			{
				DateTime timeNow = DateTime.Now;
				TimeSpan timeBetween = timeNow - _timeSinceLastInterval;

				if (timeBetween.TotalSeconds >= Interval)
				{
					_timeSinceLastInterval = timeNow;
					if (CheckSpellCast())
					{
						// idle a little after casting a spell
						m_owner.Idle(1000);
						return;
					}
				}

				base.Update();
			}

			private bool CheckSpellCast()
			{
				_wildRegenerationTick++;

				if (m_owner.HealthPct <= 70 && _wildRegenerationTick >= 21)
				{
					if (m_owner != null)
					{
						_wildRegenerationTick = 0;
						m_owner.SpellCast.Start(WildRegeneration, false, m_owner);
						return true;
					}
				}
				return false;
			}
		}

		#endregion
	}
}