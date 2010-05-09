using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core;
using WCell.Constants;
using System.IO;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Constants.Updates;

namespace WCell.PacketAnalysis.Updates
{

	public class MovementBlock
	{
		public readonly UpdateBlock Update;
		public readonly UpdateFlags UpdateFlags;
		public readonly ObjectTypeId ObjectTypeId;

		// living
		public MovementFlags MovementFlags;
		public MovementFlags2 MovementFlags2;
		public uint FallTime;
		public uint Timestamp;

		// position
		public Vector4 Position;

		// transporter
		public EntityId TransporterId;
		public Vector4 TransporterPosition;
		public uint TransporterTime;
		public byte TransportSeatPosition;

		// Swimming/Flying
		public float Pitch;

		// Falling stuff
		public float FallFloat1;
		public float FallFloat2;
		public float FallFloat3;
		public float FallFloat4;

		// MoveFlag 0x40000000 Spline
		public float Spline0x4000000;


		// speeds
		public Speeds Speeds;

		// spline
		public SplineFlags SplineFlags;
		public Vector4 SplinePosition;
		public EntityId SplineId;
		public uint SplineMsTimeUnk;
		public uint SplineUnkInt1;
		public uint SplineUnkInt2;
		public uint SplineCount;
		public Vector3[] SplineNodes;
		public byte SplineUnkByte;
		public Vector3 SplineFinalNode;
		public Vector3 SplineVectorUnk;
		public uint SplineUnkInt3;

		// has EntityId
		public EntityId EntityId;

		public uint Flag0x8;
		public uint Flag0x10;
		public float Flag0x10F;
		public ulong Flag0x200;

		public EntityId AttackingTarget;

		public uint TransportTimeSync;

		public uint VehicleId;
		public float VehicleAimAdjustment;

		public MovementBlock(UpdateBlock update)
		{
			Update = update;
			UpdateFlags = (UpdateFlags)update.ReadUShort();
			ObjectTypeId = update.ObjectType;

			this.Parse();
		}

		public void Dump(string indent, TextWriter writer)
		{
			writer.WriteLine(indent + "UpdateFlags: " + UpdateFlags);
			if (EntityId != EntityId.Zero)
			{
				writer.WriteLine(indent + "EntityId: " + EntityId);
			}
			if (MovementFlags != MovementFlags.None)
			{
				writer.WriteLine(indent + "MovementFlags: " + MovementFlags);
			}
			if (MovementFlags2 != MovementFlags2.None)
			{
				writer.WriteLine(indent + "MovementFlags2: " + MovementFlags2);
			}

			writer.WriteLine(indent + "Timestamp: " + Timestamp);
			writer.WriteLine(indent + "Position: " + Position);

			if (MovementFlags.Has(MovementFlags.OnTransport) ||
				UpdateFlags.Has(UpdateFlags.StationaryObjectOnTransport))
			{
				writer.WriteLine(indent + "Transporter: " + TransporterId);
				writer.WriteLine(indent + "TransporterPosition: " + TransporterPosition);
				writer.WriteLine(indent + "TransporterTime: " + TransporterTime);
				writer.WriteLine(indent + "TransportSeatPosition: " + TransportSeatPosition);
			}

			if (MovementFlags.Has(MovementFlags.Swimming | MovementFlags.Flying) ||
				MovementFlags2.Has(MovementFlags2.AlwaysAllowPitching))
			{
				writer.WriteLine(indent + "Pitch: " + Pitch);
			}

			if (FallTime != 0)
			{
				writer.WriteLine(indent + "FallTime: " + FallTime);
			}

			if (MovementFlags.Has(MovementFlags.Falling))
			{
				writer.WriteLine(indent + "FallFloat1: " + FallFloat1);
				writer.WriteLine(indent + "FallFloat2: " + FallFloat2);
				writer.WriteLine(indent + "FallFloat3: " + FallFloat3);
				writer.WriteLine(indent + "FallFloat4: " + FallFloat4);
			}

			if (MovementFlags.Has(MovementFlags.Spline))
			{
				writer.WriteLine(indent + "Spline0x4000000: " + Spline0x4000000);
			}

			if (Speeds.Run != 0)
			{
				writer.WriteLine(indent + "Speeds: " + Speeds);
			}

			if (MovementFlags.Has(MovementFlags.SplinePath))
			{
				DumpSpline(indent, writer);
			}

			if ((UpdateFlags & UpdateFlags.Flag_0x8) != 0)
			{
				writer.WriteLine(indent + "Flag_0x8: " + Flag0x8);
			}

			if ((UpdateFlags & UpdateFlags.Flag_0x10) != 0)
			{
				writer.WriteLine(indent + "Flag0x10: " + Flag0x10);
				writer.WriteLine(indent + "Flag0x10F: " + Flag0x10F);
			}

			if ((UpdateFlags & UpdateFlags.AttackingTarget) != 0)
			{
				writer.WriteLine(indent + "AttackingTarget: " + AttackingTarget);
			}

			if ((UpdateFlags & UpdateFlags.Transport) != 0)
			{
				writer.WriteLine(indent + "TransportTimeSync: " + TransportTimeSync);
			}

			if ((UpdateFlags & UpdateFlags.Vehicle) != 0)
			{
				writer.WriteLine(indent + "VehicleId: " + VehicleId);
				writer.WriteLine(indent + "VehicleAimAdjustment: " + VehicleAimAdjustment);
			}

			if (UpdateFlags.Has(UpdateFlags.HasRotation))
			{
				writer.WriteLine(indent + "Flag_0x200_Rotation: " + Flag0x200);
			}
		}

