/*************************************************************************
 *
 *   file		: AuraCollection.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-20 06:16:32 +0100 (l? 20 feb 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1257 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.Util;
using WCell.Util.NLog;
using WCell.RealmServer.Chat;

namespace WCell.RealmServer.Spells.Auras
{
	/// <summary>
	/// Represents the collection of all Auras of a Unit
	/// </summary>
	public class AuraCollection : IEnumerable<Aura>
	{
		public const byte InvalidIndex = 0xFF;

		protected Unit m_owner;
		protected Dictionary<AuraIndexId, Aura> m_auras;

		/// <summary>
		/// An immutable array that contains all Auras and is re-created
		/// whenever an Aura is added or removed (lazily prevents threading and update issues -> Find something better).
		/// TODO: Recycle
		/// </summary>
		protected Aura[] m_AuraArray;

		/// <summary>
		/// All non-passive Auras.		
		/// Through items and racial abilities, one Unit can easily have 100 Auras active at a time -		
		/// No need to iterate over all of them when checking for interruption etc.		
		/// </summary>        
		protected readonly Aura[] m_visibleAuras = new Aura[64];

		protected int m_visAuraCount;

		public AuraCollection(Unit owner)
		{
			m_auras = new Dictionary<AuraIndexId, Aura>();
			m_AuraArray = Aura.EmptyArray;

			m_owner = owner;
		}

		public Aura[] VisibleAuras
		{
			get { return m_visibleAuras; }
		}

		public int VisibleAuraCount
		{
			get { return m_visAuraCount; }
		}

		public Aura[] ActiveAuras
		{
			get { return m_AuraArray; }
		}

		public Unit Owner
		{
			get { return m_owner; }
			internal set { m_owner = value; }
		}

		public Character OwnerChar
		{
			get { return m_owner as Character; }
		}

		public int Count
		{
			get { return m_auras.Count; }
		}

		#region Get
		public Aura this[SpellId spellId, bool positive]
		{
			get
			{
				var spell = SpellHandler.Get(spellId);
				if (spell != null)
				{
					return this[spell, positive];
				}
				return null;
			}
		}

		public Aura this[Spell spell, bool positive]
		{
			get
			{
				if (spell.CanApplyMultipleTimes)
				{
					foreach (var aura in m_AuraArray)
					{
						if (aura.Spell == spell)
						{
							return aura;
						}
					}
				}
				else
				{
					Aura aura;
					m_auras.TryGetValue(new AuraIndexId(spell.AuraUID, positive), out aura);
					if (aura != null && aura.Spell.Id == spell.Id)
					{
						return aura;
					}
				}
				return null;
			}
		}

		/// <summary>
		/// Returns the first visible Aura with the given SpellId
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Aura this[SpellId id]
		{
			get
			{
				var spell = SpellHandler.Get(id);
				if (spell != null)
				{
					return this[spell];
				}
				return null;
			}
		}

		public Aura this[Spell spell]
		{
			get
			{
				if (spell.CanApplyMultipleTimes)
				{
					foreach (var aura in m_AuraArray)
					{
						if (aura.Spell == spell)
						{
							return aura;
						}
					}
				}
				else
				{
					Aura aura;
					m_auras.TryGetValue(new AuraIndexId(spell.AuraUID, !spell.HasHarmfulEffects), out aura);
					if (aura != null && aura.Spell.Id == spell.Id)
					{
						return aura;
					}
				}
				return null;
			}
		}

		public Aura this[SpellLineId id, bool positive]
		{
			get
			{
				var line = SpellLines.GetLine(id);
				if (line != null)
				{
					return this[line, positive];
				}
				return null;
			}
		}

		public Aura this[SpellLine line, bool positive]
		{
			get
			{
				Aura aura;
				m_auras.TryGetValue(new AuraIndexId(line.AuraUID, positive), out aura);
				if (aura != null && aura.Spell.Line == line)
				{
					return aura;
				}
				return null;
			}
		}

		/// <summary>
		/// Returns the first visible Aura with the given SpellId
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Aura this[SpellLineId id]
		{
			get
			{
				var line = SpellLines.GetLine(id);
				if (line != null)
				{
					return this[line];
				}
				return null;
			}
		}

		public Aura this[SpellLine line]
		{
			get
			{
				Aura aura;
				if (!m_auras.TryGetValue(new AuraIndexId(line.AuraUID, !line.BaseSpell.HasHarmfulEffects), out aura))
				{
					if (!m_auras.TryGetValue(new AuraIndexId(line.AuraUID, line.BaseSpell.HasHarmfulEffects), out aura))
					{
						return null;
					}
				}
				return aura.Spell.Line == line ? aura : null;
			}
		}

		public Aura this[AuraIndexId auraId]
		{
			get
			{
				Aura aura;
				m_auras.TryGetValue(auraId, out aura);
				return aura;
			}
		}

		/// <summary>
		/// Returns the first visible (not passive) Aura with the given Type (if any).
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		/// <param name="type"></param>
		/// <returns></returns>
		public Aura this[AuraType type]
		{
			get
			{
				foreach (var aura in m_visibleAuras)
				{
					if (aura != null && aura.Spell.HasEffectWith(effect => effect.AuraType == type))
					{
						return aura;
					}
				}
				return null;
			}
		}

		/// <summary>
		/// Returns the first Aura that matches the given Predicate.
		/// Only looks in active Auras.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		public Aura FindFirst(Predicate<Aura> condition)
		{
			//foreach (Aura aura in m_nonPassiveAuras)
			foreach (var aura in m_visibleAuras)
			{
				if (aura != null && condition(aura))
				{
					return aura;
				}
			}
			return null;
		}

		/// <summary>
		/// Iterates over all Auras and returns the n'th visible one
		/// </summary>
		/// <returns>The nth visible Aura or null if there is none</returns>
		public Aura GetAt(uint n)
		{
			if (n < m_visibleAuras.Length)
			{
				return m_visibleAuras[n];
			}
			return null;
		}

		/// <summary>
		/// Get an Aura that is incompatible with the one represented by the given spell
		/// </summary>
		/// <returns>Whether or not another Aura may be applied</returns>
		public Aura GetAura(ObjectReference caster, AuraIndexId id, Spell spell)
		{
			var oldAura = this[id];
			if (oldAura != null)
			{
				return oldAura;
			}
			else
			{
				// no aura found
				// check for per-caster-restrictions
				if (spell.AuraCasterGroup != null)
				{
					var count = 0;
					foreach (var aura in m_AuraArray)
					{
						if (aura.CasterReference.EntityId == caster.EntityId && spell.AuraCasterGroup == aura.Spell.AuraCasterGroup)
						{
							count++;
							if (count >= spell.AuraCasterGroup.MaxCount)
							{
								return aura;
							}
						}
					}
				}
			}
			return null;
		}
		#endregion

		#region Contains
		public bool Contains(AuraIndexId id)
		{
			return this[id] != null;
		}

		public bool Contains(uint auraUID, bool beneficial)
		{
			return this[new AuraIndexId(auraUID, beneficial)] != null;
		}

		public bool Contains(SpellId id)
		{
			return this[id] != null;
		}

		public bool Contains(Spell spell)
		{
			return this[spell] != null;
		}

		public bool ContainsAny(params Spell[] spells)
		{
			foreach (var spell in spells)
			{
				if (this[spell] != null)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns the first visible Aura with the given SpellId
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool Contains(SpellLineId id)
		{
			var line = id.GetLine();
			if (line != null)
			{
				return this[line] != null;
			}
			return false;
		}

		public bool Contains(SpellLine line)
		{
			Aura aura;
			m_auras.TryGetValue(new AuraIndexId(line.AuraUID, !line.BaseSpell.HasHarmfulEffects), out aura);
			return aura != null && aura.Spell.Line == line;
		}
		#endregion

		#region Add
		/// <summary>
		/// Applies the given spell as an Aura (the owner being the caster) to the owner of this AuraCollection.
		/// Also initializes the new Aura.
		/// </summary>
		/// <returns>null if Spell is not an Aura</returns>
		public Aura CreateSelf(SpellId id, bool noTimeout = false)
		{
			return CreateAndStartAura(m_owner.SharedReference, SpellHandler.Get(id), noTimeout);
		}

		/// <summary>
		/// Applies the given spell as an Aura (the owner being the caster) to the owner of this AuraCollection.
		/// Also initializes the new Aura.
		/// </summary>
		/// <returns>null if Spell is not an Aura</returns>
		public Aura CreateSelf(Spell spell, bool noTimeout = false)
		{
			return CreateAndStartAura(m_owner.SharedReference, spell, noTimeout);
		}

		/// <summary>
		/// Applies the given spell as a buff or debuff.
		/// Also initializes the new Aura.
		/// </summary>
		/// <returns>null if Spell is not an Aura</returns>
		public Aura CreateAndStartAura(ObjectReference caster, SpellId spell, bool noTimeout, Item usedItem = null)
		{
			return CreateAndStartAura(caster, SpellHandler.Get(spell), noTimeout, usedItem);
		}

		/// <summary>
		/// Applies the given spell as a buff or debuff.
		/// Also initializes the new Aura.
		/// </summary>
		/// <returns>null if Spell is not an Aura or an already existing version of the Aura that was refreshed</returns>
		public Aura CreateAndStartAura(ObjectReference caster, Spell spell, bool noTimeout, Item usedItem = null)
		{
			try
			{
				var aura = CreateAura(caster, spell, usedItem);
				if (aura != null)
				{
					aura.Start(null, noTimeout);
				}
			}
			catch (Exception ex)
			{
				LogUtil.ErrorException(ex, "Unable to add new Aura \"{0}\" by \"{1}\" to: {2}", spell, caster, m_owner);
			}
			return null;
		}

		/// <summary>
		/// Called when an Aura has been dynamically created (not called, when applying via SpellCast)
		/// </summary>
		private void OnCreated(Aura aura)
		{
			// create AreaAura
			if (aura.Spell.IsAreaAura && Owner.EntityId == aura.CasterReference.EntityId)
			{
				// AreaAura is created at the target location if it is a DynamicObject, else its applied to the caster
				new AreaAura(Owner, aura.Spell);
			}
		}

		/// <summary>
		/// Adds a new Aura with the given information to the Owner. 
		/// Does not initialize the new Aura.
        /// If you use this method, make sure to call <see cref="WCell.RealmServer.Spells.Auras.Aura.Start()"/> on the newly created Aura.
		/// Overrides any existing Aura that matches.
		/// </summary>
		/// <returns>null if Spell is not an Aura</returns>
		internal Aura CreateAura(ObjectReference casterReference, Spell spell, List<AuraEffectHandler> handlers, Item usedItem, bool beneficial)
		{
			// create new Aura
			// Get an index for the aura
			var index = GetFreeIndex(beneficial);

			if (index == InvalidIndex)
			{
				// no more free index fields
				return null;
			}

			var aura = new Aura(this, casterReference, spell, handlers, index, beneficial);
			aura.UsedItem = usedItem;
			AddAura(aura, false);
			return aura;
		}

		/// <summary>
		/// Applies the given spell as a buff or debuff.
		/// Does not necessarily create
		/// Also, initializes new Auras.
		/// </summary>
		/// <returns>null if Spell is not an Aura or an already existing version of the Aura that was refreshed</returns>
		public Aura CreateAura(ObjectReference caster, Spell spell, Item usedItem = null)
		{
			try
			{
				var beneficial = spell.IsBeneficialFor(caster, m_owner);
				var id = spell.GetAuraUID(beneficial);
				var err = SpellFailedReason.Ok;

				// check for existing auras & stacking
				var oldAura = GetAura(caster, id, spell);
				if (oldAura != null)
				{
					if (!PrepareStackOrOverride(caster, oldAura, spell, ref err))
					{
						if (err == SpellFailedReason.Ok)
						{
							// Stacked
							return oldAura;
						}

						if (caster.Object is Character)
						{
							SpellHandler.SendCastFailed((Character)caster.Object, 0, spell, err);
						}
						return null;
					}
				}

				// create new Aura
				var handlers = spell.CreateAuraEffectHandlers(caster, m_owner, beneficial);
				if (handlers != null)
				{
					var aura = CreateAura(caster, spell, handlers, usedItem, beneficial);
					if (aura != null)
					{
						OnCreated(aura);
					}
					return aura;
				}
			}
			catch (Exception ex)
			{
				LogUtil.ErrorException(ex, "Unable to add new Aura \"{0}\" by \"{1}\" to: {2}", spell, caster, m_owner);
			}
			return null;
		}

		/// <summary>
		/// Adds an already created Aura and optionally starts it
		/// </summary>
		public virtual void AddAura(Aura aura, bool start = true)
		{
			var id = aura.Id;
			if (m_auras.ContainsKey(aura.Id))
			{
				LogManager.GetCurrentClassLogger().Warn("Tried to add Aura \"{0}\" by \"{1}\" when it was already added, to: {2}", aura, aura.CasterReference, Owner);
				return;
			}
			m_auras.Add(id, aura);
			if (!aura.Spell.IsPassive)
			{
				m_visibleAuras[aura.Index] = aura;
				++m_visAuraCount;
			}
			InvalidateAurasCopy();

			aura.IsAdded = true;

			if (start)
			{
				aura.Start();
			}
		}
		#endregion

		#region Checks
		/// <summary>
		/// 
		/// </summary>
		public Aura.AuraOverrideStatus GetOverrideStatus(ObjectReference caster, Spell spell)
		{
			var oldAura = this[spell];
			if (oldAura != null)
			{
				return oldAura.GetOverrideStatus(caster, spell);
			}
			return Aura.AuraOverrideStatus.NotPresent;
		}

		/// <summary>
		/// Stack or removes the Aura represented by the given spell, if possible.
		/// Returns true if there is no incompatible Aura or if the Aura could be removed.
		/// <param name="err">Ok, if stacked or no incompatible Aura is blocking a new Aura</param>
		/// </summary>
		internal bool PrepareStackOrOverride(ObjectReference caster, AuraIndexId id, Spell spell, ref SpellFailedReason err, SpellCast triggeringCast = null)
		{
			var oldAura = GetAura(caster, id, spell);
			if (oldAura != null)
			{
				return PrepareStackOrOverride(caster, oldAura, spell, ref err, triggeringCast);
			}
			return true;
		}


		internal bool PrepareStackOrOverride(ObjectReference caster, Aura oldAura, Spell spell, ref SpellFailedReason err, SpellCast triggeringCast = null)
		{
			var status = oldAura.GetOverrideStatus(caster, spell);

			if (status == Aura.AuraOverrideStatus.Replace)
			{
				status = oldAura.TryRemove(true) ? Aura.AuraOverrideStatus.NotPresent : Aura.AuraOverrideStatus.Bounced;
			}

			switch (status)
			{
				case Aura.AuraOverrideStatus.NotPresent:
					return true;							// no obstacles in the way
				case Aura.AuraOverrideStatus.Refresh:
					oldAura.Refresh(caster);
					return false;							// don't need to create a new one
				default:
					err = SpellFailedReason.AuraBounced;	// cannot help it
					return false;
			}
		}
		#endregion

		#region Remove

		/// <summary>
		/// Removes all visible Auras that match the given predicate
		/// </summary>
		/// <param name="predicate"></param>
		public void RemoveWhere(Predicate<Aura> predicate)
		{
			//Aura[] auras = m_nonPassiveAuras.ToArray();
			var auras = m_visibleAuras;
			foreach (var aura in auras)
			{
				if (aura != null && predicate(aura))
				{
					aura.Remove(false);
				}
			}
		}

		/// <summary>
		/// Removes up to the given max amount of visible Auras that match the given predicate
		/// </summary>
		/// <param name="predicate"></param>
		public void RemoveWhere(Predicate<Aura> predicate, int max)
		{
			//Aura[] auras = m_nonPassiveAuras.ToArray();
			var auras = m_visibleAuras;
			var count = 0;
			foreach (var aura in auras)
			{
				if (aura != null && predicate(aura))
				{
					aura.Remove(false);
					if (count >= max)
					{
						break;
					}
				}
			}
		}

		/// <summary>
		/// Removes the first occurance of an Aura that matches the given predicate
		/// </summary>
		/// <param name="predicate"></param>
		public void RemoveFirstVisibleAura(Predicate<Aura> predicate)
		{
			//Aura[] auras = m_nonPassiveAuras.ToArray();
			var auras = m_visibleAuras;
			foreach (var aura in auras)
			{
				if (aura != null && predicate(aura))
				{
					aura.Remove(false);
					break;
				}
			}
		}

		/// <summary>
		/// Removes auras based on their interrupt flag.
		/// </summary>
		/// <param name="interruptFlags">the interrupt flags to remove the auras by</param>
		public void RemoveByFlag(AuraInterruptFlags interruptFlags)
		{
			//Aura[] auras = m_nonPassiveAuras.ToArray();
			foreach (var aura in m_visibleAuras)
			{
				if (aura != null && (aura.Spell.AuraInterruptFlags & interruptFlags) != 0)
				{
					aura.Remove(false);
				}
			}
		}

		public bool Remove(uint auraUID, bool positive)
		{
			var id = new AuraIndexId { AuraUID = auraUID, IsPositive = positive };
			return Remove(id);
		}

		public bool Remove(AuraIndexId auraId)
		{
			Aura aura;
			if (m_auras.TryGetValue(auraId, out aura))
			{
				aura.Remove();
				return true;
			}
			return false;
		}

		public bool Remove(SpellId id)
		{
			var spell = SpellHandler.Get(id);
			if (spell != null)
			{
				return Remove(spell);
			}
			return false;
		}

		/// <summary>
		/// Removes and cancels the first Aura of the given Spell
		/// </summary>
		public bool Remove(Spell spell)
		{
			Aura aura;
			if (spell.HarmType == HarmType.Beneficial || spell.HarmType == HarmType.Neutral)
			{
				aura = this[spell, true];
			}
			else
			{
				aura = null;
			}

			if (aura == null && spell.HasHarmfulEffects)
			{
				aura = this[spell, false];
			}

			if (aura != null && aura.Spell.Id == spell.Id)
			{
				aura.Remove();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Removes the given Aura without cancelling it.
		/// Automatically called by <see cref="Aura.Remove"/>.
		/// </summary>
		internal protected virtual void Remove(Aura aura)
		{
			m_auras.Remove(aura.Id);
			if (aura.Spell.IsProc)
			{
				m_owner.RemoveProcHandler(aura);
			}

			if (!aura.Spell.IsPassive)
			{
				//m_nonPassiveAuras.Remove(aura);
				m_visibleAuras[aura.Index] = null;
				--m_visAuraCount;
			}
			InvalidateAurasCopy();
			OnAuraChange(aura);
		}

		/// <summary>
		/// Removes all Aura effects, when the Owner is about to leave the world (due to logout / deletion).
		/// </summary>
		public void CleanupAuras()
		{
			for (var i = 0; i < m_AuraArray.Length; i++)
			{
				var aura = m_AuraArray[i];
				if (aura != null)
				{
					aura.Cleanup();
				}
			}
		}

		/// <summary>
		/// Removes all auras that are casted by anyone but this unit itself
		/// </summary>
		public void RemoveOthersAuras()
		{
			for (var i = 0; i < m_visibleAuras.Length; i++)
			{
				var aura = m_visibleAuras[i];
				if (aura != null && aura.CasterUnit != m_owner)
				{
					aura.Remove(true);
				}
			}
		}

		/// <summary>
		/// Removes all visible buffs and debuffs
		/// </summary>
		public void ClearVisibleAuras()
		{
			for (var i = 0; i < m_visibleAuras.Length; i++)
			{
				var aura = m_visibleAuras[i];
				if (aura != null)
				{
					aura.Remove(true);
				}
			}
		}

		/// <summary>
		/// Removes all auras, including passive auras -
		/// Don't use unless you understand the consequences.
		/// </summary>
		public void Clear()
		{
			foreach (var aura in m_AuraArray)
			{
				aura.Remove(true);
			}
		}

		/// <summary>
		/// Removes all auras, including passive auras, when owner is deleted.
		/// </summary>
		internal void ClearWithoutCleanup()
		{
			foreach (var aura in m_AuraArray)
			{
				aura.RemoveWithoutCleanup();
			}
		}
		#endregion

		#region Aura Indices
		/// <summary>
		/// TODO: Improve by having a container for recyclable ids
		/// </summary>
		/// <returns></returns>
		public byte GetFreePositiveIndex()
		{
			for (byte i = 0; i < m_visibleAuras.Length - 16; i++)
			{
				if (m_visibleAuras[i] == null)
					return i;
			}
			return InvalidIndex;
		}

		public byte GetFreeNegativeIndex()
		{
			for (byte i = 48; i < m_visibleAuras.Length; i++)
			{
				if (m_visibleAuras[i] == null)
					return i;
			}
			return InvalidIndex;
		}

		public byte GetFreeIndex(bool beneficial)
		{
			return beneficial ? GetFreePositiveIndex() : GetFreeNegativeIndex();
		}
		#endregion

		#region Special Auras
		/// <summary>
		/// Always represents your curent ride (or null when not mounted)
		/// </summary>
		public Aura MountAura
		{
			get;
			internal set;
		}

		/// <summary>
		/// Represents the Aura that makes us a Ghost
		/// </summary>
		public Aura GhostAura
		{
			get;
			internal set;
		}
		#endregion

		#region Changes
		/// <summary>
		/// Create a new Copy of 
		/// </summary>
		private void InvalidateAurasCopy()
		{
			//if (m_auras.Count > m_auras.Count)
			m_AuraArray = m_auras.Values.ToArray();
		}

		/// <summary>
		/// Called when an Aura gets added or removed
		/// </summary>
		/// <param name="aura"></param>
		internal void OnAuraChange(Aura aura)
		{
			if (aura.IsBeneficial && aura.Spell.HasModifierEffects)
			{
				ReApplyAffectedAuras(aura.Spell);
			}
		}

		/// <summary>
		/// Reapplies all passive permanent Auras that are affected by the given Spell
		/// </summary>
		/// <param name="spell"></param>
		public void ReApplyAffectedAuras(Spell spell)
		{
			foreach (var aura in m_AuraArray)
			{
				if (aura.Spell.IsPassive &&
					!aura.HasTimeout &&
					aura.Spell != spell &&
					aura.Spell.IsAffectedBy(spell))
				{
					aura.ReApplyNonPeriodicEffects();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void ReapplyAllAuras()
		{
			foreach (var aura in m_AuraArray)
			{
				aura.ReApplyNonPeriodicEffects();
			}
		}
		#endregion

		#region Spell Modifiers
		/// <summary>
		/// Returns the modified value (modified by certain talent bonusses) of the given type for the given spell (as int)
		/// </summary>
		public virtual int GetModifiedInt(SpellModifierType type, Spell spell, int value)
		{
			if (Owner.Master is Character)
			{
				return ((Character)Owner.Master).PlayerAuras.GetModifiedInt(type, spell, value);
			}
			return value;
		}

		/// <summary>
		/// Returns the given value minus bonuses through certain talents, of the given type for the given spell (as int)
		/// </summary>
		public virtual int GetModifiedIntNegative(SpellModifierType type, Spell spell, int value)
		{
			if (Owner.Master is Character)
			{
				return ((Character)Owner.Master).PlayerAuras.GetModifiedIntNegative(type, spell, value);
			}
			return value;
		}

		/// <summary>
		/// Returns the modified value (modified by certain talents) of the given type for the given spell (as float)
		/// </summary>
		public virtual float GetModifiedFloat(SpellModifierType type, Spell spell, float value)
		{
			if (Owner.Master is Character)
			{
				return ((Character)Owner.Master).PlayerAuras.GetModifiedFloat(type, spell, value);
			}
			return value;
		}

		public virtual void OnCasted(SpellCast cast)
		{
		}
		#endregion

		/// <summary>
		/// Returns whether there are any harmful Auras on the Unit.
		/// Unit cannot leave combat mode while under the influence of harmful Auras.
		/// </summary>
		/// <returns></returns>
		public bool HasHarmfulAura()
		{
			return FindFirst(aura => !aura.IsBeneficial) != null;
		}

		/// <summary>
		/// Returns whether the given spell was modified to be casted 
		/// in any shapeshift form, (even if it usually requires a specific one).
		/// </summary>
		public bool IsShapeshiftRequirementIgnored(Spell spell)
		{
			foreach (var aura in m_AuraArray)
			{
				if (aura.Spell.SpellClassSet != spell.SpellClassSet)
				{
					// must be same class
					continue;
				}
				foreach (var handler in aura.Handlers)
				{
					// check whether there is a IgnoreShapeshiftRequirement aura effect and it's AffectMask matches the spell mask
					if (handler.SpellEffect.AuraType == AuraType.IgnoreShapeshiftRequirement &&
						handler.SpellEffect.MatchesSpell(spell))
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Extra damage to be applied against a bleeding target
		/// </summary>
		public int GetBleedBonusPercent()
		{
			var bonus = 0;
			{
				foreach (var aura in m_AuraArray)
				{
					foreach (var handler in aura.Handlers)
					{
						if (handler.SpellEffect.AuraType == AuraType.IncreaseBleedEffectPct)
						{
							bonus += handler.EffectValue;
						}
					}
				}
			}
			return bonus;
		}

		/// <summary>
		/// Returns the amount of visible Auras that are casted by the given caster
		/// and have the given DispelType.
		/// </summary>
		public int GetVisibleAuraCount(ObjectReference caster, DispelType type)
		{
			var count = 0;
			foreach (var aura in m_visibleAuras)
			{
				if (aura != null && aura.CasterReference == caster && aura.Spell.DispelType == type)
				{
					count++;
				}
			}
			return count;
		}

		#region Persistence
		/// <summary>
		/// Called after Character entered world to load all it's active Auras
		/// </summary>
		internal void InitializeAuras(AuraRecord[] records)
		{
			foreach (var record in records)
			{
				var index = GetFreeIndex(record.IsBeneficial);

				if (index == InvalidIndex)			// no more free index fields
				{
					record.DeleteLater();
					continue;
				}

				var caster = record.GetCasterInfo(m_owner.Map);
				var handlers = record.Spell.CreateAuraEffectHandlers(caster, m_owner, record.IsBeneficial);

				if (handlers == null)				// couldn't create handlers
				{
					record.DeleteLater();
					continue;
				}

				var aura = new Aura(this, caster, record, handlers, index);
				OnCreated(aura);
				AddAura(aura);
			}
		}

		/// <summary>
		/// Save all savable auras
		/// </summary>
		internal void SaveAurasNow()
		{
			foreach (var aura in m_visibleAuras)
			{
				if (aura != null && aura.CanBeSaved && (!aura.HasTimeout || aura.TimeLeft > 5000))
				{
					aura.SaveNow();
				}
			}
		}
		#endregion

		#region Utilities

		/// <summary>
		/// Dumps all currently applied auras to the given chr
		/// </summary>
		/// <param name="receiver"></param>
		/// <param name="includePassive">Whether to also include invisible effects (eg through items etc)</param>
		public void DumpTo(IChatTarget receiver, bool includePassive)
		{
			if (m_auras.Count > 0)
			{
				receiver.SendMessage("{0}'s Auras:", m_owner.Name);
				foreach (var aura in m_auras.Values)
				{
					if (includePassive || !aura.Spell.IsPassive)
					{
						receiver.SendMessage("	{0}{1}", aura.Spell, aura.HasTimeout ? " [" + TimeSpan.FromMilliseconds(aura.TimeLeft).Format() + "]" : "");
					}
				}
			}
			else
			{
				receiver.SendMessage("{0} has no active Auras.", m_owner.Name);
			}
		}
		#endregion

		#region Enumerators
		/// <summary>
		/// We need a second method because yield return and return statements cannot
		/// co-exist in one method.
		/// </summary>
		/// <returns></returns>
		IEnumerator<Aura> _GetEnumerator()
		{
			for (var i = 0; i < m_AuraArray.Length; i++)
			{
				yield return m_AuraArray[i];
			}
		}

		public IEnumerator<Aura> GetEnumerator()
		{
			if (m_auras.Count == 0)
			{
				return Aura.EmptyEnumerator;
			}
			return _GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		/// <summary>
		/// Checks whether the given Aura may be activated (see PlayerAuraCollection)
		/// </summary>
		internal protected virtual bool MayActivate(Aura aura)
		{
			return true;
		}

		/// <summary>
		/// Checks whether the given Handler may be activated (see PlayerAuraCollection)
		/// </summary>
		internal protected virtual bool MayActivate(AuraEffectHandler handler)
		{
			return true;
		}
	}
}