/*************************************************************************
 *
 *   file		: Video.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 07:58:12 +0100 (lø, 07 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Video Effect.
	/// Spells with this effect might be used to trigger client-side video sequences!
	/// 
	/// eg.:
	/// 
	/// every Flight Path
	/// Filming (Id: 28129)
	/// Stormcrow Amulet (Id: 31606)
	/// Elekk Taxi (Id: 31788)
	/// Attack Run 1 (Id: 32059) - Attack Run 4
	/// Nethrandamus Flight (Id: 32551)
	/// Gateways Murket and Shaadraz
	/// Aerial Assault Flight (Horde)
	/// Aerial Assault Flight (Heavy Bomb)
	/// ....
	/// 
	/// </summary>
	public class VideoEffectHandler : SpellEffectHandler
	{
		public VideoEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			 // Client figures videos out on his own
		}

		public override bool HasOwnTargets
		{
			get
			{
				return false;
			}
		}
	}
}