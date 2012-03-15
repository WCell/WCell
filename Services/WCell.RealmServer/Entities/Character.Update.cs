﻿/*************************************************************************
 *
 *   file		: Owner.Update.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-30 10:02:00 +0100 (lø, 30 jan 2010) $

 *   revision		: $Rev: 1234 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.UpdateFields;
using WCell.Util.Collections;

namespace WCell.RealmServer.Entities
{
    /// <summary>
    /// TODO: Move Update and BroadcastValueUpdate for Character together, since else we sometimes
    /// have to fetch everything in our environment twice in a single map update
    /// </summary>
    public partial class Character
    {
        public static new readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Player);

        protected override UpdateFieldCollection _UpdateFieldInfos
        {
            get { return UpdateFieldInfos; }
        }

        public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
        {
            get { return UpdateFieldHandler.DynamicPlayerHandlers; }
        }

        /// <summary>
        /// The UpdatePacket that will be sent to this Character
        /// </summary>
        internal UpdatePacket m_updatePacket = new UpdatePacket(2048);

        /// <summary>
        /// The amount of UpdateBlocks in <see cref="UpdatePacket">m_updatePacket</see>
        /// </summary>
        internal uint UpdateCount;

        private HashSet<Item> m_itemsRequiringUpdates = new HashSet<Item>();

        /// <summary>
        /// All Characters that recently were inspecting our inventory
        /// </summary>
        private HashSet<Character> m_observers = new HashSet<Character>();

        /// <summary>
        /// Messages to be processed by the map after updating of the environment (sending of Update deltas etc).
        /// </summary>
        private readonly LockfreeQueue<Action> m_environmentQueue = new LockfreeQueue<Action>();

        protected bool m_initialized;

        private Unit observing;

        public Unit Observing
        {
            get { return observing ?? this; }
            set { observing = value; }
        }

        #region Messages

        /// <summary>
        /// Will be executed by the current map we are currently in or enqueued and executed,
        /// once we re-enter a map
        /// </summary>
        public void AddPostUpdateMessage(Action action)
        {
            m_environmentQueue.Enqueue(action);
        }

        #endregion Messages

        /// <summary>
        /// All Characters that are currently inspecting this one.
        /// Don't manipulate this collection - Use <see cref="AddObserver"/> instead.
        /// Might be null.
        /// </summary>
        /// <remarks>Requires map context.</remarks>
        public HashSet<Character> Observers
        {
            get { return m_observers; }
        }

        #region Owned objects

        internal void AddItemToUpdate(Item item)
        {
            m_itemsRequiringUpdates.Add(item);
        }

        /// <summary>
        /// Removes the given item visually from the Client.
        /// Do not call this method - but use Item.Remove instead.
        /// </summary>
        internal void RemoveOwnedItem(Item item)
        {
            //if (m_itemsRequiringUpdates.Remove(item))
            m_itemsRequiringUpdates.Remove(item);
            m_environmentQueue.Enqueue(() =>
            {
                item.SendDestroyToPlayer(this);
                if (m_observers == null)
                {
                    return;
                }

                foreach (var observer in m_observers)
                {
                    item.SendDestroyToPlayer(observer);
                }
            });
        }

        #endregion Owned objects

        #region World Knowledge

        /// <summary>
        /// Resends all updates of everything
        /// </summary>
        public void ResetOwnWorld()
        {
            MovementHandler.SendNewWorld(Client, MapId, ref m_position, Orientation);
            ClearSelfKnowledge();
        }

        /// <summary>
        /// Clears known objects and leads to resending of the creation packet
        /// during the next Map-Update.
        /// This is only needed for teleporting or body-transfer.
        /// Requires map context.
        /// </summary>
        internal void ClearSelfKnowledge()
        {
            KnownObjects.Clear();
            NearbyObjects.Clear();
            if (m_observers != null) m_observers.Clear();

            foreach (var item in m_inventory.GetAllItems(true))
            {
                item.m_unknown = true;
                m_itemsRequiringUpdates.Add(item);
            }
        }

        /// <summary>
        /// Will resend update packet of the given object
        /// </summary>
        public void InvalidateKnowledgeOf(WorldObject obj)
        {
            KnownObjects.Remove(obj);
            NearbyObjects.Remove(obj);

            obj.SendDestroyToPlayer(this);
        }

        /// <summary>
        /// Whether the given Object is visible to (and thus in broadcast-range of) this Character
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool KnowsOf(WorldObject obj)
        {
            return KnownObjects.Contains(obj);
        }

        /// <summary>
        /// Collects all update-masks from nearby objects
        /// </summary>
        internal void UpdateEnvironment(HashSet<WorldObject> updatedObjects)
        {
            var toRemove = WorldObjectSetPool.Obtain();
            toRemove.AddRange(KnownObjects);
            toRemove.Remove(this);

            NearbyObjects.Clear();

            if (m_initialized)
            {
                Observing.IterateEnvironment(BroadcastRange, (obj) =>
                {
                    if (!Observing.IsInPhase(obj))
                    {
                        return true;
                    }

                    NearbyObjects.Add(obj);

                    //ensure "this" never goes out of range
                    //if we are observing another units broadcasts
                    if (!Observing.CanSee(obj) && !ReferenceEquals(obj, this))
                    {
                        return true;
                    }

                    if (!KnownObjects.Contains(obj))
                    {
                        // encountered new object
                        obj.WriteObjectCreationUpdate(this);
                        OnEncountered(obj);
                        if (obj.RequiresUpdate)
                        {
                            // make sure that it won't send its values next round again
                            updatedObjects.Add(obj);
                        }
                    }
                    else if (obj.RequiresUpdate)
                    {
                        updatedObjects.Add(obj);
                        obj.WriteObjectValueUpdate(this);
                    }

                    toRemove.Remove(obj);	// still in range, no need to remove it
                    return true;
                });

                // Update Items
                if (m_itemsRequiringUpdates.Count > 0)
                {
                    foreach (var item in m_itemsRequiringUpdates)
                    {
                        if (item.m_unknown)
                        {
                            // creation update
                            item.m_unknown = false;
                            if (m_observers != null && item.IsEquippedItem)
                            {
                                foreach (var chr in m_observers)
                                {
                                    item.WriteObjectCreationUpdate(chr);
                                }
                            }
                            item.WriteObjectCreationUpdate(this);
                        }
                        else
                        {
                            // value update
                            if (m_observers != null && item.IsEquippedItem)
                            {
                                foreach (var chr in m_observers)
                                {
                                    item.WriteObjectValueUpdate(chr);
                                }
                            }
                            item.WriteObjectValueUpdate(this);
                        }
                        item.ResetUpdateInfo();
                    }
                    m_itemsRequiringUpdates.Clear();
                }

                //update group member stats for out of range players
                if (m_groupMember != null)
                {
                    m_groupMember.Group.UpdateOutOfRangeMembers(m_groupMember);
                }

                // send update packet
                if (UpdateCount > 0)
                {
                    SendOwnUpdates();
                }

                // delete objects that are not in range anymore
                foreach (var obj in toRemove)
                {
                    OnOutOfRange(obj);
                }

                if (toRemove.Count > 0)
                {
                    SendOutOfRangeUpdate(this, toRemove);
                }
            }

            // init player, delete Items etc
            Action action;
            while (m_environmentQueue.TryDequeue(out action))
            {
                var ac = action;
                // need to Add a message because Update state will be reset after method call
                AddMessage(ac);
            }

            // check rest state
            if (m_restTrigger != null)
            {
                UpdateRestState();
            }

            toRemove.Clear();
            WorldObjectSetPool.Recycle(toRemove);
        }

        /// <summary>
        /// Check if this Character is still resting (if it was resting before)
        /// </summary>
        private void UpdateRestState()
        {
            if (!m_restTrigger.IsInArea(this))
            {
                RestTrigger = null;
            }
        }

        /// <summary>
        /// Sends Item-information and Talents to the given Character and keeps them updated until they
        /// are out of range.
        /// </summary>
        /// <param name="chr"></param>
        public void AddObserver(Character chr)
        {
            if (m_observers == null)
            {
                m_observers = new HashSet<Character>();
            }

            if (!m_observers.Contains(chr))
            {
                // only send item creation if Character wasn't already observing
                for (var i = InventorySlot.Bag1; i < InventorySlot.Bank1; i++)
                {
                    var item = m_inventory[i];
                    if (item != null)
                    {
                        item.WriteObjectCreationUpdate(chr);
                    }
                }

                m_observers.Add(chr);
            }

            TalentHandler.SendInspectTalents(chr);
        }

        #endregion World Knowledge

        protected internal override void WriteObjectCreationUpdate(Character chr)
        {
            base.WriteObjectCreationUpdate(chr);
            //for (var i = InventorySlot.Head; i < InventorySlot.Bag1; i++)
            //{
            //    var item = m_inventory[i];
            //    if (item != null)
            //    {
            //        item.WriteObjectCreationUpdate(chr);
            //    }
            //}
        }

        //protected override void WriteMovementUpdate(PrimitiveWriter packet, UpdateFieldFlags relation)
        //{
        //    base.WriteMovementUpdate(packet, relation);
        //}

        private void SendOwnUpdates()
        {
            m_updatePacket.Position = m_updatePacket.HeaderSize;

            m_updatePacket.Write(UpdateCount);
            SendUpdatePacket(this, m_updatePacket);

            //m_updatePacket.Reset();
            m_updatePacket.Close();
            m_updatePacket = new UpdatePacket();
            UpdateCount = 0;
        }

        public override UpdateFieldFlags GetUpdateFieldVisibilityFor(Character chr)
        {
            if (chr == this)
            {
                return UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.GroupOnly | UpdateFieldFlags.Public;
            }
            return base.GetUpdateFieldVisibilityFor(chr);
        }

        protected override UpdateType GetCreationUpdateType(UpdateFieldFlags flags)
        {
            return flags.HasAnyFlag(UpdateFieldFlags.Private) ? UpdateType.CreateSelf : UpdateType.Create;
        }

        public void PushFieldUpdate(UpdateFieldId field, uint value)
        {
            if (!IsInWorld)
            {
                // set the value and don't push, we aren't in game so we'll get it on the next self full update
                SetUInt32(field, value);

                return;
            }

            using (var packet = GetFieldUpdatePacket(field, value))
            {
                SendUpdatePacket(this, packet);
            }
        }

        public void PushFieldUpdate(UpdateFieldId field, EntityId value)
        {
            if (!IsInWorld)
            {
                // set the value and don't push, we aren't in game so we'll get it on the next self full update
                SetEntityId(field, value);

                return;
            }

            using (var packet = GetFieldUpdatePacket(field, value))
            {
                SendUpdatePacket(this, packet);
            }
        }

        #region IUpdatable

        public override void Update(int dt)
        {
            base.Update(dt);

            if (m_isLoggingOut)
            {
                m_logoutTimer.Update(dt);
            }
            if (m_corpseReleaseTimer != null)
            {
                m_corpseReleaseTimer.Update(dt);
            }
            if (PvPEndTime != null)
            {
                PvPEndTime.Update(dt);
            }
            if (PlayerSpells.Runes != null)
            {
                PlayerSpells.Runes.UpdateCooldown(dt);
            }
        }

        public override UpdatePriority UpdatePriority
        {
            get
            {
                return UpdatePriority.HighPriority;
            }
        }

        #endregion IUpdatable
    }
}