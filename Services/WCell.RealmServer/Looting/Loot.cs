using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.Constants.Looting;
using WCell.Util.Logging;

namespace WCell.RealmServer.Looting
{
	/// <summary>
	/// Represents a pile of lootable objects and its looters
	/// 
	/// TODO: Roll timeout (and loot timeout?)
	/// </summary>
	public abstract class Loot
	{
		/// <summary>
		/// The set of all who are allowed to loot. 
		/// If everyone released the Loot, it becomes available to everyone else?
		/// </summary>
		public IList<LooterEntry> Looters;

		private uint m_Money;

		/// <summary>
		/// The total amount of money to be looted
		/// </summary>
		public uint Money
		{
			get { return m_Money; }
			set
			{
				m_Money = value;
				m_moneyLooted = m_Money == 0;
			}
		}

		/// <summary>
		/// All items that can be looted.
		/// </summary>
		public LootItem[] Items;

		/// <summary>
		/// The Container being looted
		/// </summary>
		public ILootable Lootable;

		/// <summary>
		/// The method that determines how to distribute the Items
		/// </summary>
		public LootMethod Method;

		/// <summary>
		/// The Group who is looting this Loot. 
		/// If all members of the group release it, the Loot becomes available to everyone else.
		/// </summary>
		public Group Group;

		protected int m_takenCount;

		/// <summary>
		/// Amount of items that are freely available
		/// </summary>
		protected int m_freelyAvailableCount;

		/// <summary>
		/// Whether money was already looted
		/// </summary>
		protected bool m_moneyLooted;

		/// <summary>
		/// Whether none of the initial looters is still claiming this.
		/// </summary>
		protected bool m_released;

		/// <summary>
		/// The least ItemQuality that is decided through rolls/MasterLooter correspondingly.
		/// </summary>
		public ItemQuality Threshold;

		protected Loot()
		{
			Looters = new List<LooterEntry>();
		}

		protected Loot(ILootable looted, uint money, LootItem[] items)
			: this()
		{
			Money = money;
			Items = items;
			Lootable = looted;
		}

		#region Properties
		/// <summary>
		/// The amount of Items that have already been taken
		/// </summary>
		public int TakenCount
		{
			get
			{
				return m_takenCount;
			}
		}

		/// <summary>
		/// Amount of remaining items
		/// </summary>
		public int RemainingCount
		{
			get
			{
				return Items.Length - m_takenCount;
			}
		}

		/// <summary>
		/// Amount of items that are freely available to everyone:
		/// Items that are passed by everyone or that have been left over by the looter whose turn it is in RoundRobin
		/// </summary>
		public int FreelyAvailableCount
		{
			get
			{
				return m_freelyAvailableCount;
			}
			internal set
			{
				m_freelyAvailableCount = value;
			}
		}

		/// <summary>
		/// Whether RoundRobin applies (by default applies if LootMethod == RoundRobin or -for items below threshold- when using most of the other methods too)
		/// </summary>
		public bool UsesRoundRobin
		{
			get { return Method == LootMethod.RoundRobin; }
		}

		/// <summary>
		/// Whether none of the initial looters is still looking at this (everyone else may thus look at it)
		/// </summary>
		public bool IsReleased
		{
			get
			{
				return m_released;
			}
			internal set
			{
				if (m_released != value)
				{
					m_released = value;
					if (value)
					{
						if (RemainingCount == 0 && m_moneyLooted)
						{
							// last looter released and there are no more Items left
							Dispose();
						}
					}
				}
			}
		}

		/// <summary>
		/// Whether the money has already been given out
		/// </summary>
		public bool IsMoneyLooted
		{
			get
			{
				return m_moneyLooted;
			}
		}

		public bool MustKneelWhileLooting
		{
			get
			{
				return Lootable is WorldObject;
			}
		}

		public bool IsGroupLoot { get { return Lootable.UseGroupLoot; } }

