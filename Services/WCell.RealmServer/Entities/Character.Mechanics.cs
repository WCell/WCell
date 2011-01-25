using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Trade;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Constants.Achievements;

namespace WCell.RealmServer.Entities
{
	public partial class Character
	{
		protected int[] m_dmgBonusVsCreatureTypePct;
		internal int[] m_MeleeAPModByStat;
		internal int[] m_RangedAPModByStat;

		#region Creature Type Damage

		/// <summary>
		/// Damage bonus vs creature type in %
		/// </summary>
		public void ModDmgBonusVsCreatureTypePct(CreatureType type, int delta)
		{
			if (m_dmgBonusVsCreatureTypePct == null)
			{
				m_dmgBonusVsCreatureTypePct = new int[(int)CreatureType.End];
			}
			var val = m_dmgBonusVsCreatureTypePct[(int)type] + delta;
			m_dmgBonusVsCreatureTypePct[(int)type] = val;
		}

		/// <summary>
		/// Damage bonus vs creature type in %
		/// </summary>
		public void ModDmgBonusVsCreatureTypePct(uint[] creatureTypes, int delta)
		{
			foreach (var type in creatureTypes)
			{
				ModDmgBonusVsCreatureTypePct((CreatureType)type, delta);
			}
		}
		#endregion

		#region AP Bonus By Stat
		public int GetMeleeAPModByStat(StatType stat)
		{
			if (m_MeleeAPModByStat == null)
			{
				return 0;
			}
			return m_MeleeAPModByStat[(int)stat];
		}

		public void SetMeleeAPModByStat(StatType stat, int value)
		{
			if (m_MeleeAPModByStat == null)
			{
				m_MeleeAPModByStat = new int[(int)StatType.End];
			}
			m_baseStats[(int)stat] = value;
			this.UpdateMeleeAttackPower();
		}

		public void ModMeleeAPModByStat(StatType stat, int delta)
		{
			SetMeleeAPModByStat(stat, (GetMeleeAPModByStat(stat) + delta));
		}

		public int GetRangedAPModByStat(StatType stat)
		{
			if (m_RangedAPModByStat == null)
			{
				return 0;
			}
			return m_RangedAPModByStat[(int)stat];
		}

		public void SetRangedAPModByStat(StatType stat, int value)
		{
			if (m_RangedAPModByStat == null)
			{
				m_RangedAPModByStat = new int[(int)StatType.End];
			}
			m_baseStats[(int)stat] = value;
			this.UpdateRangedAttackPower();
		}

		public void ModRangedAPModByStat(StatType stat, int delta)
		{
			SetRangedAPModByStat(stat, (GetRangedAPModByStat(stat) + delta));
		}
		#endregion

		#region Movement Handling
		/// <summary>
		/// Is called whenever the Character moves up or down in water or while flying.
		/// </summary>
		internal protected void MovePitch(float moveAngle)
		{
		}

		/// <summary>
		/// Is called whenever the Character falls
		/// </summary>
		internal protected void OnFalling()
		{
			if (m_fallStart == 0)
			{
				m_fallStart = Environment.TickCount;
				m_fallStartHeight = m_position.Z;
			}


			if (IsFlying || !IsAlive || GodMode)
			{
				return;
			}
			// TODO Immunity against environmental damage

		}

		public bool IsSwimming
		{
			get { return MovementFlags.HasFlag(MovementFlags.Swimming); }
		}

		public bool IsUnderwater
		{
			get { return m_position.Z < m_swimSurfaceHeight - 0.5f; }
		}

		internal protected void OnSwim()
		{
			// TODO: Lookup liquid type and verify heights
			if (!IsSwimming)
			{
				m_swimStart = DateTime.Now;
			}
			else
			{

			}
		}

		internal protected void OnStopSwimming()
		{
			m_swimSurfaceHeight = -2048;
		}

		/// <summary>
		/// Is called whenever the Character is moved while on Taxi, Ship, elevator etc
		/// </summary>
		internal protected void MoveTransport(ref Vector4 transportLocation)
		{
			SendSystemMessage("You have been identified as cheater: Faking transport movement!");
		}