		private void DumpSpline(string indent, TextWriter writer)
		{
			writer.WriteLine(indent + "SplineFlags: " + SplineFlags);

			if (SplineFlags.Has(SplineFlags.XYZ | SplineFlags.Orientation))
			{
				writer.WriteLine(indent + "SplinePosition: " + SplinePosition);
			}

			if (SplineFlags.Has(SplineFlags.GUID))
			{
				writer.WriteLine(indent + "SplineGuid: " + SplineId);
			}

			writer.WriteLine(indent + "SplineMsTimeUnk: " + SplineMsTimeUnk);
			writer.WriteLine(indent + "SplineUnkInt1: " + SplineUnkInt1);
			writer.WriteLine(indent + "SplineUnkInt2: " + SplineUnkInt2);

			writer.WriteLine(indent + "SplineVectorUnk (Unit vector?): " + SplineVectorUnk);
			writer.WriteLine(indent + "SplineUnkInt3: " + SplineUnkInt3);

			writer.WriteLine(indent + "Path Count: " + SplineCount);
			for (int i = 0; i < SplineCount; i++)
			{
				writer.WriteLine(indent + "\tIntermediate Path #{0}: {1}", i, SplineNodes[i]);
			}
			writer.WriteLine(indent + "SplineUnkByte: " + SplineUnkByte);
			writer.WriteLine(indent + "SplineFinalNode: " + SplineFinalNode);
		}

		public override string ToString()
		{
			return EntityId.ToString();
		}
	}

	public struct Speeds
	{
		public float Walk;
		public float Run;
		public float RunBack;
		public float Swim;
		public float SwimBack;
		public float Fly;
		public float FlyBack;
		public float Turn;
		public float Pitch;

		public override string ToString()
		{
			return "Walk: " + Walk +
				", Run: " + Run +
				", RunBack: " + RunBack +
				", Swim: " + Swim +
				", SwimBack: " + SwimBack +
				", Fly: " + Fly +
				", FlyBack: " + FlyBack +
				", Turn: " + Turn +
				", Pitch: " + Pitch;
		}
	}

	internal static class MovementBlockHelper
	{
		public static void Parse(this MovementBlock block)
		{
			// a world object will always be one of these.
			if (block.UpdateFlags.Has(UpdateFlags.Living))
			{
				ParseLiving(block);
			}
			else if (block.UpdateFlags.Has(UpdateFlags.StationaryObjectOnTransport))
			{
				ParseStationaryObjectOnTransport(block);
			}
			else if (block.UpdateFlags.Has(UpdateFlags.StationaryObject))
			{
				ParseStationaryObject(block);
			}

			if (block.UpdateFlags.Has(UpdateFlags.Flag_0x8))
			{
				block.Flag0x8 = block.Update.ReadUInt();
			}

			if (block.UpdateFlags.Has(UpdateFlags.Flag_0x10))
			{
				block.Flag0x10 = block.Update.ReadUInt();
				block.Update.packet.index -= 4;
				block.Flag0x10F = block.Update.ReadFloat();
			}

			if (block.UpdateFlags.Has(UpdateFlags.AttackingTarget))
			{
				block.AttackingTarget = block.Update.ReadPackedEntityId();
			}

			if (block.UpdateFlags.Has(UpdateFlags.Transport))
			{
				block.TransportTimeSync = block.Update.ReadUInt();
			}

			if (block.UpdateFlags.Has(UpdateFlags.Vehicle))
			{
				block.VehicleId = block.Update.ReadUInt();
				block.VehicleAimAdjustment = block.Update.ReadFloat();
			}
			if (block.UpdateFlags.Has(UpdateFlags.HasRotation))
			{
				block.Flag0x200 = block.Update.ReadUInt64();
			}
		}