		public abstract LootResponseType ResponseType { get; }
		#endregion

		/// <summary>
		/// Adds all initial Looters of nearby Characters who may loot this Loot.
		/// When all of the initial Looters gave up the Loot, the Loot becomes free for all.
		/// </summary>
		public void Initialize(Character chr, IList<LooterEntry> looters, MapId mapid)
		{
			Looters = looters;
			if (IsGroupLoot)
			{
				var groupMember = chr.GroupMember;
				if (groupMember != null)
				{
					Group = groupMember.Group;
					Method = Group.LootMethod;
					Threshold = Group.LootThreshold;

					var decision = Method == LootMethod.MasterLoot ? LootDecision.Master : LootDecision.Rolling;

					IList<LooterEntry> nearbyLooters = null;

					// TODO: masterlooter
					foreach (var item in Items)
					{
						if (item.Template.Flags.HasFlag(ItemFlags.MultiLoot) ||
							item.Template.Quality >= Threshold)
						{
							if (UsesRoundRobin)
							{
								nearbyLooters = new List<LooterEntry>();
								Group.GetNearbyLooters(Lootable, chr, nearbyLooters);
							}
							else
							{
								nearbyLooters = Looters;
							}
						}

						if (item.Template.Flags.HasFlag(ItemFlags.MultiLoot))
						{
							item.AddMultiLooters(nearbyLooters);
						}
						else if (item.Template.Quality >= Threshold)
						{
							item.Decision = decision;
							if (decision == LootDecision.Rolling)
							{
								item.RollProgress = new LootRollProgress(this, item, nearbyLooters);
								LootHandler.SendStartRoll(this, item, nearbyLooters, mapid);
							}
						}
					}
					return;
				}
			}
			Method = LootMethod.FreeForAll;
		}


		/// <summary>
		/// This gives the money to everyone involved. Will only work the first time its called. 
		/// Afterwards <c>IsMoneyLooted</c> will be true.
		/// </summary>
		public void GiveMoney()
		{
			if (!m_moneyLooted)
			{
				if (Group == null)
				{
					// we only have a single looter
					var looter = Looters.FirstOrDefault();
					if (looter != null && looter.Owner != null)
					{
						m_moneyLooted = true;
						SendMoney(looter.Owner, Money);
					}
				}
				else
				{
					var looters = new List<Character>();
					if (UsesRoundRobin)
					{
						// we only added the RoundRobin member, so we have to find everyone in the radius for the money now

						var looter = Looters.FirstOrDefault();
						if (looter != null && looter.Owner != null)
						{
							looters.Add(looter.Owner);

							WorldObject center;
							if (Lootable is WorldObject)
							{
								center = (WorldObject)Lootable;
							}
							else
							{
								center = looter.Owner;
							}

							GroupMember otherMember;
							var chrs = center.GetObjectsInRadius(LootMgr.LootRadius, ObjectTypes.Player, false, 0);
							foreach (Character chr in chrs)
							{
								if (chr.IsAlive && (chr == looter.Owner ||
									((otherMember = chr.GroupMember) != null && otherMember.Group == Group)))
								{
									looters.Add(chr);
								}
							}
						}
					}
					else
					{
						foreach (var looter in Looters)
						{
							if (looter.m_owner != null)
							{
								looters.Add(looter.m_owner);
							}
						}
					}

					if (looters.Count > 0)
					{
						m_moneyLooted = true;

						var amount = Money / (uint)looters.Count;
						foreach (var looter in looters)
						{
							SendMoney(looter, amount);
							LootHandler.SendMoneyNotify(looter, amount);
						}
					}
				}
				CheckFinished();
			}
		}

		/// <summary>
		/// Gives the receiver the money and informs everyone else
		/// </summary>
		/// <param name="receiver"></param>
		/// <param name="amount"></param>
		protected void SendMoney(Character receiver, uint amount)
		{
			receiver.Money += amount;

			LootHandler.SendClearMoney(this);
		}