		/// <summary>
		/// Is called whenever a Character moves
		/// </summary>
		public override void OnMove()
		{
			base.OnMove();

			if (m_standState != StandState.Stand)
			{
				StandState = StandState.Stand;
			}

			if (m_currentRitual != null)
			{
				m_currentRitual.Remove(this);
			}

			if (IsTrading && !IsInRadius(m_tradeWindow.OtherWindow.Owner, TradeMgr.MaxTradeRadius))
			{
				m_tradeWindow.Cancel(TradeStatus.TooFarAway);
			}

			var now = Environment.TickCount;
			if (m_fallStart > 0 && now - m_fallStart > 3000 && m_position.Z == LastPosition.Z)
			{
				if (IsAlive && Flying == 0 && Hovering == 0 && FeatherFalling == 0 && !IsImmune(DamageSchool.Physical))
				{
					var fallDamage = FallDamageGenerator.GetFallDmg(this, m_fallStartHeight - m_position.Z);
					
					if (fallDamage > 0)
					{
						// If the character current health is higher then the fall damage, the player survived the fall.
						if (fallDamage < Health)
						{
							Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.FallWithoutDying, (uint)(m_fallStartHeight - m_position.Z * 100));
						}
					//	DoEnvironmentalDamage(EnviromentalDamageType.Fall, fallDamage);
					}

					m_fallStart = 0;
					m_fallStartHeight = 0;
				}
			}

			// TODO: Change speedhack detection
			// TODO: Check whether the character is really in Taxi
			if (SpeedHackCheck)
			{
				var msg = "You have been identified as a SpeedHacker. - Byebye!";

				// simple SpeedHack protection
				int latency = Client.Latency;
				int delay = now - m_lastMoveTime + Math.Max(1000, latency);

				float speed = Flying > 0 ? FlightSpeed : RunSpeed;
				float maxDistance = (speed / 1000f) * delay * SpeedHackToleranceFactor;
				if (!IsInRadius(ref LastPosition, maxDistance))
				{
					// most certainly a speed hacker
					log.Warn("WARNING: Possible speedhacker [{0}] moved {1} yards in {2} milliseconds (Latency: {3}, Tollerance: {4})",
							 this, GetDistance(ref LastPosition), delay, latency, SpeedHackToleranceFactor);
				}

				Kick(msg);
			}

			LastPosition = MoveControl.Mover.Position;
		}

		public void SetMover(WorldObject mover, bool canControl)
		{
			MoveControl.Mover = mover;
			MoveControl.CanControl = canControl;

			if (mover == null)
			{
				CharacterHandler.SendControlUpdate(this, this, canControl);
			}
			else
			{
				CharacterHandler.SendControlUpdate(this, mover, canControl);
			}
		}

		public void ResetMover()
		{
			MoveControl.Mover = this;
			MoveControl.CanControl = true;
		}

		/// <summary>
		/// Is called whenever a new object appears within vision range of this Character
		/// </summary>
		public void OnEncountered(WorldObject obj)
		{
			if (obj != this)
			{
				obj.AreaCharCount++;
			}
			KnownObjects.Add(obj);
			SendUnknownState(obj);
		}

		/// <summary>
		/// Sends yet unknown information about a new object,
		/// such as Aura packets
		/// </summary>
		/// <param name="obj"></param>
		private void SendUnknownState(WorldObject obj)
		{
			if (obj is Unit)
			{
				var unit = (Unit)obj;

				if (unit.Auras.VisibleAuraCount > 0)
				{
					AuraHandler.SendAllAuras(this, unit);
				}
			}
		}

		/// <summary>
		/// Is called whenever an object leaves this Character's sight
		/// </summary>
		public void OnOutOfRange(WorldObject obj)
		{
			obj.AreaCharCount--;
			if (obj is Character && m_observers != null)
			{
				if (m_observers.Remove((Character)obj))
				{
					// Character was observing: Now destroy items for him
					for (var i = (InventorySlot)0; i < InventorySlot.Bag1; i++)
					{
						var item = m_inventory[i];
						if (item != null)
						{
							item.SendDestroyToPlayer((Character)obj);
						}
					}
				}
			}

			if (obj == DuelOpponent && !Duel.IsActive)
			{
				// opponent vanished before Duel started: Cancel duel
				Duel.Dispose();
			}

			if (obj == m_target)
			{
				// unset current Target
				ClearTarget();
			}

			if (obj == m_activePet)
			{
				ActivePet = null;
			}

			if (GossipConversation != null && obj == GossipConversation.Speaker && GossipConversation.Character == this)
			{
				// stop conversation with a vanished object
				GossipConversation.Dispose();
			}

			if (!(obj is Transport))
			{
				KnownObjects.Remove(obj);

				// send the destroy packet
				obj.SendDestroyToPlayer(this);
			}
		}

