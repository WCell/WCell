using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.Constants.Talents;
using WCell.RealmServer.Database;
using WCell.RealmServer.Items;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.Util.Threading;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Tests.Misc;
using WCell.RealmServer.Tests.Tools;
using WCell.RealmServer.NPCs.Pets;

namespace WCell.RealmServer.Tests.NPCs.Pets
{
    /// <summary>
    /// Summary description for PetTests
    /// </summary>
    [TestClass]
    public class PetTests
    {
        private static TestCharacter Master;
        private static NPC Cat;
        private static NPC Bear;


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [ClassInitialize]
        public void PetTestClassInitialize()
        {
            Setup.EnsureNPCsLoaded();
            Setup.EnsureItemsLoaded();

            EnsureChar(ref Master, Setup.AllianceCharacterPool);
            EnsurePet(ref Cat, Setup.NPCPool);
            Cat.Name = "Cletus";
            Cat.Entry.Family = new CreatureFamily() {Id = CreatureFamilyId.Cat, PetTalentType = PetTalentType.Ferocity};
            EnsurePet(ref Bear, Setup.NPCPool);
            Bear.Name = "BillyRay";
            Bear.Entry.Family = new CreatureFamily() {Id = CreatureFamilyId.Bear, PetTalentType = PetTalentType.Tenacity};
            
            // Add the FeedPet spell to the Master
            Master.Class = ClassId.Hunter;
            var spell = SpellHandler.Get(SpellId.ClassSkillFeedPet);
            Master.Spells.AddSpell(spell);
        }

        [TestInitialize]
        public void PetTestInitialize()
        {
            
        }

        private void EnsureChar(ref TestCharacter chr, CharacterPool pool)
        {
            chr = pool.Create();
            chr.SetMoney(100000);
			chr.GodMode = true;
			chr.EnsureInWorldAndLiving();
        }

        private void EnsurePet(ref NPC npc, NPCPool pool)
        {
            npc = pool.CreateMob();
            npc.EnsureInWorldAndLiving();
        }

        [ClassCleanup()]
		public static void PetTestClassCleanup()
		{
			Master.GodMode = false;
		}

        [TestCleanup]
		public void PetTestCleanup()
		{
            Cat.RejectMaster();
            Cat.PetRecord = null;

            Bear.RejectMaster();
            Bear.PetRecord = null;

            Master.StabledPetRecords.Clear();
            Master.ActivePet = null;
		}

        [TestMethod]
        public void TestMakePet()
        {
            Master.Map.AddMessageAndWait(new Message(() => Master.MakePet(Cat, 0)));
            Assert.IsTrue(Master.ActivePet == Cat);
        }

        [TestMethod]
        public void TestNameQuery()
        {
            TestMakePet();
            SendNameQueryPacket();

            var nameResult = Master.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_PET_NAME_QUERY_RESPONSE);
            var petNumber = nameResult["PetNumber"].UIntValue;
            var name = nameResult["Name"].StringValue;
            var timestamp = nameResult["Timestamp"].UIntValue;

            Assert.IsTrue(petNumber == Cat.PetNumber);
            Assert.IsTrue(name == Cat.Name);
            Assert.IsTrue(timestamp == Cat.PetNameTimestamp);
        }

        [TestMethod]
        public void TestPetRename()
        {
            TestMakePet();
            
            // Valid name
            var newName = "Chester";
            SendRenamePacket(newName);
            Assert.IsTrue(Cat.Name == newName);

            // Test No Name
            newName = "";
            SendRenamePacket(newName);
            var response = Master.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_PET_NAME_INVALID);
            Assert.IsTrue((PetNameInvalidReason)(response["Reason"].UIntValue) == PetNameInvalidReason.NoName);