		/// <summary>
		/// Checks whether this Loot has been fully looted and if so, dispose and dismember the corpse or consumable object
		/// </summary>
		public void CheckFinished()
		{
			if (m_moneyLooted && m_takenCount == Items.Length)
			{
				Dispose();
			}
		}

		/// <summary>
		/// Returns whether the given looter may loot the given Item.
		/// Make sure the Looter is logged in before calling this Method.
		/// 
		/// TODO: Find the right error messages
		/// TODO: Only give every MultiLoot item to everyone once! Also check for quest-dependencies etc.
		/// </summary>
		public InventoryError CheckTakeItemConditions(LooterEntry looter, LootItem item)
		{
			if (item.Taken)
			{
				return InventoryError.ALREADY_LOOTED;
			}
			if (item.RollProgress != null)
			{
				// TODO: Still being rolled for
				return InventoryError.DONT_OWN_THAT_ITEM;
			}
			if (!looter.MayLoot(this))
			{
				return InventoryError.DontReport;
			}

			var multiLooters = item.MultiLooters;
			if (multiLooters != null)
			{
				if (!multiLooters.Contains(looter))
				{
					if (looter.Owner != null)
					{
						// make sure, Item cannot be seen by client anymore
						LootHandler.SendLootRemoved(looter.Owner, item.Index);
					}
					return InventoryError.DONT_OWN_THAT_ITEM;
				}
				return InventoryError.OK;
			}

			if (!item.Template.CheckLootConstraints(looter.Owner))
			{
				return InventoryError.DONT_OWN_THAT_ITEM;
			}

			if (Method != LootMethod.FreeForAll)
			{
				// definitely Group-Loot
				if ((item.Template.Quality > Group.LootThreshold && !item.Passed) ||
					(Group.MasterLooter != null &&
					 Group.MasterLooter != looter.Owner.GroupMember))
				{
					return InventoryError.DONT_OWN_THAT_ITEM;
				}
			}

			return InventoryError.OK;
		}

		/// <summary>
		/// Try to loot the item at the given index of the current loot
		/// </summary>
		/// <returns>The looted Item or null if Item could not be taken</returns>
		public void TakeItem(LooterEntry entry, uint index, BaseInventory targetCont, int targetSlot)
		{
			LootItem lootItem = null;
			try
			{
				var chr = entry.Owner;
				if (chr != null && index < Items.Length)
				{
					lootItem = Items[index];
					var err = CheckTakeItemConditions(entry, lootItem);
					if (err == InventoryError.OK)
					{
						HandoutItem(chr, lootItem, targetCont, targetSlot);
					}
					else
					{
						ItemHandler.SendInventoryError(chr.Client, null, null, err);
					}
				}
			}
			catch (Exception e)
			{
				LogUtil.ErrorException(e, "{0} threw an Exception while looting \"{1}\" (index = {2}) from {3}",
					entry.Owner, lootItem, index, targetCont);
			}
		}