		/// <summary>
		/// Is called whenever this Character was added to a new map
		/// </summary>
		internal protected override void OnEnterMap()
		{
			base.OnEnterMap();

			// when removed from map, make sure the Character forgets everything and gets everything re-sent
			ClearSelfKnowledge();

			m_lastMoveTime = Environment.TickCount;
			LastPosition = m_position;

			AddPostUpdateMessage(() =>
			{
				// Add Honorless Target buff
				if (m_zone != null && m_zone.Template.IsPvP)
				{
					SpellCast.TriggerSelf(SpellId.HonorlessTarget);
				}
			});

			if (IsPetActive)
			{
				// actually spawn pet
				IsPetActive = true;
			}
		}

		protected internal override void OnLeavingMap()
		{
			if (m_activePet != null && m_activePet.IsInWorld)
			{
				m_activePet.Map.RemoveObject(m_activePet);
			}

			if (m_minions != null)
			{
				foreach (var minion in m_minions)
				{
					minion.Delete();
				}
			}

			base.OnLeavingMap();
		}

		private StandState m_standState;

		/// <summary>
		/// Changes the character's stand state and notifies the client.
		/// </summary>
		public override StandState StandState
		{
			get { return m_standState; }
			set
			{
				if (value != StandState)
				{
					m_standState = value;
					base.StandState = value;

					if (m_looterEntry != null &&
						m_looterEntry.Loot != null &&
						value != StandState.Kneeling &&
						m_looterEntry.Loot.MustKneelWhileLooting)
					{
						CancelLooting();
					}

					if (value == StandState.Stand)
					{
						m_auras.RemoveByFlag(AuraInterruptFlags.OnStandUp);
					}

					if (IsInWorld)
					{
						CharacterHandler.SendStandStateUpdate(this, value);
					}
				}
			}
		}
		#endregion

		#region Overrides
		protected override void OnResistanceChanged(DamageSchool school)
		{
			base.OnResistanceChanged(school);
			if (m_activePet != null && m_activePet.IsHunterPet)
			{
				m_activePet.UpdatePetResistance(school);
			}
		}

		public override void ModSpellHitChance(DamageSchool school, int delta)
		{
			base.ModSpellHitChance(school, delta);

			// also modify pet's hit chance
			if (m_activePet != null)
			{
				m_activePet.ModSpellHitChance(school, delta);
			}
		}

		public override float GetResiliencePct()
		{
			var resilience = GetCombatRating(CombatRating.MeleeResilience);
			return resilience / GameTables.GetCRTable(CombatRating.MeleeResilience).GetMax((uint)Level - 1);
		}

		public override void DoEnvironmentalDamage(EnviromentalDamageType dmgType, int amount)
		{
			base.DoEnvironmentalDamage(dmgType, amount);
			if(!IsAlive)
			{
				Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.DeathsFrom, (uint)dmgType, 1);
			}
		}
		#endregion

		public BaseRelation GetRelationTo(Character chr, CharacterRelationType type)
		{
			return RelationMgr.Instance.GetRelation(EntityId.Low, chr.EntityId.Low, type);
		}

		/// <summary>
		/// Returns whether this Character ignores the Character with the given low EntityId.
		/// </summary>
		/// <returns></returns>
		public bool IsIgnoring(IUser user)
		{
			return RelationMgr.Instance.HasRelation(EntityId.Low, user.EntityId.Low, CharacterRelationType.Ignored);
		}

		/// <summary>
		/// Indicates whether the two Characters are in the same <see cref="Group"/>
		/// </summary>
		/// <param name="chr"></param>
		/// <returns></returns>
		public bool IsAlliedWith(Character chr)
		{
			return m_groupMember != null && chr.m_groupMember != null && m_groupMember.Group == chr.m_groupMember.Group;
		}

		/// <summary>
		/// Binds Character to start position if none other is set
		/// </summary>
		void CheckBindLocation()
		{
			if (!m_bindLocation.IsValid())
			{
				BindTo(this, m_archetype.StartLocation);
			}
		}

		public void TeleportToBindLocation()
		{
			TeleportTo(BindLocation);
		}

		public bool CanFly
		{
			get
			{
				return (m_Map.CanFly && (m_zone == null || m_zone.Flags.HasFlag(ZoneFlags.CanFly))) || Role.IsStaff;
			}
		}

		#region Mounts

		public override void Mount(uint displayId)
		{
			if (m_activePet != null)
			{
				// remove active pet
				m_activePet.RemoveFromMap();
			}

			base.Mount(displayId);
		}

		protected internal override void DoDismount()
		{
			if (IsPetActive)
			{
				// put pet into world
				PlaceOnTop(ActivePet);
			}
			base.DoDismount();
		}
		#endregion
	}
}
