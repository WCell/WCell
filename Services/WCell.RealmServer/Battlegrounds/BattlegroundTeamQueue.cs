using System.Collections.Generic;
using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Battlegrounds
{
    /// <summary>
    /// Each side of a Battelground has its own Queue
    /// </summary>
    public abstract class BattlegroundTeamQueue
    {
        protected readonly BattlegroundQueue _parentQueue;
        protected readonly BattlegroundSide m_Side;
        protected int m_chrCount;

        public readonly LinkedList<BattlegroundRelation> PendingRequests = new LinkedList<BattlegroundRelation>();

        protected BattlegroundTeamQueue(BattlegroundQueue parentQueue, BattlegroundSide side)
        {
            _parentQueue = parentQueue;
            m_Side = side;
        }

        /// <summary>
        /// Warning: May be null if this belongs to an <see cref="InstanceBattlegroundQueue"/> after the BG has been disposed
        /// </summary>
        public BattlegroundQueue ParentQueue
        {
            get { return _parentQueue; }
        }

        public int CharacterCount
        {
            get { return m_chrCount; }
        }

        public int RelationCount
        {
            get { return PendingRequests.Count; }
        }

        public BattlegroundSide Side
        {
            get { return m_Side; }
        }

        public ICharacterSet GetCharacterSet(Character chr, bool asGroup)
        {
            // we are working with a non-synchronized Character object here
            if (asGroup)
            {
                var group = chr.Group;
                if (group == null || !group.IsLeader(chr))
                {
                    // invalid request
                    GroupHandler.SendResult(chr, GroupResult.DontHavePermission);
                }
                else
                {
                    var chrs = group.GetAllCharacters();

                    // if no characters in the group, return the original character
                    if (chrs.Length == 0)
                        return chr;

                    // make sure the group isn't bigger than the max team size for this BG
                    if (chrs.Length > _parentQueue.Template.MaxPlayersPerTeam)
                    {
                        BattlegroundHandler.SendBattlegroundError(chr, BattlegroundJoinError.GroupJoinedNotEligible);
                        return null;
                    }

                    // make sure there are no deserters
                    foreach (Character groupChr in chrs)
                    {
                        if (groupChr != null && groupChr.Battlegrounds.IsDeserter)
                        {
                            BattlegroundHandler.SendBattlegroundError(group, BattlegroundJoinError.Deserter);
                            return null;
                        }
                    }

                    var list = new SynchronizedCharacterList(Side.GetFactionGroup());

                    foreach (var groupChr in chrs)
                    {
                        if (groupChr.IsInWorld && (groupChr.GodMode || _parentQueue.CanEnter(groupChr)))
                        {
                            list.Add(groupChr);
                        }
                        else
                        {
                            // cannot join the same team
                            BattlegroundHandler.SendBattlegroundError(groupChr, BattlegroundJoinError.GroupJoinedNotEligible);
                        }
                    }

                    return list;
                }
            }
            else
            {
                // enqueue the character if they aren't a deserter
                if (!chr.Battlegrounds.IsDeserter)
                    return chr;

                BattlegroundHandler.SendBattlegroundError(chr, BattlegroundJoinError.Deserter);
            }

            return null;
        }

        internal void Enqueue(BattlegroundRelation request)
        {
            if (_parentQueue.RequiresLocking)
            {
                lock (PendingRequests)
                {
                    PendingRequests.AddLast(request);
                    m_chrCount += request.Characters.CharacterCount;
                }
            }
            else
            {
                PendingRequests.AddLast(request);
                m_chrCount += request.Characters.CharacterCount;
            }
        }

        public BattlegroundRelation Enqueue(Character chr, bool asGroup)
        {
            var chrs = GetCharacterSet(chr, asGroup);
            if (chrs != null)
            {
                return Enqueue(chrs);
            }
            return null;
        }

        public virtual BattlegroundRelation Enqueue(ICharacterSet chrs)
        {
            var request = new BattlegroundRelation(this, chrs);

            chrs.ForeachCharacter(chr =>
                chr.ExecuteInContext(() =>
                {
                    chr.Battlegrounds.AddRelation(request);
                }));

            Enqueue(request);
            return request;
        }

        public BattlegroundQueue ParentQueueBase
        {
            get { return _parentQueue; }
        }

        internal void Remove(BattlegroundRelation relation)
        {
            if (_parentQueue.RequiresLocking)
            {
                lock (PendingRequests)
                {
                    RemoveUnlocked(relation);
                }
            }
            else
            {
                RemoveUnlocked(relation);
            }
        }

        private void RemoveUnlocked(BattlegroundRelation relation)
        {
            var node = PendingRequests.First;
            while (node != null)
            {
                if (node.Value == relation)
                {
                    m_chrCount -= relation.Characters.CharacterCount;

                    var next = node.Next;
                    relation.IsEnqueued = false;
                    PendingRequests.Remove(node);
                    node = next;

                    if (node == null)
                    {
                        // was the last node
                        break;
                    }

                    // next node already selected
                    continue;
                }
                node = node.Next;
            }
        }

        /// <summary>
        /// Removes the given amount of Characters from this Queue and adds them
        /// to the given <see cref="Battleground"/>
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="bg"></param>
        /// <returns>The amount of dequeued Characters</returns>
        internal int DequeueCharacters(int amount, Battleground bg)
        {
            bg.EnsureContext();

            var node = PendingRequests.First;
            if (node == null)
            {
                return 0;
            }

            if (_parentQueue.RequiresLocking)
            {
                lock (PendingRequests)
                {
                    return Dequeue(amount, bg, node);
                }
            }
            else
            {
                return Dequeue(amount, bg, node);
            }
        }

        private int Dequeue(int amount, Battleground bg, LinkedListNode<BattlegroundRelation> node)
        {
            var team = bg.GetTeam(Side);
            var added = 0;

            do
            {
                var relation = node.Value;
                if (relation.Count <= amount)
                {
                    m_chrCount -= relation.Characters.CharacterCount;

                    added += team.Invite(relation.Characters);

                    relation.IsEnqueued = false;

                    var next = node.Next;
                    PendingRequests.Remove(node);
                    node = next;

                    if (node == null)
                    {
                        // was the last node
                        break;
                    }

                    // next node already selected
                    continue;
                }
            } while ((node = node.Next) != null && added <= amount);

            return added;
        }
    }
}