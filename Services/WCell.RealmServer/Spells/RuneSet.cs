using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells
{
	/// <summary>
	/// A set of Runes that Death Knights use
	/// </summary>
	public class RuneSet
	{
		public static int RuneCooldownMillis = 10000;

		public readonly RuneType[] ActiveRunes = new RuneType[(int)RuneType.End];
		public readonly int[] Cooldowns;

		public RuneSet(Character owner, int runeSetMask, int[] runeCooldowns)
		{
			Owner = owner;

			UnpackRuneSetMask(runeSetMask);

			if (runeCooldowns == null || runeCooldowns.Length != SpellConstants.MaxRuneCount)
			{
				runeCooldowns = new int[SpellConstants.MaxRuneCount];
			}
			Cooldowns = runeCooldowns;
		}

		public Character Owner
		{
			get;
			internal set;
		}

		#region Convert
		public bool Convert(RuneType from, RuneType to)
		{
			for (var i = 0u; i < SpellConstants.MaxRuneCount; i++)
			{
				if (ActiveRunes[i] == from)
				{
					Convert(i, to);
					return true;
				}
			}
			return false;
		}

		public void ConvertToDefault(uint index)
		{
			Convert(index, SpellConstants.DefaultRuneSet[index]);
		}

		public void Convert(uint index, RuneType to)
		{
			ActiveRunes[index] = to;
			SpellHandler.SendConvertRune(Owner.Client, index, to);
		}
		#endregion

		#region Check & Consume Rune cost
		/// <summary>
		/// Whether there are enough runes in this set to satisfy the given cost requirements
		/// </summary>
		public bool HasEnoughRunes(RuneCostEntry costs)
		{
			for (RuneType type = 0; type < (RuneType) costs.CostPerType.Length; type++)
			{
				var cost = costs.CostPerType[(int) type];
				if (cost > 0)
				{
					for (var i = 0; i < SpellConstants.MaxRuneCount; i++)
					{
						if ((ActiveRunes[i] == type || ActiveRunes[i] == RuneType.Death)
							&& Cooldowns[i] <= 0)
						{
							cost--;
						}
					}
					if (cost > 0)
					{
						return false;
					}
				}
			}
			return true;
		}

		public void ConsumeRunes(RuneCostEntry costs)
		{
			for (RuneType type = 0; type < (RuneType)costs.CostPerType.Length; type++)
			{
				var cost = costs.CostPerType[(int) type];
				if (cost > 0)
				{
					// first look for normal runes
					for (var i = 0u; i < SpellConstants.MaxRuneCount; i++)
					{
						if (ActiveRunes[i] == type)
						{
							if (Cooldowns[i] <= 0)
							{
								StartCooldown(i);		// start cooldown
								cost--;
								if (cost == 0)
								{
									break;
								}
							}
						}
					}

					// then consume death runes
					for (var i = 0u; i < SpellConstants.MaxRuneCount; i++)
					{
						if (ActiveRunes[i] == RuneType.Death)
						{
							if (Cooldowns[i] <= 0)
							{
								ConvertToDefault(i);	// Convert death rune back to normal rune
								StartCooldown(i);		// start cooldown
								cost--;
								if (cost == 0)
								{
									break;
								}
							}
						}
					}
				}
			}
		}
		#endregion

		#region Cooldown
		/// <summary>
		/// TODO: Send update to client, if necessary
		/// </summary>
		public void StartCooldown(uint index)
		{
			Cooldowns[index] = RuneCooldownMillis;
		}

		/// <summary>
		/// TODO: Send update to client, if necessary
		/// </summary>
		public void UnsetCooldown(uint index)
		{
			Cooldowns[index] = 0;
		}

		internal void UpdateCooldown(int dt)
		{
			for (var i = 0u; i < SpellConstants.MaxRuneCount; i++)
			{
				var cd = Cooldowns[i] - dt;
				if (cd > 0)
				{
					Cooldowns[i] = cd;
				}
				else
				{
					UnsetCooldown(i);
				}
			}
		}
		#endregion

		#region Serialize & Deserialize
		public int PackRuneSetMask()
		{
			var setMask = 0;
			for (var i = 0; i < SpellConstants.MaxRuneCount; i++)
			{
				setMask |= ((int)ActiveRunes[i] << (SpellConstants.BitsPerRune * i));
			}
			return setMask;
		}

		public void UnpackRuneSetMask(int runeSetMask)
		{
			for (var i = 0; i < SpellConstants.MaxRuneCount; i++)
			{
				var rune = (RuneType)(runeSetMask);
				if (rune >= RuneType.End)
				{
					ActiveRunes[i] = SpellConstants.DefaultRuneSet[i];
				}
				else
				{
					ActiveRunes[i] = rune;
				}
				runeSetMask >>= SpellConstants.BitsPerRune;
			}
		}
		#endregion
	}
}