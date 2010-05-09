using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.RealmServer.Database;
using WCell.Core;
using WCell.RealmServer.RacesClassesClasses;
using WCell.Core.Network;
using WCell.Core.Initialization;
using WCell.PacketAnalysis;
using WCell.RealmServer.World;
using Microsoft.Xna.Framework;
using WCell.RealmServer.Spells;

namespace WCell.Tools.Domi.Connected
{
	public static class Setup
	{
		public static void Start()
		{
			Account acc = new Account("abc");
			//acc.InitializeAccount

			var rec = CharacterRecord.GetRecordByName("Abc");
			if (rec == null)
			{
				rec = CharacterRecord.CreateNewCharacterRecord(acc, "Abc");

				rec.Gender = (GenderType)1;
				rec.Skin = (byte)1;
				rec.Face = (byte)1;
				rec.HairStyle = (byte)1;
				rec.HairColor = (byte)1;
				rec.FacialHair = (byte)1;
				rec.Outfit = (byte)1;


				var archetype = RaceClassMgr.GetArchetype(ClassType.Warlock, RaceType.Human);

				var race = archetype.Race;

				rec.Class = archetype.Class.ClassID;
				rec.Race = race.Type;
				rec.Level = 1;
				rec.PositionX = race.StartPosition.X;
				rec.PositionY = race.StartPosition.Y;
				rec.PositionZ = race.StartPosition.Z;
				rec.Orientation = race.StartPosition.W;
				rec.CurrentMap = race.StartMap;
				rec.CurrentZone = race.StartZone;
				rec.TotalPlayTime = 0;
				rec.LevelPlayTime = 0;
				rec.TutorialFlags = new byte[32];

				uint h = 4000000;
				rec.Health = (int)h;

				if (race.Type == RaceType.BloodElf)
				{
					rec.DisplayId = race.ModelOffset - (uint)rec.Gender;
				}
				else
				{
					rec.DisplayId = race.ModelOffset + (uint)rec.Gender;
				}
				rec.SaveAndFlush();
			}

			var client = new FakeRealmClient(acc);
			var chr = new Character(acc, rec, client);

			var packet = new RealmPacketOut(RealmServerOpCode.CMSG_CAST_SPELL);
			packet.WriteUInt(3662);
			client.Receive(packet);


			packet = new RealmPacketOut(RealmServerOpCode.CMSG_CANCEL_CAST);
			packet.WriteUInt(425);
			client.Receive(packet);


			packet = new RealmPacketOut(RealmServerOpCode.CMSG_MESSAGECHAT);
			packet.WriteUInt((uint)ChatMsgType.Officer);
			packet.WriteUInt((uint)ChatLanguage.Darnassian);
			client.Receive(packet);

			var region = WorldMgr.GetRegion(MapId.Outland);
			if (region != null)
			{
				var pos = new Vector3();
				region.AddObjectInstantly(chr, ref pos);
			}
		}
	}
}
