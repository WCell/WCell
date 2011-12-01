using System.IO;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Spells.Auras
{
	public static partial class AuraHandler
	{
		/// <summary>
		/// Cancels a positive aura (by right-clicking on the corresponding icon)
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CANCEL_AURA)]
		public static void HandleCancelCastSpell(IRealmClient client, RealmPacketIn packet)
		{
			var spellId = (SpellId)packet.ReadUInt32();

			var spell = SpellHandler.Get(spellId);
			if (spell != null)
			{
				var chr = client.ActiveCharacter;
				var aura = chr.Auras[spell, true];

				if (aura != null && aura.CanBeRemoved)
				{
					aura.TryRemove(true);
				}
			}
		}

		#region Send Auras
		public static void SendAuraUpdate(Unit owner, Aura aura)
		{
			if (!owner.IsAreaActive) return;
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_AURA_UPDATE))
			{
				owner.EntityId.WritePacked(packet);

				WriteAura(aura, packet);

				owner.SendPacketToArea(packet);
			}
		}

		public static void SendAllAuras(IPacketReceiver rcv, Unit owner)
		{
			if (!owner.IsAreaActive) return;
			using (var packet = CreateAllAuraPacket(owner))
			{
				rcv.Send(packet);
			}
		}

		public static void SendAllAuras(Unit owner)
		{
			if (!owner.IsAreaActive) return;
			using (var packet = CreateAllAuraPacket(owner))
			{
				owner.SendPacketToArea(packet);
			}
		}

		public static RealmPacketOut CreateAllAuraPacket(Unit owner)
		{
			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_AURA_UPDATE_ALL);
			owner.EntityId.WritePacked(packet);
			foreach (var aura in owner.Auras)
			{
				if (aura.IsVisible)
				{
					WriteAura(aura, packet);
				}
			}
			return packet;
		}

		private static void WriteAura(Aura aura, BinaryWriter packet)
		{
			packet.Write(aura.Index);

			packet.Write(aura.Spell.Id);

			packet.Write((byte)aura.Flags);
			packet.Write(aura.Level);
			packet.Write((byte)aura.StackCount);

			// If the target was not the caster
			if (!aura.Flags.HasFlag(AuraFlags.TargetIsCaster))
			{
				aura.CasterReference.EntityId.WritePacked(packet);
			}

            if (aura.Flags.HasFlag(AuraFlags.HasDuration))
			{
				packet.Write(aura.Duration);
				packet.Write(aura.TimeLeft);
			}
		}

		public static void SendRemoveAura(Unit owner, Aura aura)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_AURA_UPDATE))
			{
				owner.EntityId.WritePacked(packet);
				packet.Write(aura.Index);

				// a spellid of 0 tells the client to remove the aura
				packet.Write(0);

				owner.SendPacketToArea(packet);
			}
		}
		#endregion

		/// <summary>
		/// Sends updates to the client for spell-modifier
		/// </summary>
		public static void SendModifierUpdate(Character chr, SpellEffect effect, bool isPercent)
		{
			var type = (SpellModifierType)effect.MiscValue;
			var enhancers = isPercent ? chr.PlayerAuras.SpellModifiersPct : chr.PlayerAuras.SpellModifiersFlat;

			foreach (var bit in effect.AffectMaskBitSet)
			{
				// calculate new amount for current group by looking through all enhancers for the player
				var amount = 0;
				var groupNum = bit >> 5;					// detemine group (0,1 or 2)
				var groupBit = bit - (groupNum << 5);		// determine position within group (0 to 31)
				for (var i = 0; i < enhancers.Count; i++)
				{
					var enhancerEffect = enhancers[i];
					if ((SpellModifierType)enhancerEffect.SpellEffect.MiscValue == type &&
						enhancerEffect.SpellEffect.Spell.SpellClassOptions.SpellClassSet == effect.Spell.SpellClassOptions.SpellClassSet &&
						(enhancerEffect.SpellEffect.AffectMask[groupNum] & (1 << (int)groupBit)) != 0)
					{
						amount += enhancerEffect.SpellEffect.ValueMin;
					}
				}
				SpellHandler.SendSpellModifier(chr, (byte)bit, type, amount, isPercent);
			}
		}
	}
}