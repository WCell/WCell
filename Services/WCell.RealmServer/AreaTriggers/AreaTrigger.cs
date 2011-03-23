using System;
using System.IO;
using WCell.Constants;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Quests;
using WCell.Constants.AreaTriggers;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Graphics;
using WCell.Util.Logging;

namespace WCell.RealmServer.AreaTriggers
{
	/// <summary>
	/// AreaTriggers are invisible areas ingame that are always known by the client.
	/// An AreaTrigger is triggered when a Character steps on it.
	/// </summary>
	public partial class AreaTrigger
	{
		public readonly uint Id;
		public readonly AreaTriggerId ATId;

		public readonly MapId MapId;
		public Vector3 Position;
		public readonly float Radius;
        public readonly float BoxLength;
        public readonly float BoxWidth;
        public readonly float BoxHeight;
        public readonly float BoxYaw;

		const float tollerance = 55.0f;
		[NotPersistent] public readonly float MaxDistSq;
        
		[NotPersistent]
		public ATTemplate Template;

		public AreaTrigger(uint id, MapId mapId, float x, float y, float z, float radius, float boxLength, float boxWidth, float boxHeight, float boxYaw)
		{
			Id = id;
			ATId = (AreaTriggerId)Id;
			MapId = mapId;
			Position.X = x;
			Position.Y = y;
			Position.Z = z;
			Radius = radius;
            BoxLength = boxLength;
            BoxWidth = boxWidth;
            BoxHeight = boxHeight;
            BoxYaw = boxYaw;

			MaxDistSq = (Radius + tollerance) * (Radius + tollerance);
		}

		/// <summary>
		/// Returns whether the given object is within the bounds of this AreaTrigger.
		/// </summary>
		/// <param name="chr"></param>
		/// <returns></returns>
		public bool IsInArea(Character chr)
		{
            if (chr.Region.Id != MapId)
            {
                return false;
            }

            if (Radius > 0) // Sphere
            {
                var distSq = chr.GetDistanceSq(Position);

                if (distSq > MaxDistSq)
                {
                	LogManager.GetCurrentClassLogger().Warn("Character {0} tried to trigger {1} while being {2} yards away.",
						chr, this, Math.Sqrt(distSq));
                    return false;
                }
            }
            else // Box
            {
                // 2PI = 360 degrees. Keep in mind that in-game orientation is counter-clockwise.
                var rotation = 2 * MathUtil.PI - BoxYaw;
                var sinval = Math.Sin(rotation);
                var cosval = Math.Cos(rotation);

                var playerBoxDistX = chr.Position.X - Position.X;
                var playerBoxDistY = chr.Position.Y - Position.Y;

                var rotPlayerX = Position.X + playerBoxDistX * cosval - playerBoxDistY * sinval;
                var rotPlayerY = Position.Y + playerBoxDistY * cosval + playerBoxDistX * sinval;

                // Box edges are parallel to coordinate axis, so we can treat every dimension independently.
                var dx = rotPlayerX - Position.X;
                var dy = rotPlayerY - Position.Y;
                var dz = chr.Position.Z - Position.Z;

				if ((Math.Abs(dx) > BoxLength / 2 + tollerance) ||
					(Math.Abs(dy) > BoxWidth / 2 + tollerance) ||
					(Math.Abs(dz) > BoxHeight / 2 + tollerance))
                {
                    return false;
                }
            }

		    return true;
		}

		/// <summary>
		/// Does general checks, for whether the given Character may trigger this and sends
		/// an error response if not.
		/// </summary>
		/// <param name="chr"></param>
		/// <returns></returns>
		public bool CheckTrigger(Character chr)
		{
			if (!IsInArea(chr))
			{
				return false;
			}
            if (chr.IsOnTaxi)
            {
                return false;
            }
		    if (Template != null && chr.Level < Template.RequiredLevel)
			{
				Handlers.AreaTriggerHandler.SendAreaTriggerMessage(chr.Client, "You need at least level " + Template.RequiredLevel + ".");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Triggers this trigger
		/// </summary>
		/// <remarks>Requires region context.</remarks>
		public bool Trigger(Character chr)
		{
			if (CheckTrigger(chr))
			{
				NotifyTriggered(this, chr);
				if (Template != null)
				{
					return Template.Handler(chr, this);
				}
				return true;
			}
			return false;
		}

		#region Events
		internal void NotifyTriggered(AreaTrigger at, Character chr)
		{
			var evt = Triggered;
			if (evt != null)
			{
				evt(at, chr);
			}
		}
		#endregion

		public override string ToString()
		{
            if (Template != null)
            {
                return Template.Name + " (Id: " + Id + " [" + ATId +"])";
            }
            return ATId + " (Id: " + Id + ")";
		}

		public void Write(IndentTextWriter writer)
		{
			writer.WriteLine(this);
			writer.IndentLevel++;
			writer.WriteLine("MapId: " + MapId);
			writer.WriteLine("Position: " + Position);
			writer.WriteLineNotDefault(Radius, "Radius: " + Radius);
			writer.WriteLineNotDefault(BoxLength + BoxWidth + BoxHeight + BoxYaw, 
				"Box Length: " + BoxLength +
				", Width: " + BoxWidth +
				", Height: " + BoxHeight + 
				", Yaw: " + BoxYaw);
			if (Template != null)
			{
				Template.Write(writer);	
			}
			writer.IndentLevel--;
		}
	}
}