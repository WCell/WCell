using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.NPCs
{
	public class VehicleEntry
	{
		/// <summary>
		/// This is *NOT* the EntryId of the NPCEntry
		/// </summary>
		public uint Id;

		/// <summary>
		/// flag, position 1
		/// </summary>
		public VehicleFlags Flags;

		/// <summary>
		/// turn speed, position 2
		/// </summary>
		public float TurnSpeed;

		/// <summary>
		/// pitchspeed, position 3
		/// </summary>
		public float PitchSpeed;

		public float PitchMin;

		public float PitchMax;

		public VehicleSeatEntry[] Seats = new VehicleSeatEntry[8]; //6-13

		public float MouseLookOffsetPitch; //14

		public float CameraFadeDistScalarMin; //15

		public float CameraFadeDistScalarMax; //16

		public float CameraPitchOffset; //17

		public int[] PowerType; //18-20

		public int[] PowerToken; //21-23

		public float FacingLimitRight; //24

		public float FacingLimitLeft; //25

		public float TurnLingering; //26

		public float PitchLingering; //27

		public float MouseLingering; //28

		public float EndOpacity; //29

		public float ArcSpeed; //30

		public float ArcRepeat; //31

		public float ArcWidth; //32

		public float[] ImpactRadius; //33-34


		// custom
		public int SeatCount;

	    public bool IsMinion;
	}

	public class VehicleSeatEntry
	{
		public uint Id; //0

		public VehicleSeatFlags Flags; //1

		public int AttachmentId; //2

		public Vector3 AttachmentOffset; //3-5

		public float EnterPreDelay; //6

		public float EnterSpeed; //7

		public float EnterGravity; //8

		public float EnterMinDuration; //9

		public float EnterMaxDuration; //10

		public float EnterMinArcHeight; //11

		public float EnterMaxArcHeight; //12

		public int EnterAnimStart; //13

		public int EnterAnimLoop; //14

		public int RideAnimStart; //15

		public int RideAnimLoop; //16;

		public int RideUpperAnimStart; //17

		public int RideUpperAnimLoop; //18

		public float ExitPreDelay; //19

		public float ExitSpeed; //20

		public float ExitGravity; //21

		public float ExitMinDuration; //22

		public float ExitMaxDuration; //23

		public float ExitMinArcHeight; //24

		public float ExitMaxArcHeight; //25

		public int ExitAnimStart; //26

		public int ExitAnimLoop; //27

		public int ExitAnimEnd; //28

		public float PassengerYaw; //29

		public float PassengerPitch; //30

		public float PassengerRoll; //31

		public int PassengerAttachmentId; //32

		public int VehicleEnterAnim; //33

		public int VehicleExitAnim; //34

		public int VehicleRideAnimLoop; //35

		public int VehicleEnterAnimBone; //36

		public int VehicleExitAnimBone; //37

		public int VehicleRideAnimLoopBone; //38

		public float VehicleEnterAnimDelay; //39

		public float VehicleExitAnimDelay; //40

		public uint VehicleAbilityDisplay; //41

		public uint EnterUISoundId; //42

		public uint ExitUISoundId; //43

		public int SkinId; //44

		public VehicleSeatFlagsB FlagsB; //45

        // custom

	    public uint PassengerNPCId;
	}
}