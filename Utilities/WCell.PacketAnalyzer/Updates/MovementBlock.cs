using System.IO;
using WCell.Constants;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Util.Graphics;

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

        // levitating
	    public uint MoveFlag2_0x400_Unk;

		// Swimming/Flying
		public float Pitch;

		// Falling stuff
		public float FallFloat1;
		public float FallFloat2;
		public float FallFloat3;
		public float FallFloat4;

		// MoveFlag 0x40000000 Spline
		public float SplineElevation;


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

			if (MovementFlags.HasAnyFlag(MovementFlags.OnTransport) ||
				UpdateFlags.HasAnyFlag(UpdateFlags.StationaryObjectOnTransport))
			{
				writer.WriteLine(indent + "Transporter: " + TransporterId);
				writer.WriteLine(indent + "TransporterPosition: " + TransporterPosition);
				writer.WriteLine(indent + "TransporterTime: " + TransporterTime);
				writer.WriteLine(indent + "TransportSeatPosition: " + TransportSeatPosition);
			}

            if (MovementFlags.HasAnyFlag(MovementFlags.Swimming | MovementFlags.Flying) ||	// don't use HasFlag!
				MovementFlags2.HasFlag(MovementFlags2.AlwaysAllowPitching))
			{
				writer.WriteLine(indent + "Pitch: " + Pitch);
			}

			if (FallTime != 0)
			{
				writer.WriteLine(indent + "FallTime: " + FallTime);
			}

			if (MovementFlags.HasAnyFlag(MovementFlags.Falling))
			{
				writer.WriteLine(indent + "FallFloat1: " + FallFloat1);
				writer.WriteLine(indent + "FallFloat2: " + FallFloat2);
				writer.WriteLine(indent + "FallFloat3: " + FallFloat3);
				writer.WriteLine(indent + "FallFloat4: " + FallFloat4);
			}

            if (MovementFlags.HasFlag(MovementFlags.SplineElevation))
			{
				writer.WriteLine(indent + "SplineElevation: " + SplineElevation);
			}

			if (Speeds.Run != 0)
			{
				writer.WriteLine(indent + "Speeds: " + Speeds);
			}

            if (MovementFlags.HasFlag(MovementFlags.SplineEnabled))
			{
				DumpSpline(indent, writer);
			}

            if (MovementFlags2.HasFlag(MovementFlags2.InterpolateMove))
            {
                writer.WriteLine(indent + "MoveFlag2_10_0x400: " + MoveFlag2_0x400_Unk);
            }

			if (UpdateFlags.HasFlag(UpdateFlags.AttackingTarget))
			{
				writer.WriteLine(indent + "AttackingTarget: " + AttackingTarget);
			}

			if (UpdateFlags.HasFlag(UpdateFlags.Transport))
			{
				writer.WriteLine(indent + "TransportTimeSync: " + TransportTimeSync);
			}

			if (UpdateFlags.HasFlag(UpdateFlags.Vehicle))
			{
				writer.WriteLine(indent + "VehicleId: " + VehicleId);
				writer.WriteLine(indent + "VehicleAimAdjustment: " + VehicleAimAdjustment);
			}

            if (UpdateFlags.HasFlag(UpdateFlags.HasRotation))
			{
				writer.WriteLine(indent + "Flag_0x200_Rotation: " + Flag0x200);
			}

            if (UpdateFlags.HasFlag(UpdateFlags.Flag_0x400))
            {
            }

            if (UpdateFlags.HasFlag(UpdateFlags.Flag_0x800))
            {
            }

            if (UpdateFlags.HasFlag(UpdateFlags.Flag_0x1000))
            {
            }
		}

		private void DumpSpline(string indent, TextWriter writer)
		{
			writer.WriteLine(indent + "SplineFlags: " + SplineFlags);

			if (SplineFlags.HasAnyFlag(SplineFlags.FinalFacePoint | SplineFlags.FinalFaceAngle))
			{
				writer.WriteLine(indent + "SplinePosition: " + SplinePosition);
			}

            if (SplineFlags.HasFlag(SplineFlags.FinalFaceTarget))
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
            if (block.UpdateFlags.HasFlag(UpdateFlags.Living))
			{
				ParseLiving(block);
			}
            else if (block.UpdateFlags.HasFlag(UpdateFlags.StationaryObjectOnTransport))
			{
				ParseStationaryObjectOnTransport(block);
			}
            else if (block.UpdateFlags.HasFlag(UpdateFlags.StationaryObject))
			{
				ParseStationaryObject(block);
			}

            if (block.UpdateFlags.HasFlag(UpdateFlags.AttackingTarget))
			{
				block.AttackingTarget = block.Update.ReadPackedEntityId();
			}

            if (block.UpdateFlags.HasFlag(UpdateFlags.Transport))
			{
				block.TransportTimeSync = block.Update.ReadUInt();
			}

            if (block.UpdateFlags.HasFlag(UpdateFlags.Vehicle))
			{
				block.VehicleId = block.Update.ReadUInt();
				block.VehicleAimAdjustment = block.Update.ReadFloat();
			}

            if(block.UpdateFlags.HasFlag(UpdateFlags.Flag_0x800))
            {
                //short
                //short
                //short
            }
            if (block.UpdateFlags.HasFlag(UpdateFlags.HasRotation))
			{
				block.Flag0x200 = block.Update.ReadUInt64();
			}
            if (block.UpdateFlags.HasFlag(UpdateFlags.Flag_0x1000))
            {
                //byte count
                //for count
                //uint
                //end for
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
            if (block.MovementFlags.HasFlag(MovementFlags.OnTransport))
			{
				block.TransporterId = block.Update.ReadPackedEntityId();
				block.TransporterPosition = block.Update.ReadVector4();
				block.TransporterTime = block.Update.ReadUInt();
				block.TransportSeatPosition = block.Update.ReadByte();

                // Client checks for 0x400
                if (block.MovementFlags2.HasFlag(MovementFlags2.InterpolateMove))
                {
                    block.MoveFlag2_0x400_Unk = block.Update.ReadUInt();
                }
			}

			// Client checks for 0x2200000
            if (block.MovementFlags.HasAnyFlag(MovementFlags.Swimming | MovementFlags.Flying) ||
                block.MovementFlags2.HasFlag(MovementFlags2.AlwaysAllowPitching))
			{
				block.Pitch = block.Update.ReadFloat();
			}

            if (block.MovementFlags2.HasFlag(MovementFlags2.InterpolateTurning))
            {
                block.FallTime = block.Update.ReadUInt();
                // constant, but different when jumping in water and on land?                
                block.FallFloat1 = block.Update.ReadFloat();

                if (block.MovementFlags.HasFlag(MovementFlags.Falling))
                {
                    block.FallFloat2 = block.Update.ReadFloat();
                    block.FallFloat3 = block.Update.ReadFloat();
                    block.FallFloat4 = block.Update.ReadFloat();
                }
            }

			// Client checks for 0x4000000
            if (block.MovementFlags.HasFlag(MovementFlags.SplineElevation))
			{
				block.SplineElevation = block.Update.ReadFloat();
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


            if (block.MovementFlags.HasFlag(MovementFlags.SplineEnabled))
			{
				block.SplineFlags = (SplineFlags)block.Update.ReadUInt();

                if (block.SplineFlags.HasFlag(SplineFlags.FinalFaceAngle))
				{
					block.SplinePosition.W = block.Update.ReadFloat();
				}
                else if (block.SplineFlags.HasFlag(SplineFlags.FinalFaceTarget))
				{
					block.SplineId = block.Update.ReadEntityId();
				}
                else if (block.SplineFlags.HasFlag(SplineFlags.FinalFacePoint))
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