		/// <summary>
		/// Hands out the given LootItem to the given Character.
		/// </summary>
		/// <remarks>Adds the given container at the given slot or -if not specified- to the next free slot</remarks>
		/// <param name="chr"></param>
		/// <param name="lootItem"></param>
		/// <param name="targetCont"></param>
		/// <param name="targetSlot"></param>
		public void HandoutItem(Character chr, LootItem lootItem, BaseInventory targetCont, int targetSlot)
		{
			InventoryError err;
			var multiLooters = lootItem.MultiLooters;

			int amount;
			if (targetCont != null)
			{
				// specific slot
				amount = lootItem.Amount;
				if (!targetCont.IsValidSlot(targetSlot) || targetCont.Items[targetSlot] != null)
				{
					// slot is not valid or occupied
					err = targetCont.TryAdd(lootItem.Template, ref amount, ItemReceptionType.Loot);
				}
				else
				{
					err = targetCont.TryAdd(lootItem.Template, ref amount, targetSlot, ItemReceptionType.Loot);
				}
			}
			else
			{
				// auto loot
				amount = lootItem.Amount;
				//err = chr.Inventory.TryAdd(lootItem.Template, ref amount);
				err = chr.Inventory.TryAdd(lootItem.Template, ref amount, ItemReceptionType.Loot);
			}

			if (err == InventoryError.OK)
			{
				if (amount < lootItem.Amount && multiLooters == null)
				{
					// Only a part of the item stack was added, a part remains
					// TODO: Amount changed, but amount displayed to looters is still the same (SMSG_LOOT_ITEM_NOTIFY?)
					lootItem.Amount = amount;
				}

				if (multiLooters != null)
				{
					multiLooters.Remove(chr.LooterEntry);
				}
				if (multiLooters == null || multiLooters.Count == 0)
				{
					RemoveItem(lootItem);
				}
			}
			else
			{
				ItemHandler.SendInventoryError(chr.Client, null, null, err);
			}
		}

		/// <summary>
		/// Marks the given Item as taken and removes it from the list of available Items
		/// </summary>
		/// <param name="lootItem"></param>
		public void RemoveItem(LootItem lootItem)
		{
			lootItem.Taken = true;
			m_takenCount++;

			// TODO: Have correct collection of all observing Characters
			foreach (var looter in Looters)
			{
				if (looter.Owner != null)
				{
					LootHandler.SendLootRemoved(looter.Owner, lootItem.Index);
				}
			}
			CheckFinished();
		}

		/// <summary>
		/// Lets the given Character roll for the item at the given index
		/// </summary>
		/// <param name="index"></param>
		/// <param name="type"></param>
		public void Roll(Character chr, uint index, LootRollType type)
		{
			var item = Items[index];
			if (item.RollProgress != null)
			{
				// item is actually being rolled for
				item.RollProgress.Roll(chr, type);

				if (item.RollProgress.IsRollFinished)
				{
					// handout the item, once the Roll finished
					var highest = item.RollProgress.HighestParticipant;

					if (highest != null)
					{
						LootHandler.SendRollWon(highest, this, item, item.RollProgress.HighestEntry);
						HandoutItem(highest, item, null, BaseInventory.INVALID_SLOT);
					}
					else
					{
						// no one online to receive the item (odd) -> Simply remove it
						RemoveItem(item);
					}
					item.RollProgress.Dispose();
				}
			}
		}

		/// <summary>
		/// Disposes this loot, despite the fact that it could still contain something valuable
		/// </summary>
		public void ForceDispose()
		{
			Dispose();
		}

		protected void Dispose()
		{
			if (Looters == null)
			{
				return;
			}

			var looters = Looters;
			Looters = null;

			for (var i = 0; i < looters.Count; i++)
			{
				var looter = looters[i];
				var owner = looter.Owner;
				if (owner != null)
				{
					LootHandler.SendLootReleaseResponse(owner, this);
				}

				if (looter.Loot == this)
				{
					looter.Loot = null;
				}
			}

			OnDispose();
		}

		protected virtual void OnDispose()
		{
			if (Lootable != null)
			{
				Lootable.OnFinishedLooting();
				Lootable = null;
			}
		}

		public void RemoveLooter(LooterEntry entry)
		{
			Looters.Remove(entry);
		}

		/// <summary>
		/// Lets the master looter give the item in the given slot to the given receiver
		/// </summary>
        public void GiveLoot(Character master, Character receiver, byte lootSlot)
        {
            if (master.Group == null) return;
            if (master.Group.MasterLooter.Character != master) return;
            if (receiver.Group == null) return;
            if (master.Group != receiver.Group) return;
            if (!master.Loot.Looters.Contains(receiver.LooterEntry)) return;
            if (Items[lootSlot] == null) return;

            HandoutItem(receiver, Items[lootSlot], null, BaseInventory.INVALID_SLOT);
        }
    }
}