		public static void ParseLiving(this MovementBlock block)
		{
			block.MovementFlags = (MovementFlags)block.Update.ReadUInt();
			block.MovementFlags2 = (MovementFlags2)block.Update.ReadUShort();
			block.Timestamp = block.Update.ReadUInt();

			block.ParseStationaryObject();

			// Console.WriteLine("\tParsing Movement for {0}", block.UpdateFlags);

			// Client checks for 0x200
			if (block.MovementFlags.Has(MovementFlags.OnTransport))
			{
				block.TransporterId = block.Update.ReadPackedEntityId();
				block.TransporterPosition = block.Update.ReadVector4();
				block.TransporterTime = block.Update.ReadUInt();
				block.TransportSeatPosition = block.Update.ReadByte();
			}

			// Client checks for 0x2200000
			if (block.MovementFlags.Has(MovementFlags.Swimming | MovementFlags.Flying) ||
				block.MovementFlags2.Has(MovementFlags2.AlwaysAllowPitching))
			{
				block.Pitch = block.Update.ReadFloat();
			}

			block.FallTime = block.Update.ReadUInt();

			// Client checks for 0x1000
			if (block.MovementFlags.Has(MovementFlags.Falling))
			{
				// no idea
				block.FallFloat1 = block.Update.ReadFloat();
				block.FallFloat2 = block.Update.ReadFloat();
				block.FallFloat3 = block.Update.ReadFloat();
				block.FallFloat4 = block.Update.ReadFloat();
			}

			// Client checks for 0x4000000
			if (block.MovementFlags.Has(MovementFlags.Spline))
			{
				block.Spline0x4000000 = block.Update.ReadFloat();
			}

			// read speeds
			block.Speeds = new Speeds
			{
				Walk = block.Update.ReadFloat(),
				Run = block.Update.ReadFloat(),
				RunBack = block.Update.ReadFloat(),
				Swim = block.Update.ReadFloat(),
				SwimBack = block.Update.ReadFloat(),
				Fly = block.Update.ReadFloat(),
				FlyBack = block.Update.ReadFloat(),
				Turn = block.Update.ReadFloat(),
				Pitch = block.Update.ReadFloat()
			};


			if (block.MovementFlags.Has(MovementFlags.SplinePath))
			{
				block.SplineFlags = (SplineFlags)block.Update.ReadUInt();

				if (block.SplineFlags.Has(SplineFlags.Orientation))
				{
					block.SplinePosition.W = block.Update.ReadFloat();
				}
				else if (block.SplineFlags.Has(SplineFlags.GUID))
				{
					block.SplineId = block.Update.ReadEntityId();
				}
				else if (block.SplineFlags.Has(SplineFlags.XYZ))
				{
					block.SplinePosition = block.Update.ReadVector4NoO();
				}

				block.SplineMsTimeUnk = block.Update.ReadUInt();
				block.SplineUnkInt1 = block.Update.ReadUInt();
				block.SplineUnkInt2 = block.Update.ReadUInt();

				block.SplineVectorUnk = block.Update.ReadVector3();
				block.SplineUnkInt3 = block.Update.ReadUInt();

				block.SplineCount = block.Update.ReadUInt();
				if (block.SplineCount > 0)
				{
					block.SplineNodes = new Vector3[block.SplineCount];
					for (int i = 0; i < block.SplineCount; i++)
					{
						block.SplineNodes[i] = block.Update.ReadVector3();
					}
				}
				block.SplineUnkByte = block.Update.ReadByte();

				block.SplineFinalNode = block.Update.ReadVector3();
			}
		}

		public static void ParseStationaryObject(this MovementBlock block)
		{
			block.Position = block.Update.ReadVector4();
		}

		public static void ParseStationaryObjectOnTransport(this MovementBlock block)
		{
			block.TransporterId = block.Update.ReadPackedEntityId();
			block.Position = block.Update.ReadVector4NoO();
			block.TransporterPosition = block.Update.ReadVector4NoO();
			block.Position.W = block.Update.ReadFloat();
			block.TransporterPosition.W = block.Update.ReadFloat();
		}
	}
}
