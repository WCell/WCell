using System.Collections.Generic;
using NLog;
using WCell.Constants;
using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.Constants.Talents;
using WCell.Core.Network;
using WCell.RealmServer.AI.Actions.Combat;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.AI;
using WCell.RealmServer.NPCs.Vehicles;
using WCell.RealmServer.NPCs.Pets;

namespace WCell.RealmServer.Handlers
{
	public static class PetHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		#region IN
		[PacketHandler(RealmServerOpCode.CMSG_PET_NAME_QUERY)]
		public static void HandleNameQuery(IRealmClient client, RealmPacketIn packet)
		{
			var petNumber = packet.ReadUInt32();
			var petEntityId = packet.ReadEntityId();

			var chr = client.ActiveCharacter;
			var pet = chr.Region.GetObject(petEntityId) as NPC;

			if (pet != null)
			{
				SendName(chr, pet.PetNumber, pet.Name, pet.PetNameTimestamp);
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_REQUEST_PET_INFO)]
		public static void HandleInfoRequest(IRealmClient client, RealmPacketIn packet)
		{
			// seems useless?
			// log.Warn("Client {0} sent CMSG_REQUEST_PET_INFO", client);
		}


		[PacketHandler(RealmServerOpCode.CMSG_PET_ACTION)]
		public static void HandleAction(IRealmClient client, RealmPacketIn packet)
		{
			var entityId = packet.ReadEntityId();

			var chr = client.ActiveCharacter;
			var pet = chr.Region.GetObject(entityId) as NPC;

			if (pet != null && pet == chr.ActivePet && pet.IsAlive)
			{
				var record = pet.PetRecord;
				if (record != null)
				{
					var data = (PetActionEntry)packet.ReadUInt32();

					switch (data.Type)
					{
						case PetActionType.SetMode:
							// mode
							pet.SetPetAttackMode(data.AttackMode);
							break;
						case PetActionType.SetAction:
							// action
							var targetAction = data.Action;
							pet.SetPetAction(targetAction);
							break;
						default:
							// spell cast
							var target = chr.Region.GetObject(packet.ReadEntityId());
							pet.CastPetSpell(data.SpellId, target);
							break;
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		/// <param name="packet"></param>
		[PacketHandler(RealmServerOpCode.CMSG_PET_SET_ACTION)]
		public static void HandleSetAction(IRealmClient client, RealmPacketIn packet)
		{
			var entityId = packet.ReadEntityId();

			var chr = client.ActiveCharacter;
			var pet = chr.Region.GetObject(entityId) as NPC;

			if (pet == null || pet.PermanentPetRecord == null || (pet != chr.ActivePet && !chr.GodMode))
			{
				return;
			}

			while (packet.Length - packet.Position >= 8)	// a button has 8 bytes in the packet
			{
				ReadButton(pet, packet);
			}
		}

		private static void ReadButton(NPC pet, RealmPacketIn packet)
		{
			var index = packet.ReadUInt32();
			var newAction = (PetActionEntry)packet.ReadUInt32();

			var record = pet.PermanentPetRecord;

			if (index > record.Actions.Length)
			{
				return;
			}

			// TODO: Interpret spell settings (auto-cast etc)
			record.Actions[index] = newAction;
		}

		[PacketHandler(RealmServerOpCode.CMSG_PET_RENAME)]
		public static void HandleRename(IRealmClient client, RealmPacketIn packet)
		{
			var entityId = packet.ReadEntityId();

			var chr = client.ActiveCharacter;
			var pet = chr.Region.GetObject(entityId) as NPC;

			if (pet != null)
			{
				if (pet == chr.ActivePet || chr.GodMode)
				{
					var newName = packet.ReadCString();
					var reason = pet.TrySetPetName(chr, newName);
					if (reason != PetNameInvalidReason.Ok)
					{
						SendNameInvalid(chr, reason, newName);
					}
				}
			}
#if DEBUG
			else
			{
				chr.SendSystemMessage("You sent CMSG_PET_RENAME for a pet that does not exist in the same Region.");
			}
#endif
		}

		[PacketHandler(RealmServerOpCode.CMSG_PET_ABANDON)]
		public static void HandleAbandon(IRealmClient client, RealmPacketIn packet)
		{
			var petId = packet.ReadEntityId();

			var chr = client.ActiveCharacter;
			var pet = chr.Region.GetObject(petId) as NPC;

			if (pet != null && pet.IsAlive && pet.IsInContext)
			{
				if (pet == chr.ActivePet || chr.GodMode)
				{
					chr.AbandonActivePet();
				}
			}
#if DEBUG
			else
			{
				chr.SendSystemMessage("You sent CMSG_PET_ABANDON for a pet that does not exist in the same Region.");
			}
#endif
		}

		[PacketHandler(RealmServerOpCode.CMSG_PET_SPELL_AUTOCAST)]
		public static void HandleAutocast(IRealmClient client, RealmPacketIn packet)
		{
			var petId = packet.ReadEntityId();

			var chr = client.ActiveCharacter;
			var pet = chr.Region.GetObject(petId) as NPC;

			if (pet != null)
			{
				if (pet == chr.ActivePet || chr.GodMode)
				{
					// TODO: Autocast
				}
			}
#if DEBUG
			else
			{
				chr.SendSystemMessage("You sent CMSG_PET_SPELL_AUTOCAST for a pet that does not exist in the same Region.");
			}
#endif
		}

		[PacketHandler(RealmServerOpCode.CMSG_PET_CANCEL_AURA)]
		public static void HandleCancelAura(IRealmClient client, RealmPacketIn packet)
		{
			var petId = packet.ReadEntityId();
			var spellId = (SpellId)packet.ReadUInt32();

			var chr = client.ActiveCharacter;
			var pet = chr.Region.GetObject(petId) as NPC;

			if (pet != null)
			{
				if (pet.Master == chr)
				{
					var aura = pet.Auras[spellId, true];
					if (aura != null && aura.CanBeCancelled)
					{
						aura.TryRemove(true);
					}
				}
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_PET_STOP_ATTACK)]
		public static void HandleStopAttack(IRealmClient client, RealmPacketIn packet)
		{
			var petId = packet.ReadEntityId();

			var chr = client.ActiveCharacter;
			var pet = chr.Region.GetObject(petId) as NPC;

			if (pet != null && pet.IsAlive)
			{
				if (pet == chr.ActivePet)
				{
					pet.Brain.EnterDefaultState();
				}
			}
#if DEBUG
			else
			{
				chr.SendSystemMessage("You sent CMSG_PET_STOP_ATTACK for a pet that does not exist in the same Region.");
			}
#endif
		}

		[PacketHandler(RealmServerOpCode.CMSG_TOTEM_DESTROYED)]
		public static void HandleDestroyTotem(IRealmClient client, RealmPacketIn packet)
		{
			var totemSlot = packet.ReadUInt32();
			log.Debug("Received CMSG_TOTEM_DESTROYED for Slot {0}", totemSlot);
		}
		#endregion

		public static void SendTameFailure(IPacketReceiver receiver, TameFailReason reason)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_TAME_FAILURE, 1))
			{
				packet.Write((byte)reason);
				receiver.Send(packet);
			}
		}


		#region Spells
		public static void SendPetGUIDs(Character chr)
		{
			if (chr.ActivePet == null)
			{
				using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_GUIDS, 12))
				{
					packet.Write(0); // list count
					chr.Send(packet);
				}
			}
			else
			{
				using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_GUIDS, 12))
				{
					packet.Write(1); // list count
					packet.Write(chr.ActivePet.EntityId);
					chr.Send(packet);
				}
			}
		}

		/// <summary>
		/// Sends any kind of extra command-bar to control other entities, such as NPCs, vehicles etc
		/// </summary>
		/// <param name="owner"></param>
		//public static void SendSpells(Character owner, NPC npc, uint duration,
		//    PetAttackMode attackMode, PetAction action, PetFlags flags,
		//    PetActionEntry[] petActions,
		//    PetSpell[] spells)

		public static void SendSpells(Character owner, NPC pet, PetAction currentAction)
		{
			// TODO: Cooldowns
			var record = pet.PetRecord;
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_SPELLS, 20 + (PetConstants.PetActionCount * 4) + 1 + (pet.Spells.Count) + 1 + (0)))
			{
				packet.Write(pet.EntityId);
				packet.Write((ushort)pet.Entry.FamilyId);
				//packet.Write((ushort)0);
				packet.Write(pet.RemainingDecayDelayMillis);			// duration
				packet.Write((byte)record.AttackMode);
				packet.Write((byte)currentAction);
				packet.Write((ushort)record.Flags);

				var actions = pet.BuidPetActionBar();
				for (var i = 0; i < PetConstants.PetActionCount; i++)
				{
					var action = actions[i];
					packet.Write(action.Raw);
				}

				var spellPos = packet.Position;
				++packet.Position;
				var spellCount = 0;
				foreach (var spell in pet.Spells)
				{
					if (!spell.IsPassive)
					{
						packet.Write(spell.Id | ((uint)PetSpellState.Enabled << 24));
						++spellCount;
					}
				}

				packet.Write((byte)0);		// TODO: Cooldowns

				packet.Position = spellPos;
				packet.Write((byte)spellCount);

				owner.Send(packet);
			}
		}

		public static void SendEmptySpells(IPacketReceiver receiver)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_SPELLS, 8))
			{
				packet.Write(0L);
				receiver.Send(packet);
			}
		}

		public static void SendVehicleSpells(IPacketReceiver receiver, Vehicle vehicle)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_SPELLS, 18))
			{
				packet.Write(vehicle.EntityId);
				packet.Write((short)0);
				packet.Write(0);
				packet.Write(0x00000101);

				for (var i = 0; i < 10; i++)
				{
					packet.Write((short)0);
					packet.Write((byte)0);
					packet.Write((byte)i + 8);
				}

				packet.Write((short)0);

				receiver.Send(packet);
			}
		}

		public static void SendCastFailed(IPacketReceiver receiver, SpellId spellId, SpellFailedReason reason)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_CAST_FAILED, 8))
			{
				packet.Write(0);				// unkown
				packet.Write((uint)spellId);
				packet.Write((byte)reason);
				receiver.Send(packet);
			}
		}

		public static void SendPetLearnedSpell(IPacketReceiver receiver, SpellId spellId)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_LEARNED_SPELL, 4))
			{
				packet.Write((uint)spellId);

				receiver.Send(packet);
			}
		}

		public static void SendUnlearnedSpell(IPacketReceiver receiver, ushort spell)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_REMOVED_SPELL, 4))
			{
				packet.Write(spell);

				receiver.Send(packet);
			}
		}
		#endregion

		#region Naming
		public static void SendName(IPacketReceiver receiver, uint petId, string name, uint timestamp)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_NAME_QUERY_RESPONSE, 9 + name.Length))
			{
				packet.Write(petId);
				packet.WriteCString(name);
				packet.Write(timestamp);
				packet.Write((byte)0);			// 3.2.0

				receiver.Send(packet);
			}
		}

		public static void SendNameInvalid(IPacketReceiver receiver, PetNameInvalidReason reason, string name)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_NAME_INVALID))
			{
				packet.Write((uint)reason);
				packet.WriteCString(name);
				packet.WriteByte(0);

				receiver.Send(packet);
			}
		}
		#endregion

		#region Action & Mode
		public static void SendActionSound(IPacketReceiver receiver, IEntity pet, uint soundId)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_ACTION_SOUND, 12))
			{
				packet.Write(pet.EntityId);
				packet.Write(soundId);

				receiver.Send(packet);
			}
		}

		public static void SendMode(IPacketReceiver receiver, IEntity pet, PetAttackMode attackMode,
			PetAction action, PetFlags flags)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_MODE, 12))
			{
				packet.Write(pet.EntityId);
				packet.Write((byte)attackMode);
				packet.Write((byte)action);
				packet.Write((ushort)flags);

				receiver.Send(packet);
			}
		}

		public static void SendActionFeedback(IPacketReceiver receiver, PetActionFeedback feedback)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_ACTION_FEEDBACK, 1))
			{
				packet.Write((byte)feedback);

				receiver.Send(packet);
			}
		}
		#endregion

		public static void SendPetRenameable(IPacketReceiver receiver)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_RENAMEABLE, 0))
			{
				receiver.Send(packet);
			}
		}

		public static void SendPetBroken(IPacketReceiver receiver)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_BROKEN, 0))
			{
				receiver.Send(packet);
			}
		}

		#region Stabling

		[PacketHandler(RealmServerOpCode.CMSG_UNSTABLE_PET)]
		public static void HandleUnstablePet(IRealmClient client, RealmPacketIn packet)
		{
			var npcGuid = packet.ReadEntityId();
			var petNumber = packet.ReadUInt32();

			var chr = client.ActiveCharacter;
			var stableMaster = chr.Region.GetObject(npcGuid) as NPC;

			PetMgr.DeStablePet(chr, stableMaster, petNumber);
		}

		[PacketHandler(RealmServerOpCode.CMSG_STABLE_PET)]
		public static void HandleStablePet(IRealmClient client, RealmPacketIn packet)
		{
			var guid = packet.ReadEntityId();
			var chr = client.ActiveCharacter;
			var stableMaster = chr.Region.GetObject(guid) as NPC;

			PetMgr.StablePet(chr, stableMaster);
		}

		[PacketHandler(RealmServerOpCode.CMSG_BUY_STABLE_SLOT)]
		public static void HandleBuyStableSlot(IRealmClient client, RealmPacketIn packet)
		{
			var guid = packet.ReadEntityId();
			var chr = client.ActiveCharacter;
			var stableMaster = chr.Region.GetObject(guid) as NPC;

			PetMgr.BuyStableSlot(chr, stableMaster);
		}

		[PacketHandler(RealmServerOpCode.CMSG_STABLE_SWAP_PET)]
		public static void HandleStableSwapPet(IRealmClient client, RealmPacketIn packet)
		{
			var guid = packet.ReadEntityId();
			var petNumber = packet.ReadUInt32();

			var chr = client.ActiveCharacter;
			var stableMaster = chr.Region.GetObject(guid) as NPC;

			PetMgr.SwapStabledPet(chr, stableMaster, petNumber);
		}

		[PacketHandler(RealmServerOpCode.MSG_LIST_STABLED_PETS)]
		public static void HandleListStabledPets(IRealmClient client, RealmPacketIn packet)
		{
			var guid = packet.ReadEntityId();
			var chr = client.ActiveCharacter;
			var stableMaster = chr.Region.GetObject(guid) as NPC;

			PetMgr.ListStabledPets(chr, stableMaster);
		}

		public static void SendStableResult(IPacketReceiver receiver, StableResult result)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_STABLE_RESULT, 1))
			{
				packet.Write((byte)result);
				receiver.Send(packet);
			}
		}

		/// <summary>
		/// Send the stabled pets list packet to the client
		/// </summary>
		/// <param name="receiver">The client to receive the packet.</param>
		/// <param name="stableMaster">The stable the client is interacting with.</param>
		/// <param name="numStableSlots">The number of stable slots the character owns.</param>
		/// <param name="pets">An array of NPCs containing the ActivePet and the StabledPets</param>
		public static void SendStabledPetsList(IPacketReceiver receiver, NPC stableMaster, byte numStableSlots,
			List<PermanentPetRecord> pets)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_LIST_STABLED_PETS))
			{
				byte count = 1;


				packet.Write(stableMaster.EntityId);
				packet.Write((byte)pets.Count);
				packet.Write(numStableSlots);

				foreach (var pet in pets)
				{
					// write in the Active/Stabled Pet(s)
					packet.Write(pet.PetNumber);
					packet.Write((uint)pet.EntryId);
					packet.Write(pet.Level);
					packet.Write(pet.Name);

					if (pet.IsActivePet && !pet.Flags.HasFlag(PetFlags.Stabled))
					{
						packet.Write((byte)(0x01));
					}
					else if (!pet.IsActivePet && pet.Flags.HasFlag(PetFlags.Stabled))
					{
						packet.Write((byte)(0x02 + count));
						count++;
					}
					else
					{
						log.Warn("{0} tried to send a pet list that included a pet that is marked as both active and stabled.", receiver);
					}
				}

				receiver.Send(packet);
			}
		}

		#endregion

		#region Talents

		[PacketHandler(RealmServerOpCode.CMSG_PET_LEARN_TALENT)]
		public static void HandlePetLearnTalent(IRealmClient client, RealmPacketIn packet)
		{
			var petGuid = packet.ReadEntityId();
			var talentId = (TalentId)packet.ReadInt32();
			var rank = packet.ReadInt32();						// 0 based rank

			var chr = client.ActiveCharacter;
			var pet = chr.Region.GetObject(petGuid) as NPC;

			if (pet != null)
			{
				if (chr.ActivePet != pet) return;

				pet.Talents.Learn(talentId, rank);
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_PET_UNLEARN)]
		public static void HandlePetUnlearn(IRealmClient client, RealmPacketIn packet)
		{
			var petGuid = packet.ReadEntityId();

			var chr = client.ActiveCharacter;
			var pet = chr.Region.GetObject(petGuid) as NPC;

			if (pet != null && pet.HasTalents && pet.Master == chr)
			{
				pet.Talents.ResetTalents();
			}
		}

		#endregion
	}
}