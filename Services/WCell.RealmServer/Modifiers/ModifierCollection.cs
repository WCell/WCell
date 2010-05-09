/*************************************************************************
 *
 *   file		: ModifierCollection.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-02-04 21:47:44 +0100 (ma, 04 feb 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 106 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core;
using Cell.Core;
using WCell.RealmServer.Entities;
using System.Threading;

namespace WCell.RealmServer.Modifiers
{
	/// <summary>
	/// Manages the different factors (flat modifier, percentage, etc) of unit modifiers 
    /// such as stats or combat ratings.
	/// </summary>
	public class ModifierCollection
	{
		private Unit m_owner;
        private LinkedList<ModifierType> m_changes;
        private ModifierDictionary m_modifiers;
		private ReaderWriterLockSlim m_changeLock;

		/// <summary>
		/// Creates a <see cref="ModifierCollection" /> object with the given <see cref="Unit" /> as the owner.
		/// </summary>
		/// <param name="unit">the <see cref="Unit" /> that this collection belongs to</param>
		public ModifierCollection(Unit unit)
		{
			m_owner = unit;

            m_modifiers = new ModifierDictionary();

			foreach(ModifierCategory colType in Enum.GetValues(typeof(ModifierCategory)))
			{
                ModifierGroup tempDict = new ModifierGroup();

				foreach(ModifierType statType in Enum.GetValues(typeof(ModifierType)))
				{
					tempDict.Add(statType, 0.0);
				}

                m_modifiers.Add(colType, tempDict);
			}

            m_changes = new LinkedList<ModifierType>();

			m_changeLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		}

        /// <summary>
        /// Checks if a change is flagged in the collection.
        /// </summary>
        /// <param name="statType">the stat type</param>
        /// <returns>true if the stat type is flagged; false otherwise</returns>
        private bool IsChangeFlagged(ModifierType statType)
        {
            return m_changes.Where(statChange => statChange == statType).Any();
        }

		/// <summary>
		/// Adds a given value to the existing value for a specific <see cref="ModifierType" /> in a
		/// specific <see cref="ModifierCategory">collection</see>. 
		/// </summary>
		/// <param name="collectionType">the collection type</param>
		/// <param name="statType">the stat type</param>
		/// <param name="value">the amount to add</param>
		public void AddToStat(ModifierCategory collectionType, ModifierType statType, double value)
		{
			m_changeLock.EnterWriteLock();

			try
			{
				m_modifiers[collectionType][statType] += value;

				if (!IsChangeFlagged(statType))
				{
					m_changes.AddLast(statType);
				}
			}
			finally
			{
				m_changeLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Adds a given value to the existing value for a specific <see cref="ModifierType" /> in a
		/// specific <see cref="ModifierCategory">collection</see>.
		/// </summary>
		/// <param name="collectionType">the collection type</param>
		/// <param name="statType">the stat type</param>
		/// <param name="value">the amount to add</param>
		public void AddToStat(ModifierCategory collectionType, ModifierType statType, short value)
		{
			AddToStat(collectionType, statType, (double)value);
		}

		/// <summary>
		/// Adds a given value to the existing value for a specific <see cref="ModifierType" /> in a
		/// specific <see cref="ModifierCategory">collection</see>. 
		/// </summary>
		/// <param name="collectionType">the collection type</param>
		/// <param name="statType">the stat type</param>
		/// <param name="value">the amount to add</param>
		public void AddToStat(ModifierCategory collectionType, ModifierType statType, float value)
		{
			AddToStat(collectionType, statType, (double)value);
		}

        /// <summary>
        /// Adds a given value to the existing value for a specific <see cref="ModifierType" /> in a
        /// specific <see cref="ModifierCategory">collection</see>. 
        /// </summary>
        /// <param name="collectionType">the collection type</param>
        /// <param name="statType">the stat type</param>
        /// <param name="value">the amount to add</param>
        public void AddToStat(ModifierCategory collectionType, ModifierType statType, int value)
        {
            AddToStat(collectionType, statType, (double)value);
        }

		/// <summary>
		/// Removes a given amount from the existing value for a specific <see cref="ModifierType" /> in a
		/// specific <see cref="ModifierCategory">collection</see>. 
		/// </summary>
		/// <param name="collectionType">the collection type</param>
		/// <param name="statType">the stat type</param>
		/// <param name="value">the amount to remove</param>
		public void RemoveFromStat(ModifierCategory collectionType, ModifierType statType, double value)
		{
			m_changeLock.EnterWriteLock();

			try
			{
				m_modifiers[collectionType][statType] -= value;

				if (!IsChangeFlagged(statType))
				{
					m_changes.AddLast(statType);
				}
			}
			finally
			{
				m_changeLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Removes a given amount from the existing value for a specific <see cref="ModifierType" /> in a
		/// specific <see cref="ModifierCategory">collection</see>. 
		/// </summary>
		/// <param name="collectionType">the collection type</param>
		/// <param name="statType">the stat type</param>
		/// <param name="value">the amount to remove</param>
		public void RemoveFromStat(ModifierCategory collectionType, ModifierType statType, short value)
		{
			RemoveFromStat(collectionType, statType, (double)value);
		}

        /// <summary>
        /// Removes a given amount from the existing value for a specific <see cref="ModifierType" /> in a
        /// specific <see cref="ModifierCategory">collection</see>. 
        /// </summary>
        /// <param name="collectionType">the collection type</param>
        /// <param name="statType">the stat type</param>
        /// <param name="value">the amount to remove</param>
        public void RemoveFromStat(ModifierCategory collectionType, ModifierType statType, float value)
        {
            RemoveFromStat(collectionType, statType, (double)value);
        }

		/// <summary>
		/// Removes a given amount from the existing value for a specific <see cref="ModifierType" /> in a
		/// specific <see cref="ModifierCategory">collection</see>. 
		/// </summary>
		/// <param name="collectionType">the collection type</param>
		/// <param name="statType">the stat type</param>
		/// <param name="value">the amount to remove</param>
		public void RemoveFromStat(ModifierCategory collectionType, ModifierType statType, int value)
		{
			RemoveFromStat(collectionType, statType, (double)value);
		}

		/// <summary>
		/// Sets a specific value for a specific <see cref="ModifierType" /> in a specific 
		/// <see cref="ModifierCategory">collection</see>.
		/// </summary>
		/// <param name="collectionType">the collection type</param>
		/// <param name="statType">the stat type</param>
		/// <param name="value">the amount to set</param>
		public void SetValue(ModifierCategory collectionType, ModifierType statType, double value)
		{
			m_changeLock.EnterWriteLock();

			try {
				m_modifiers[collectionType][statType] = value;

				if (!IsChangeFlagged(statType)) {
					m_changes.AddLast(statType);
				}
			}
			finally {
				m_changeLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Set a value without logging the change 
		/// (used when an Updater makes an internal change and ensures to synchronizes the update itself)
		/// </summary>
		internal void SetValueUnlogged(ModifierCategory collectionType, ModifierType statType, double value)
		{
			m_changeLock.EnterWriteLock();

			try {
				m_modifiers[collectionType][statType] = value;
			}
			finally {
				m_changeLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Sets a specific value for a specific <see cref="ModifierType" /> in a specific 
		/// <see cref="ModifierCategory">collection</see>.
		/// </summary>
		/// <param name="collectionType">the collection type</param>
		/// <param name="statType">the stat type</param>
		/// <param name="value">the amount to set</param>
		public void SetValue(ModifierCategory collectionType, ModifierType statType, short value)
		{
			SetValue(collectionType, statType, (double)value);
		}

        /// <summary>
        /// Sets a specific value for a specific <see cref="ModifierType" /> in a specific 
        /// <see cref="ModifierCategory">collection</see>.
        /// </summary>
        /// <param name="collectionType">the collection type</param>
        /// <param name="statType">the stat type</param>
        /// <param name="value">the amount to set</param>
        public void SetValue(ModifierCategory collectionType, ModifierType statType, float value)
        {
            SetValue(collectionType, statType, (double)value);
        }

		/// <summary>
		/// Sets a specific value for a specific <see cref="ModifierType" /> in a specific 
		/// <see cref="ModifierCategory">collection</see>.
		/// </summary>
		/// <param name="collectionType">the collection type</param>
		/// <param name="statType">the stat type</param>
		/// <param name="value">the amount to set</param>
		public void SetValue(ModifierCategory collectionType, ModifierType statType, int value)
		{
			SetValue(collectionType, statType, (double)value);
		}

		/// <summary>
		/// Gets the value for a specific <see cref="ModifierType" /> and a specific <see cref="ModifierCategory" />. 
		/// </summary>
		/// <param name="collectionType">the collection type</param>
		/// <param name="statType">the stat type</param>
		/// <returns>the value for the given collection type/stat type</returns>
		public float GetValue(ModifierCategory collectionType, ModifierType statType)
		{
			return (float)m_modifiers[collectionType][statType];
		}

		/// <summary>
		/// Gets the modified (total/final) value of a specific <see cref="ModifierType" /> for the 
		/// <see cref="Unit">owner</see>.
		/// </summary>
		/// <param name="statType">the stat type</param>
		/// <returns>the modified value for the given <see cref="ModifierType" />; or <see cref="Double.MinValue" /> 
		/// if there was an exception when executing the calculator.</returns>
		public double GetModified(ModifierType statType)
		{
			m_changeLock.EnterReadLock();

			try
			{
				double staticVal = m_modifiers[ModifierCategory.BaseModifier][statType] +
								   m_modifiers[ModifierCategory.PositiveModifier][statType] +
								   m_modifiers[ModifierCategory.NegativeModifier][statType];

				double posPercentMod = staticVal * m_modifiers[ModifierCategory.PositiveMultiplier][statType];
				double negPercentMod = staticVal * m_modifiers[ModifierCategory.NegativeMultiplier][statType];

				return staticVal + posPercentMod + negPercentMod;
			}
			finally
			{
				m_changeLock.ExitReadLock();
			}
		}

		/// <summary>
		/// Manually flags a modifier as changed.
		/// </summary>
		/// <param name="modifierType"></param>
		public void FlagChange(ModifierType modifierType)
		{
			m_changeLock.EnterWriteLock();

			try
			{
				if (!IsChangeFlagged(modifierType))
				{
					m_changes.AddLast(modifierType);
				}
			}
			finally
			{
				m_changeLock.ExitWriteLock();
			}
		}

        /// <summary>
        /// Manually flags multiple modifiers as changed.
        /// </summary>
        /// <param name="modifierTypes"></param>
        public void FlagChanges(IEnumerable<ModifierType> modifierTypes)
        {
            m_changeLock.EnterWriteLock();

            try
            {
                foreach (ModifierType modifierType in modifierTypes)
                {
                    if (!IsChangeFlagged(modifierType))
                    {
                        m_changes.AddLast(modifierType);
                    }
                }
            }
            finally
            {
                m_changeLock.ExitWriteLock();
            }
        }

		/// <summary>
		/// Synchonizes all the pending stat changes with the owner <see cref="Unit" />.
		/// </summary>
		public void SynchronizeChanges()
		{
			if (m_changes.Count == 0)
			{
				return;
			}

			m_changeLock.EnterWriteLock();

			try
			{
				foreach (ModifierType changeType in m_changes)
				{
					ModifierChange.CommitChange(m_owner, changeType, this);
				}

                m_changes.Clear();
			}
			finally
			{
				m_changeLock.ExitWriteLock();
			}
		}
	}
}