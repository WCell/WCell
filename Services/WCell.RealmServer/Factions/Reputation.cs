/*************************************************************************
 *
 *   file		: Reputation.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-04-29 19:12:32 +0200 (on, 29 apr 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 881 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants.Factions;
using WCell.RealmServer.Database;
using WCell.Util;

namespace WCell.RealmServer.Factions
{
    public class Reputation
    {
        public const int Max = (int)Standing.Exalted + 999;
        public const int Min = (int)Standing.Hostile - 36000;
        /// <summary>
        /// Discounts indexed by StandingLevel
        /// </summary>
        public static uint[] DiscountPercents = new[] {
			0u,
			0u,
			0u,
			0u,
			5u,
			10u,
			15u,
			20u
		};

        Standing m_standing;
        readonly ReputationRecord m_record;

        public readonly Faction Faction;

        /// <summary>
        /// Loads an existing Reputation from the given Record.
        /// </summary>
        public Reputation(ReputationRecord record, Faction faction)
        {
            m_record = record;
            Faction = faction;
            m_standing = GetStanding(record.Value);
        }

        public Reputation(ReputationRecord record, Faction faction, int defaultValue, ReputationFlags defaultFlags)
        {
            m_record = record;
            m_record.ReputationIndex = faction.ReputationIndex;
            m_record.Value = defaultValue;
            m_record.Flags = defaultFlags;

            Faction = faction;
            m_standing = GetStanding(defaultValue);

            m_record.Save();
        }

        #region Properties

        /// <summary>
        /// The reputation value
        /// </summary>
        public int Value
        {
            get { return m_record.Value; }
        }

        public Standing Standing
        {
            get { return m_standing; }
        }

        /// <summary>
        /// Exalted, Honored, Neutral, Hated
        /// </summary>
        public StandingLevel StandingLevel
        {
            get { return GetStandingLevel(m_record.Value); }
        }

        public ReputationFlags Flags
        {
            get
            {
                return m_record.Flags;
            }
        }

        // The following properties' reputation requirements
        // have been changed by blizzard before, it thus is recommended
        // to use these properties and not check for the reputation value
        // itself.

        /// <summary>
        /// Whether racial and faction mounts/tabards etc can be purchased.
        /// </summary>
        public bool SpecialItems
        {
            get { return m_standing >= Standing.Exalted; }
        }

        /// <summary>
        /// Whether Heroic mode keys can be purchased for Outland dungeons.
        /// <see href="http://www.wowwiki.com/Heroic"/>
        /// </summary>
        public bool HeroicModeAllowed
        {
            get { return m_standing >= Standing.Honored; }
        }

        /// <summary>
        /// Enough reputation to interact with NPCs of that Faction
        /// </summary>
        public bool CanInteract
        {
            get { return m_standing >= Standing.Neutral; }
        }

        /// <summary>
        /// Either very bad rep or the player declared war.
        /// Will cause mobs to attack on sight.
        /// </summary>
        public bool Hostile
        {
            get
            {
                return DeclaredWar || IsHostileStanding(m_standing);
            }
        }

        #endregion Properties

        #region Flag based Properties

        public bool IsVisible
        {
            get
            {
                return m_record.Flags.HasFlag(ReputationFlags.Visible);
            }
            internal set
            {
                // We can't make a faction visible if its forced invis
                if (IsForcedInvisible)
                    return;

                if (value)
                {
                    m_record.Flags |= ReputationFlags.Visible;
                }
                else
                {
                    m_record.Flags &= ~ReputationFlags.Visible;
                }
            }
        }

        /// <summary>
        /// whether the player actively declared war
        /// </summary>
        public bool DeclaredWar
        {
            get
            {
                return m_record.Flags.HasFlag(ReputationFlags.AtWar);
            }
            internal set
            {
                // We can't declare war if peace is forced
                if (IsForcedAtPeace)
                    return;

                if (value)
                {
                    m_record.Flags |= ReputationFlags.AtWar;
                }
                else
                {
                    m_record.Flags &= ~ReputationFlags.AtWar;
                }
            }
        }

        public bool IsHidden
        {
            get
            {
                return m_record.Flags.HasFlag(ReputationFlags.Hidden);
            }
            set
            {
                if (value)
                {
                    m_record.Flags |= ReputationFlags.Hidden;
                }
                else
                {
                    m_record.Flags &= ~ReputationFlags.Hidden;
                }
            }
        }

        public bool IsForcedInvisible
        {
            get
            {
                return m_record.Flags.HasFlag(ReputationFlags.ForcedInvisible);
            }
            internal set
            {
                if (value)
                {
                    m_record.Flags |= ReputationFlags.ForcedInvisible;
                }
                else
                {
                    m_record.Flags &= ~ReputationFlags.ForcedInvisible;
                }
            }
        }

        public bool IsForcedAtPeace
        {
            get
            {
                return m_record.Flags.HasFlag(ReputationFlags.ForcedPeace);
            }
            internal set
            {
                if (value)
                {
                    m_record.Flags |= ReputationFlags.ForcedPeace;
                }
                else
                {
                    m_record.Flags &= ~ReputationFlags.ForcedPeace;
                }
            }
        }

        public bool IsInactive
        {
            get { return m_record.Flags.HasFlag(ReputationFlags.Inactive); }
            set
            {
                if (value)
                {
                    m_record.Flags |= ReputationFlags.Inactive;
                }
                else
                {
                    m_record.Flags &= ~ReputationFlags.Inactive;
                }
            }
        }

        #endregion Flag based Properties

        /// <summary>
        /// Changes the reputation value with a specific Faction.
        /// Is called by ReputationCollect.SetValue
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Whether hostility changed due to the stending change</returns>
        internal bool SetValue(int value)
        {
            var oldSt = m_standing;
            var wasHostile = Hostile;

            m_standing = GetStanding(value);
            var nowHostile = Hostile;

            m_record.Value = value;
            return (oldSt != m_standing) && (wasHostile != nowHostile);
        }

        #region Static Helpers

        // Standings, sorted from highest to lowest
        public static readonly Standing[] Standings = (Standing[])Enum.GetValues(typeof(Standing));

        static Reputation()
        {
            Array.Sort(Standings);
        }

        public static Standing GetStanding(int repValue)
        {
            // go from bottom to bottom
            for (int i = Standings.Length - 1; i >= 0; i--)
            {
                if (repValue >= (int)Standings[i])
                {
                    return Standings[i];
                }
            }
            return Standing.Hated;
        }

        public static StandingLevel GetStandingLevel(int repValue)
        {
            // go from top to bottom
            for (int i = 0; i < Standings.Length; i++)
            {
                if (repValue >= (int)Standings[i])
                {
                    return (StandingLevel)(Standings.Length - i);
                }
            }
            return StandingLevel.Hated;
        }

        public static bool IsHostileStanding(Standing standing)
        {
            return standing <= Standing.Hostile;
        }

        public static uint GetReputationDiscountPct(StandingLevel lvl)
        {
            return DiscountPercents.Get((uint)lvl);
        }

        #endregion Static Helpers
    }
}