            // Test Too Short
            newName = "A";
            SendRenamePacket(newName);
            response = Master.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_PET_NAME_INVALID);
            Assert.IsTrue((PetNameInvalidReason)(response["Reason"].UIntValue) == PetNameInvalidReason.TooShort);

            // Test Too Long
            newName = "ThisNameIsTooLong";
            SendRenamePacket(newName);
            response = Master.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_PET_NAME_INVALID);
            Assert.IsTrue((PetNameInvalidReason)(response["Reason"].UIntValue) == PetNameInvalidReason.TooLong);

            // TODO: Test Profanity

            // Test Three Consecutive
            newName = "aXXXb";
            SendRenamePacket(newName);
            response = Master.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_PET_NAME_INVALID);
            Assert.IsTrue((PetNameInvalidReason)(response["Reason"].UIntValue) == PetNameInvalidReason.ThreeConsecutive);

            // Test Spaces
            newName = "a bcd";
            SendRenamePacket(newName);
            response = Master.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_PET_NAME_INVALID);
            Assert.IsTrue((PetNameInvalidReason)(response["Reason"].UIntValue) == PetNameInvalidReason.ConsecutiveSpaces);

            // Test Digits
            newName = "12345";
            SendRenamePacket(newName);
            response = Master.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_PET_NAME_INVALID);
            Assert.IsTrue((PetNameInvalidReason)(response["Reason"].UIntValue) == PetNameInvalidReason.Invalid);

            // Test Apostrphes
            newName = "a'b'c";
            SendRenamePacket(newName);
            response = Master.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_PET_NAME_INVALID);
            Assert.IsTrue((PetNameInvalidReason)(response["Reason"].UIntValue) == PetNameInvalidReason.Invalid);

            // Test First Apostrophe
            newName = "'abcd";
            SendRenamePacket(newName);
            response = Master.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_PET_NAME_INVALID);
            Assert.IsTrue((PetNameInvalidReason)(response["Reason"].UIntValue) == PetNameInvalidReason.Invalid);

            // Test Last Apostrophe
            newName = "abcd'";
            SendRenamePacket(newName);
            response = Master.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_PET_NAME_INVALID);
            Assert.IsTrue((PetNameInvalidReason)(response["Reason"].UIntValue) == PetNameInvalidReason.Invalid);
        }

        [TestMethod]
        public void TestabandonPet()
        {
            TestMakePet();
            
            using(var packet = new RealmPacketOut(RealmServerOpCode.CMSG_PET_ABANDON))
            {
                packet.Write(Cat.EntityId);

                Master.FakeClient.ReceiveCMSG(packet, true, true);
            }

            Assert.IsTrue(Master.ActivePet == null);
        }

        [TestMethod]
        public void TestStablePet()
        {
            TestMakePet();
            Master.StableSlotCount = PetMgr.MaxStableSlots;
            SendStablePetPacket();

            var packet = Master.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_STABLE_RESULT);
            var response = (StableResult)(packet["Result"].ByteValue);
            
            Assert.IsTrue(response == StableResult.StableSuccess);
            Assert.IsTrue(Master.StabledPetRecords.Contains(Cat.PermanentPetRecord));
            Assert.IsTrue(Master.ActivePet == null);
        }

        [TestMethod]
        public void TestUnStablePet()
        {
            TestStablePet();
            SendUnstablePetPacket();

            var packet = Master.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_STABLE_RESULT);
            var response = (StableResult)(packet["Result"].ByteValue);

            Assert.IsTrue(response == StableResult.DeStableSuccess);
            Assert.IsTrue(!Master.StabledPetRecords.Contains(Cat.PermanentPetRecord));
            Assert.IsTrue(Master.ActivePet == Cat);
            Assert.IsTrue((Cat.PetRecord.Flags & PetFlags.Stabled) == 0);
        }

        [TestMethod]
        public void TestSwapStabledPet()
        {
            // Puts the Cat in the stable
            TestStablePet();

            // Makes the Bear the active pet
            Master.Map.AddMessageAndWait(new Message(() => Master.MakePet(Bear)));
            Assert.IsTrue(Master.ActivePet == Bear);

            SendSwapStabledPetsPacket();
            var packet = Master.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_STABLE_RESULT);
            var response = (StableResult)(packet["Result"].ByteValue);

            Assert.IsTrue(response == StableResult.DeStableSuccess);
            Assert.IsTrue(Master.ActivePet == Cat);
            Assert.IsTrue(Master.StabledPetRecords.Contains(Bear.PermanentPetRecord));
            Assert.IsTrue((Cat.PetRecord.Flags & PetFlags.Stabled) == 0);
			Assert.IsTrue((Bear.PetRecord.Flags & PetFlags.Stabled) != 0);
        }

        [TestMethod]
        public void TestBuyStableSlot()
        {
            var slots = Master.StableSlotCount;
            SendBuyStableSlotPacket();

            var packet = Master.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_STABLE_RESULT);
            var response = (StableResult)(packet["Result"].ByteValue);

            Assert.IsTrue(response == StableResult.BuySlotSuccess);
            Assert.IsTrue(Master.StableSlotCount == (slots + 1));
        }
        
        [TestMethod]
        public void TestGetStabledPetList()
        {
            TestStablePet();

            // Makes the Bear the active pet
            Master.Map.AddMessageAndWait(new Message(() => Master.MakePet(Bear, 0)));
            Assert.IsTrue(Master.ActivePet == Bear);

            // Puts the Bear in the stable
            SendStablePetPacket();
            var packet = Master.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_STABLE_RESULT);
            var response = (StableResult)(packet["Result"].ByteValue);
            Assert.IsTrue(response == StableResult.StableSuccess);
            Assert.IsTrue(Master.StabledPetRecords.Contains(Bear.PermanentPetRecord));
            Assert.IsTrue(Master.ActivePet == null);

            // Get the list
            SendGetStabledPetsListPacket();
            packet = Master.FakeClient.DequeueSMSG(RealmServerOpCode.MSG_LIST_STABLED_PETS);
            var numPets = packet["NumPets"].ByteValue;
            var numSlots = packet["NumOwnedSlots"].ByteValue;
            var pet1Number = packet["Pets"][0]["PetNumber"].UIntValue;
            var pet1EntryId = packet["Pets"][0]["PetEntryId"].UIntValue;
            var pet1Level = packet["Pets"][0]["PetLevel"].UIntValue;
            var pet1Name = packet["Pets"][0]["PetName"].StringValue;
            var pet1Slot = packet["Pets"][0]["SlotNum"].ByteValue;
            var pet2Number = packet["Pets"][1]["PetNumber"].UIntValue;
            var pet2EntryId = packet["Pets"][1]["PetEntryId"].UIntValue;
            var pet2Level = packet["Pets"][1]["PetLevel"].UIntValue;
            var pet2Name = packet["Pets"][1]["PetName"].StringValue;
            var pet2Slot = packet["Pets"][1]["SlotNum"].ByteValue;

            Assert.IsTrue(numPets == 2);
			Assert.IsTrue(numSlots == (byte)PetMgr.MaxStableSlots);
            Assert.IsTrue(pet1Number == Cat.PetNumber);
            Assert.IsTrue(pet1Level == Cat.Level);
            Assert.IsTrue(pet1EntryId == Cat.EntryId);
            Assert.IsTrue(pet1Name == Cat.Name);
            Assert.IsTrue(pet1Slot == 3);
            Assert.IsTrue(pet2Number == Bear.PetNumber);
            Assert.IsTrue(pet2Level == Bear.Level);
            Assert.IsTrue(pet2EntryId == Bear.EntryId);
            Assert.IsTrue(pet2Name == Bear.Name);
            Assert.IsTrue(pet2Slot == 4);
        }

        [TestMethod]
        public void TestFeedPet()
        {
            TestMakePet();
            Cat.Power = 100;
            Cat.Level = 10;
            Cat.Entry.FamilyId = CreatureFamilyId.Cat;

            // Add some meat to the Master's inventory
            var amount = 5;
            var invErr = Master.Inventory.TryAdd(ItemId.KodoMeat, ref amount, InventorySlot.BackPack5);
            Assert.IsTrue( invErr == InventoryError.OK);
            var meat = Master.Inventory.GetItemByItemId(ItemId.KodoMeat, false);
            Assert.IsNotNull(meat);

            var spell = SpellHandler.Get(SpellId.ClassSkillFeedPet);
            var preHappiness = Cat.Power;
            Master.Map.AddMessageAndWait(new Message(() => {
                var cast = Master.SpellCast;
                cast.UsedItem = meat;
                var err = cast.Start(spell, false);
                
                Assert.AreEqual(SpellFailedReason.Ok, err);
                Assert.IsTrue(Cat.Auras.Count > 0);
 
                var aura = Cat.Auras[AuraType.PeriodicEnergize];
                var handler = aura.GetHandler(AuraType.PeriodicEnergize);
                Assert.IsNotNull(aura);
                Assert.IsNotNull(handler);

                Assert.IsTrue(handler.EffectValue > 0);
            }));

            var milis = (int)spell.Effects[0].TriggerSpell.Effects[0].Amplitude;
            Cat.CallDelayed(milis, (obj) => Asser.GreaterThan(Cat.Power, preHappiness));
        }

        [TestMethod]
        public void TestFeedPetNoFood()
        {
            TestMakePet();
            
            var spell = SpellHandler.Get(SpellId.ClassSkillFeedPet);
            
            Master.Map.AddMessageAndWait(new Message(() => {
                var cast = Master.SpellCast;
                var err = cast.Start(spell, false);
                
                Assert.AreEqual(SpellFailedReason.ItemNotFound, err);
            }));
        }

        [TestMethod]
        public void TestPetResetTalents()
        {
            TestMakePet();
            Cat.Level = 50;
            Master.Talents.LearnAll();

            SendPetTalentResetPacket();
            Assert.AreEqual(12, Cat.FreeTalentPoints);
        }

        [TestMethod]
        public void TestPetLearnTalent()
        {
            TestPetResetTalents();

            var points = Cat.FreeTalentPoints;

            SendLearnTalentPacket(TalentId.PetTalentsFerocityCobraReflexes, 1);

            Assert.IsTrue(Cat.Talents.HasTalent(TalentId.PetTalentsFerocityCobraReflexes));
            Asser.GreaterThan(points, Cat.FreeTalentPoints);
        }

        [TestMethod]
        public void TestPetLearnTalentWrongTree()
        {
            TestPetResetTalents();

            var points = Cat.FreeTalentPoints;

            SendLearnTalentPacket(TalentId.PetTalentsCunningCobraReflexes, 1);

            Assert.IsFalse(Cat.Talents.HasTalent(TalentId.PetTalentsCunningCobraReflexes));
            Assert.AreEqual(points, Cat.FreeTalentPoints);
        }

        [TestMethod]
        public void TestBeastMasteryEffects()
        {
            Master.Level = 50;
            Cat.Level = 50;
            Master.Talents.LearnAll();
            TestMakePet();
            var basePoints = PetMgr.GetPetTalentPointsByLevel(Cat.Level);
            Cat.FreeTalentPoints = basePoints;
            
            Asser.GreaterThan(Cat.FreeTalentPoints, basePoints);
        }

        private void SendNameQueryPacket()
        {
            using(var packet = new RealmPacketOut(RealmServerOpCode.CMSG_PET_NAME_QUERY))
            {
                packet.Write(Cat.PetNumber);
                packet.Write(Cat.EntityId);

                Master.FakeClient.ReceiveCMSG(packet, true, true);
            }
        }

        private void SendRenamePacket(string newName)
        {
            using(var packet = new RealmPacketOut(RealmServerOpCode.CMSG_PET_RENAME))
            {
                packet.Write(Cat.EntityId);
                packet.Write(newName);

                Master.FakeClient.ReceiveCMSG(packet, true, true);
            }
        }

        private void SendStablePetPacket()
        {
            using(var packet = new RealmPacketOut(RealmServerOpCode.CMSG_STABLE_PET))
            {
                packet.Write(Cat.EntityId);
                
                Master.FakeClient.ReceiveCMSG(packet, true, true);
            }
        }

        private void SendUnstablePetPacket()
        {
            using(var packet = new RealmPacketOut(RealmServerOpCode.CMSG_UNSTABLE_PET))
            {
                packet.Write(Cat.EntityId);
                packet.Write(Cat.PetNumber);

                Master.FakeClient.ReceiveCMSG(packet, true, true);
            }
        }

        private void SendBuyStableSlotPacket()
        {
            using(var packet = new RealmPacketOut(RealmServerOpCode.CMSG_BUY_STABLE_SLOT))
            {
                packet.Write(Cat.EntityId);

                Master.FakeClient.ReceiveCMSG(packet, true, true);
            }
        }

        private void SendSwapStabledPetsPacket()
        {
            using(var packet = new RealmPacketOut(RealmServerOpCode.CMSG_STABLE_SWAP_PET))
            {
                packet.Write(Cat.EntityId);
                packet.Write(Cat.PetNumber);

                Master.FakeClient.ReceiveCMSG(packet, true, true);
            }
        }

        private void SendGetStabledPetsListPacket()
        {
            using(var packet = new RealmPacketOut(RealmServerOpCode.MSG_LIST_STABLED_PETS))
            {
                packet.Write(Cat.EntityId);
                Master.FakeClient.ReceiveCMSG(packet, true, true);
            }
        }

        private void SendPetTalentResetPacket()
        {
            using(var packet = new RealmPacketOut((RealmServerOpCode.CMSG_PET_UNLEARN)))
            {
                packet.Write(Cat.EntityId);
                Master.FakeClient.ReceiveCMSG(packet, true, true);
            }
        }

        private void SendLearnTalentPacket(TalentId id, int rank)
        {
            using(var packet = new RealmPacketOut(RealmServerOpCode.CMSG_PET_LEARN_TALENT))
            {
                packet.Write(Cat.EntityId);
                packet.Write((uint)id);
                packet.Write(rank - 1);

                Master.FakeClient.ReceiveCMSG(packet, true, true);
            }
        }
    } // end class
}