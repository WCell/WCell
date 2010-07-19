using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Misc;
using WCell.RealmServer.Quests;
using WCell.Constants.NPCs;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Spells;
using WCell.RealmServer.AI.Brains;
using WCell.Core.Initialization;
using WCell.RealmServer.Chat;
using WCell.RealmServer.AI;

namespace WCell.Addons.Default.Samples
{
	/// <summary>
	/// Illustrates some basic capabilities of the WCell API:
	/// Ambushers want their sins to be forgiven.
	/// 
	/// Uncomment the initialization-Attributes to activate it
	/// </summary>
	public static class AmbusherQuestSample
	{
		// setup QuestTemplate
		public static QuestTemplate AmbusherQuest;

		public static float MaxForgivenessDistance = 100;

		static void InitQuest()
		{
			AmbusherQuest = new QuestTemplate {
				Id = 12345,
				// TODO: Setup QuestTemplate here
			};
		}

		[Initialization]
		[DependentInitialization(typeof(QuestMgr))]
		[DependentInitialization(typeof(NPCMgr))]
		public static void SetupSampleAmbusherQuest()
		{
			InitQuest();

			// must forgive 5 Ambushers
			AmbusherQuest.AddNPCInteraction(NPCId.WitchwingAmbusher, 5);
			QuestMgr.AddQuest(AmbusherQuest);

			var entry = NPCMgr.GetEntry(NPCId.WitchwingAmbusher);

			entry.BeforeDeath += OnBeforeAmbusherDeath;

			// check if our guy has forgiven the ambusher yet
			ChatMgr.MessageSent += CheckAmbusherRelease;
		}

		private static bool OnBeforeAmbusherDeath(NPC ambusher)
		{
			if (ambusher.FirstAttacker is Character)
			{
				var killer = (Character)ambusher.FirstAttacker;
				if (!killer.QuestLog.HasActiveQuest(AmbusherQuest))
				{
					// quest isn't active
					return true;
				}

				// say something
				ambusher.Say("Oh kind sir, please don't kill me!");

				// discarded the idea of the backstabber:
				//ambusher.MoveBehindThenExecute(killer,
				//    attacker => attacker.SpellCast.Start(SpellId.ClassSkillBackstabRank1, false, killer));

				// cast some AoE spell (ignoring all restrictions)
				ambusher.SpellCast.Trigger(SpellId.ClassSkillFrostNovaRank6);

				// Is now friendly with everyone
				ambusher.FactionId = FactionId.Friendly;

				// more health than ever before (not necessary here)
				ambusher.BaseHealth = ambusher.Health = 100000;

				// won't do anything stupid anymore
				ambusher.Brain.DefaultState = BrainState.Idle;
				ambusher.Brain.EnterDefaultState();

				// never leave the killer alone: Approach again and again...
				ambusher.CallPeriodically(2000,
					obj => SeekForgiveness(ambusher, killer));

				return false; 		// return false to indicate that the guy didn't die
			}
			return true;			// die
		}

		/// <summary>
		/// Move to the killer and pleed
		/// </summary>
		/// <param name="ambusher"></param>
		/// <param name="killer"></param>
		private static void SeekForgiveness(NPC ambusher, Character killer)
		{
			if (killer.IsInWorld &&
						killer.IsAlive &&
						killer.Region == ambusher.Region &&
						killer.IsInRadius(ambusher, MaxForgivenessDistance))
			{
				// make sure we are using the right means of transportation
				if (killer.IsFlying)
				{
					ambusher.Mount(MountId.FlyingBroom);			// probably won't display correctly
					ambusher.Movement.MoveType = AIMoveType.Fly;
				}
				else
				{
					ambusher.Dismount();
					ambusher.Movement.MoveType = AIMoveType.Walk;
				}

				// go to killer
				ambusher.MoveInFrontThenExecute(killer, mover => {
					mover.StandState = StandState.Kneeling;
					mover.Say("I beg thee for forgiveness!");
				});
			}
			else
			{
				// killer is not in reach anymore
				ambusher.Say("Sigh!");
				ambusher.Delete();
			}
		}

		/// <summary>
		/// Check if a Character forgave an Ambusher
		/// </summary>
		static void CheckAmbusherRelease(IChatter chatter, string message,
			ChatLanguage lang, ChatMsgType chatType, IGenericChatTarget target)
		{
			if (chatter is Character)	// make sures its a Character (could also be a broadcast or an IRC bot etc)
			{
				var chr = (Character)chatter;
				var selected = chr.Target as NPC;
				if (selected != null &&
					selected.Entry.NPCId == NPCId.WitchwingAmbusher &&					// Chr selected the ambusher
					(chatType == ChatMsgType.Say || chatType == ChatMsgType.Yell) &&	// Chr speaks out loud
					selected.StandState == StandState.Kneeling &&						// Ambusher is kneeling
					message == "I forgive thee!")										// Chr says the right words
				{
					if (!selected.IsInFrontOf(chr))
					{
						// the char was not talking towards the ambusher
						selected.Say("What? I couldn't hear you!");
					}
					else
					{
						// The Killer has forgiven the Ambusher

						// Standup
						selected.StandState = StandState.Stand;

						// delay (because standing up takes time)
						selected.CallDelayed(800, obj => {
							if (selected.IsInWorld) // ensure that Chr and selected didn't disappear in the meantime
							{
								if (chr.IsInWorld)
								{
									selected.Yell("Thank you so much! - Now I can leave this place.");
								}

								selected.Emote(EmoteType.SimpleApplaud);

								selected.CallDelayed(1000, obj2 => {
									if (selected.IsInWorld)
									{
										// Finally die
										selected.Kill();
									}
								});
							}
						});
					}
				}
			}
		}
	}
}