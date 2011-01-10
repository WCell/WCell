using WCell.Constants.Misc;
using WCell.Constants.Updates;
using WCell.Core.Initialization;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Entities;
using WCell.RealmServer.AreaTriggers;
using WCell.Constants;
using WCell.RealmServer.AI.Actions.States;
using WCell.RealmServer.AI.Actions;
using WCell.Constants.AreaTriggers;
using WCell.Constants.Spells;
using WCell.RealmServer.AI;
using WCell.Util.Graphics;

///
/// Date of creation: 9/1/2009
///

namespace WCell.Addons.Default.Instances
{
	public class UtgardePinnacle : DungeonInstance
	{
		NPC m_svala;
		NPC m_arthasMirror;
		bool m_EncounterStarted;

		public NPC Svala
		{
			get { return m_svala; }
		}

		public bool EncounterStarted
		{
			get { return m_EncounterStarted; }
		}

		/// <summary>
		/// Prepare Svala and Artha's Mirror
		/// </summary>
		void PrepareEncounter(NPC svala)
		{
			m_svala = svala;

			m_arthasMirror = arthasMirrorEntry.Create(DifficultyIndex);
			m_arthasMirror.Orientation = 1.58825f;
			AddObject(m_arthasMirror, arthasPosition);

			// big bosses are idle
			m_arthasMirror.Brain.State = BrainState.Idle;
			svala.Brain.State = BrainState.Idle;
		}

		/// <summary>
		/// Starts Svala's big show
		/// </summary>
		private void StartEncounter()
		{
			if (!m_svala.IsInWorld || !m_arthasMirror.IsInWorld)
			{
				// make sure, both bosses are still in town (not removed by a GM etc)
				return;
			}

			m_EncounterStarted = true;

			// freeze everyone
			IsAIFrozen = true;

			// let Svala look at Arthas
			m_svala.IsInCombat = false;
			m_svala.Target = m_arthasMirror;
			m_svala.CallDelayed(8000, cb =>
			{
				m_svala.Yell("My liege! I have done as you asked, and now beseech you for your blessing!");
				m_svala.PlaySound(13856);

				m_arthasMirror.CallDelayed(10000, o1 =>
				{
					m_arthasMirror.Yell("Your sacrifice is a testament to your obedience. Indeed you are worthy of this charge. Arise, and forever be known as Svala Sorrowgrave!");
					m_arthasMirror.PlaySound(14732);
					m_arthasMirror.Emote(EmoteType.SimpleTalk);

					m_arthasMirror.CallDelayed(7000, o2 =>
					{
						// we called on Artha's Mirror, so we have to make sure that Svala is still in World
						if (!m_svala.IsInWorld)
						{
							return;
						}

						m_arthasMirror.SpellCast.TriggerSingle(SpellId.ArthasTransformingSvala, m_svala);

						m_arthasMirror.CallDelayed(7000, o3 =>
						{
							//m_owner.SpellCast.Cancel();
							//svala.Auras.AddSelf(SpellId.ArthasTransformingSvala, true); //wrong id

							m_svala.CallDelayed(3000, o4 =>
							{
								m_svala.Yell("The sensation is... beyond my imagining. I am yours to command, my king");
								m_svala.PlaySound(13857);
								m_svala.Invulnerable--;

								// reactivate everyone
								IsAIFrozen = false;
							});
						});
					});
				});
			});
		}

		/// <summary>
		/// Let everyone look at Arthas while he is speaking
		/// </summary>
		private void FaceArthas()
		{
			CallOnAllNPCs(npc =>
							{
								if (npc != m_arthasMirror)
								{
									npc.Face(arthasPosition);
								}
							});
		}

		#region Init & Events
		private static NPCEntry svalaEntry;
		private static NPCEntry arthasMirrorEntry;
		static readonly Vector3 arthasPosition = new Vector3(296.2659f, -364.3349f, 92.92485f);

		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void InitNPCs()
		{
			// both big bosses are invul at first
			arthasMirrorEntry = NPCMgr.GetEntry(29280u);
			arthasMirrorEntry.MinLevel = arthasMirrorEntry.MaxLevel = 83;
			arthasMirrorEntry.UnitFlags = UnitFlags.SelectableNotAttackable;

			svalaEntry = NPCMgr.GetEntry(29281u);
			svalaEntry.UnitFlags = UnitFlags.SelectableNotAttackable;
			svalaEntry.Activated += svala =>
			{
				var instance = svala.Region as UtgardePinnacle;
				if (instance != null)
				{
					instance.PrepareEncounter(svala);
				}
			};

			// TODO: Set the emotestate of the sitting audience correctly and make them idle until the convo is over
			var observanceTrigger = AreaTriggerMgr.GetTrigger(AreaTriggerId.UtgardePinnacleObservanceHall);
            if(observanceTrigger != null)
            {
			    observanceTrigger.Triggered += OnObservanceHallTriggered;                
            }


		}

		static void OnObservanceHallTriggered(AreaTrigger at, Character chr)
		{
			var instance = chr.Region as UtgardePinnacle;

			if (instance != null && !instance.m_EncounterStarted)
			{
				instance.StartEncounter();
			}
		}

		#endregion
	}
}