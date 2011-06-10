using System;

///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 30/04/2011
///

namespace WCell.Constants.Updates
{
	public static class UpdateFields 
	{
		public static readonly 		UpdateField[][] AllFields = new UpdateField[][] {
			#region Object
			new UpdateField[]{
				// ObjectFields.GUID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Object,
					Name = "GUID",
					Offset = 0,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// ObjectFields.TYPE
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Object,
					Name = "TYPE",
					Offset = 2,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ObjectFields.ENTRY
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Object,
					Name = "ENTRY",
					Offset = 3,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ObjectFields.SCALE_X
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Object,
					Name = "SCALE_X",
					Offset = 4,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// ObjectFields.DATA
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Object,
					Name = "DATA",
					Offset = 5,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// ObjectFields.PADDING
				new UpdateField {
					Flags = UpdateFieldFlags.None,
					Group = ObjectTypeId.Object,
					Name = "PADDING",
					Offset = 7,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
			},
			#endregion

			#region Item
			new UpdateField[]{
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// ItemFields.OWNER
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "OWNER",
					Offset = 8,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// ItemFields.CONTAINED
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "CONTAINED",
					Offset = 10,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// ItemFields.CREATOR
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "CREATOR",
					Offset = 12,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// ItemFields.GIFTCREATOR
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "GIFTCREATOR",
					Offset = 14,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// ItemFields.STACK_COUNT
				new UpdateField {
					Flags = UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.Flag_0x8_Unused,
					Group = ObjectTypeId.Item,
					Name = "STACK_COUNT",
					Offset = 16,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.DURATION
				new UpdateField {
					Flags = UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.Flag_0x8_Unused,
					Group = ObjectTypeId.Item,
					Name = "DURATION",
					Offset = 17,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.SPELL_CHARGES
				new UpdateField {
					Flags = UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.Flag_0x8_Unused,
					Group = ObjectTypeId.Item,
					Name = "SPELL_CHARGES",
					Offset = 18,
					Size = 5,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				// ItemFields.FLAGS
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "FLAGS",
					Offset = 23,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.ENCHANTMENT_1_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_1_1",
					Offset = 24,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_1_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_1_3",
					Offset = 26,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_2_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_2_1",
					Offset = 27,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_2_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_2_3",
					Offset = 29,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_3_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_3_1",
					Offset = 30,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_3_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_3_3",
					Offset = 32,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_4_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_4_1",
					Offset = 33,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_4_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_4_3",
					Offset = 35,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_5_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_5_1",
					Offset = 36,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_5_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_5_3",
					Offset = 38,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_6_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_6_1",
					Offset = 39,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_6_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_6_3",
					Offset = 41,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_7_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_7_1",
					Offset = 42,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_7_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_7_3",
					Offset = 44,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_8_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_8_1",
					Offset = 45,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_8_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_8_3",
					Offset = 47,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_9_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_9_1",
					Offset = 48,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_9_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_9_3",
					Offset = 50,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_10_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_10_1",
					Offset = 51,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_10_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_10_3",
					Offset = 53,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_11_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_11_1",
					Offset = 54,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_11_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_11_3",
					Offset = 56,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_12_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_12_1",
					Offset = 57,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_12_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_12_3",
					Offset = 59,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_13_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_13_1",
					Offset = 60,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_13_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_13_3",
					Offset = 62,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_14_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_14_1",
					Offset = 63,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_14_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_14_3",
					Offset = 65,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.PROPERTY_SEED
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "PROPERTY_SEED",
					Offset = 66,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.RANDOM_PROPERTIES_ID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "RANDOM_PROPERTIES_ID",
					Offset = 67,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.DURABILITY
				new UpdateField {
					Flags = UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.Flag_0x8_Unused,
					Group = ObjectTypeId.Item,
					Name = "DURABILITY",
					Offset = 68,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.MAXDURABILITY
				new UpdateField {
					Flags = UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.Flag_0x8_Unused,
					Group = ObjectTypeId.Item,
					Name = "MAXDURABILITY",
					Offset = 69,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.CREATE_PLAYED_TIME
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "CREATE_PLAYED_TIME",
					Offset = 70,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.PAD
				new UpdateField {
					Flags = UpdateFieldFlags.None,
					Group = ObjectTypeId.Item,
					Name = "PAD",
					Offset = 71,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
			},
			#endregion

			#region Container
			new UpdateField[]{
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// ContainerFields.NUM_SLOTS
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Container,
					Name = "NUM_SLOTS",
					Offset = 72,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ContainerFields.ALIGN_PAD
				new UpdateField {
					Flags = UpdateFieldFlags.None,
					Group = ObjectTypeId.Container,
					Name = "ALIGN_PAD",
					Offset = 73,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// ContainerFields.SLOT_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Container,
					Name = "SLOT_1",
					Offset = 74,
					Size = 72,
					Type = UpdateFieldType.Guid
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
			},
			#endregion

			#region Unit
			new UpdateField[]{
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// UnitFields.CHARM
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "CHARM",
					Offset = 8,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.SUMMON
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "SUMMON",
					Offset = 10,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.CRITTER
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Unit,
					Name = "CRITTER",
					Offset = 12,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.CHARMEDBY
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "CHARMEDBY",
					Offset = 14,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.SUMMONEDBY
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "SUMMONEDBY",
					Offset = 16,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.CREATEDBY
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "CREATEDBY",
					Offset = 18,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.TARGET
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "TARGET",
					Offset = 20,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.CHANNEL_OBJECT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "CHANNEL_OBJECT",
					Offset = 22,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.CHANNEL_SPELL
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "CHANNEL_SPELL",
					Offset = 24,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.BYTES_0
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "BYTES_0",
					Offset = 25,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// UnitFields.HEALTH
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "HEALTH",
					Offset = 26,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER1",
					Offset = 27,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER2
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER2",
					Offset = 28,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER3",
					Offset = 29,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER4
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER4",
					Offset = 30,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER5
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER5",
					Offset = 31,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER6
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER6",
					Offset = 32,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER7
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER7",
					Offset = 33,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER8
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER8",
					Offset = 34,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER9
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER9",
					Offset = 35,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER10
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER10",
					Offset = 36,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER11
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER11",
					Offset = 37,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXHEALTH
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXHEALTH",
					Offset = 38,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER1",
					Offset = 39,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER2
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER2",
					Offset = 40,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER3",
					Offset = 41,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER4
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER4",
					Offset = 42,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER5
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER5",
					Offset = 43,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER6
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER6",
					Offset = 44,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER7
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER7",
					Offset = 45,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER8
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER8",
					Offset = 46,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER9
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER9",
					Offset = 47,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER10
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER10",
					Offset = 48,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER11
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER11",
					Offset = 49,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER_REGEN_FLAT_MODIFIER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POWER_REGEN_FLAT_MODIFIER",
					Offset = 50,
					Size = 11,
					Type = UpdateFieldType.Float
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// UnitFields.POWER_REGEN_INTERRUPTED_FLAT_MODIFIER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POWER_REGEN_INTERRUPTED_FLAT_MODIFIER",
					Offset = 61,
					Size = 11,
					Type = UpdateFieldType.Float
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// UnitFields.LEVEL
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "LEVEL",
					Offset = 72,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.FACTIONTEMPLATE
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "FACTIONTEMPLATE",
					Offset = 73,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.VIRTUAL_ITEM_SLOT_ID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "VIRTUAL_ITEM_SLOT_ID",
					Offset = 74,
					Size = 3,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				// UnitFields.FLAGS
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "FLAGS",
					Offset = 77,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.FLAGS_2
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "FLAGS_2",
					Offset = 78,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.AURASTATE
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "AURASTATE",
					Offset = 79,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.BASEATTACKTIME
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "BASEATTACKTIME",
					Offset = 80,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// UnitFields.RANGEDATTACKTIME
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Unit,
					Name = "RANGEDATTACKTIME",
					Offset = 82,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.BOUNDINGRADIUS
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "BOUNDINGRADIUS",
					Offset = 83,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.COMBATREACH
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "COMBATREACH",
					Offset = 84,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.DISPLAYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "DISPLAYID",
					Offset = 85,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NATIVEDISPLAYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "NATIVEDISPLAYID",
					Offset = 86,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MOUNTDISPLAYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MOUNTDISPLAYID",
					Offset = 87,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MINDAMAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.ItemOwner,
					Group = ObjectTypeId.Unit,
					Name = "MINDAMAGE",
					Offset = 88,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.MAXDAMAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.ItemOwner,
					Group = ObjectTypeId.Unit,
					Name = "MAXDAMAGE",
					Offset = 89,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.MINOFFHANDDAMAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.ItemOwner,
					Group = ObjectTypeId.Unit,
					Name = "MINOFFHANDDAMAGE",
					Offset = 90,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.MAXOFFHANDDAMAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.ItemOwner,
					Group = ObjectTypeId.Unit,
					Name = "MAXOFFHANDDAMAGE",
					Offset = 91,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.BYTES_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "BYTES_1",
					Offset = 92,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// UnitFields.PETNUMBER
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "PETNUMBER",
					Offset = 93,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.PET_NAME_TIMESTAMP
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "PET_NAME_TIMESTAMP",
					Offset = 94,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.PETEXPERIENCE
				new UpdateField {
					Flags = UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "PETEXPERIENCE",
					Offset = 95,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.PETNEXTLEVELEXP
				new UpdateField {
					Flags = UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "PETNEXTLEVELEXP",
					Offset = 96,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.DYNAMIC_FLAGS
				new UpdateField {
					Flags = UpdateFieldFlags.Dynamic,
					Group = ObjectTypeId.Unit,
					Name = "DYNAMIC_FLAGS",
					Offset = 97,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MOD_CAST_SPEED
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MOD_CAST_SPEED",
					Offset = 98,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.CREATED_BY_SPELL
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "CREATED_BY_SPELL",
					Offset = 99,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NPC_FLAGS
				new UpdateField {
					Flags = UpdateFieldFlags.Dynamic,
					Group = ObjectTypeId.Unit,
					Name = "NPC_FLAGS",
					Offset = 100,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NPC_EMOTESTATE
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "NPC_EMOTESTATE",
					Offset = 101,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.STAT0
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "STAT0",
					Offset = 102,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.STAT1
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "STAT1",
					Offset = 103,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.STAT2
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "STAT2",
					Offset = 104,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.STAT3
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "STAT3",
					Offset = 105,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.STAT4
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "STAT4",
					Offset = 106,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POSSTAT0
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POSSTAT0",
					Offset = 107,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POSSTAT1
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POSSTAT1",
					Offset = 108,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POSSTAT2
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POSSTAT2",
					Offset = 109,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POSSTAT3
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POSSTAT3",
					Offset = 110,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POSSTAT4
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POSSTAT4",
					Offset = 111,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NEGSTAT0
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "NEGSTAT0",
					Offset = 112,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NEGSTAT1
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "NEGSTAT1",
					Offset = 113,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NEGSTAT2
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "NEGSTAT2",
					Offset = 114,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NEGSTAT3
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "NEGSTAT3",
					Offset = 115,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NEGSTAT4
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "NEGSTAT4",
					Offset = 116,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.RESISTANCES
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.ItemOwner,
					Group = ObjectTypeId.Unit,
					Name = "RESISTANCES",
					Offset = 117,
					Size = 7,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				null,
				null,
				// UnitFields.RESISTANCEBUFFMODSPOSITIVE
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "RESISTANCEBUFFMODSPOSITIVE",
					Offset = 124,
					Size = 7,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				null,
				null,
				// UnitFields.RESISTANCEBUFFMODSNEGATIVE
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "RESISTANCEBUFFMODSNEGATIVE",
					Offset = 131,
					Size = 7,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				null,
				null,
				// UnitFields.BASE_MANA
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "BASE_MANA",
					Offset = 138,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.BASE_HEALTH
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "BASE_HEALTH",
					Offset = 139,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.BYTES_2
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "BYTES_2",
					Offset = 140,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// UnitFields.ATTACK_POWER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "ATTACK_POWER",
					Offset = 141,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.ATTACK_POWER_MOD_POS
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "ATTACK_POWER_MOD_POS",
					Offset = 142,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.ATTACK_POWER_MOD_NEG
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "ATTACK_POWER_MOD_NEG",
					Offset = 143,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.ATTACK_POWER_MULTIPLIER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "ATTACK_POWER_MULTIPLIER",
					Offset = 144,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.RANGED_ATTACK_POWER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "RANGED_ATTACK_POWER",
					Offset = 145,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.RANGED_ATTACK_POWER_MOD_POS
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "RANGED_ATTACK_POWER_MOD_POS",
					Offset = 146,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.RANGED_ATTACK_POWER_MOD_NEG
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "RANGED_ATTACK_POWER_MOD_NEG",
					Offset = 147,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.RANGED_ATTACK_POWER_MULTIPLIER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "RANGED_ATTACK_POWER_MULTIPLIER",
					Offset = 148,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.MINRANGEDDAMAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "MINRANGEDDAMAGE",
					Offset = 149,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.MAXRANGEDDAMAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "MAXRANGEDDAMAGE",
					Offset = 150,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.POWER_COST_MODIFIER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POWER_COST_MODIFIER",
					Offset = 151,
					Size = 7,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				null,
				null,
				// UnitFields.POWER_COST_MULTIPLIER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POWER_COST_MULTIPLIER",
					Offset = 158,
					Size = 7,
					Type = UpdateFieldType.Float
				},
				null,
				null,
				null,
				null,
				null,
				null,
				// UnitFields.MAXHEALTHMODIFIER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "MAXHEALTHMODIFIER",
					Offset = 165,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.HOVERHEIGHT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "HOVERHEIGHT",
					Offset = 166,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.MAXITEMLEVEL
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXITEMLEVEL",
					Offset = 167,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
			},
			#endregion

			#region Player
			new UpdateField[]{
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.DUEL_ARBITER
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "DUEL_ARBITER",
					Offset = 168,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// PlayerFields.FLAGS
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "FLAGS",
					Offset = 170,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.GUILDRANK
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "GUILDRANK",
					Offset = 171,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.GUILDDELETE_DATE
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "GUILDDELETE_DATE",
					Offset = 172,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.GUILDLEVEL
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "GUILDLEVEL",
					Offset = 173,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.BYTES
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "BYTES",
					Offset = 174,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// PlayerFields.BYTES_2
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "BYTES_2",
					Offset = 175,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// PlayerFields.BYTES_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "BYTES_3",
					Offset = 176,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// PlayerFields.DUEL_TEAM
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "DUEL_TEAM",
					Offset = 177,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.GUILD_TIMESTAMP
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "GUILD_TIMESTAMP",
					Offset = 178,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_1_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_1_1",
					Offset = 179,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_1_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_1_2",
					Offset = 180,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_1_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_1_3",
					Offset = 181,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_1_4
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_1_4",
					Offset = 183,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_2_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_2_1",
					Offset = 184,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_2_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_2_2",
					Offset = 185,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_2_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_2_3",
					Offset = 186,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_2_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_2_5",
					Offset = 188,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_3_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_3_1",
					Offset = 189,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_3_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_3_2",
					Offset = 190,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_3_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_3_3",
					Offset = 191,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_3_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_3_5",
					Offset = 193,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_4_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_4_1",
					Offset = 194,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_4_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_4_2",
					Offset = 195,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_4_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_4_3",
					Offset = 196,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_4_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_4_5",
					Offset = 198,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_5_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_5_1",
					Offset = 199,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_5_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_5_2",
					Offset = 200,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_5_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_5_3",
					Offset = 201,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_5_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_5_5",
					Offset = 203,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_6_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_6_1",
					Offset = 204,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_6_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_6_2",
					Offset = 205,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_6_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_6_3",
					Offset = 206,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_6_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_6_5",
					Offset = 208,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_7_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_7_1",
					Offset = 209,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_7_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_7_2",
					Offset = 210,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_7_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_7_3",
					Offset = 211,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_7_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_7_5",
					Offset = 213,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_8_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_8_1",
					Offset = 214,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_8_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_8_2",
					Offset = 215,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_8_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_8_3",
					Offset = 216,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_8_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_8_5",
					Offset = 218,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_9_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_9_1",
					Offset = 219,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_9_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_9_2",
					Offset = 220,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_9_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_9_3",
					Offset = 221,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_9_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_9_5",
					Offset = 223,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_10_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_10_1",
					Offset = 224,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_10_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_10_2",
					Offset = 225,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_10_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_10_3",
					Offset = 226,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_10_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_10_5",
					Offset = 228,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_11_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_11_1",
					Offset = 229,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_11_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_11_2",
					Offset = 230,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_11_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_11_3",
					Offset = 231,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_11_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_11_5",
					Offset = 233,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_12_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_12_1",
					Offset = 234,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_12_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_12_2",
					Offset = 235,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_12_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_12_3",
					Offset = 236,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_12_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_12_5",
					Offset = 238,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_13_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_13_1",
					Offset = 239,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_13_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_13_2",
					Offset = 240,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_13_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_13_3",
					Offset = 241,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_13_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_13_5",
					Offset = 243,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_14_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_14_1",
					Offset = 244,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_14_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_14_2",
					Offset = 245,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_14_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_14_3",
					Offset = 246,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_14_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_14_5",
					Offset = 248,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_15_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_15_1",
					Offset = 249,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_15_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_15_2",
					Offset = 250,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_15_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_15_3",
					Offset = 251,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_15_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_15_5",
					Offset = 253,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_16_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_16_1",
					Offset = 254,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_16_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_16_2",
					Offset = 255,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_16_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_16_3",
					Offset = 256,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_16_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_16_5",
					Offset = 258,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_17_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_17_1",
					Offset = 259,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_17_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_17_2",
					Offset = 260,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_17_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_17_3",
					Offset = 261,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_17_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_17_5",
					Offset = 263,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_18_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_18_1",
					Offset = 264,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_18_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_18_2",
					Offset = 265,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_18_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_18_3",
					Offset = 266,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_18_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_18_5",
					Offset = 268,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_19_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_19_1",
					Offset = 269,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_19_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_19_2",
					Offset = 270,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_19_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_19_3",
					Offset = 271,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_19_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_19_5",
					Offset = 273,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_20_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_20_1",
					Offset = 274,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_20_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_20_2",
					Offset = 275,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_20_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_20_3",
					Offset = 276,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_20_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_20_5",
					Offset = 278,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_21_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_21_1",
					Offset = 279,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_21_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_21_2",
					Offset = 280,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_21_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_21_3",
					Offset = 281,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_21_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_21_5",
					Offset = 283,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_22_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_22_1",
					Offset = 284,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_22_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_22_2",
					Offset = 285,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_22_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_22_3",
					Offset = 286,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_22_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_22_5",
					Offset = 288,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_23_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_23_1",
					Offset = 289,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_23_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_23_2",
					Offset = 290,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_23_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_23_3",
					Offset = 291,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_23_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_23_5",
					Offset = 293,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_24_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_24_1",
					Offset = 294,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_24_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_24_2",
					Offset = 295,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_24_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_24_3",
					Offset = 296,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_24_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_24_5",
					Offset = 298,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_25_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_25_1",
					Offset = 299,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_25_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_25_2",
					Offset = 300,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_25_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_25_3",
					Offset = 301,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_25_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_25_5",
					Offset = 303,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_26_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_26_1",
					Offset = 304,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_26_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_26_2",
					Offset = 305,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_26_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_26_3",
					Offset = 306,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_26_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_26_5",
					Offset = 308,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_27_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_27_1",
					Offset = 309,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_27_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_27_2",
					Offset = 310,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_27_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_27_3",
					Offset = 311,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_27_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_27_5",
					Offset = 313,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_28_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_28_1",
					Offset = 314,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_28_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_28_2",
					Offset = 315,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_28_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_28_3",
					Offset = 316,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_28_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_28_5",
					Offset = 318,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_29_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_29_1",
					Offset = 319,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_29_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_29_2",
					Offset = 320,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_29_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_29_3",
					Offset = 321,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_29_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_29_5",
					Offset = 323,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_30_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_30_1",
					Offset = 324,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_30_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_30_2",
					Offset = 325,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_30_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_30_3",
					Offset = 326,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_30_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_30_5",
					Offset = 328,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_31_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_31_1",
					Offset = 329,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_31_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_31_2",
					Offset = 330,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_31_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_31_3",
					Offset = 331,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_31_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_31_5",
					Offset = 333,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_32_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_32_1",
					Offset = 334,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_32_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_32_2",
					Offset = 335,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_32_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_32_3",
					Offset = 336,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_32_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_32_5",
					Offset = 338,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_33_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_33_1",
					Offset = 339,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_33_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_33_2",
					Offset = 340,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_33_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_33_3",
					Offset = 341,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_33_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_33_5",
					Offset = 343,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_34_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_34_1",
					Offset = 344,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_34_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_34_2",
					Offset = 345,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_34_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_34_3",
					Offset = 346,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_34_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_34_5",
					Offset = 348,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_35_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_35_1",
					Offset = 349,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_35_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_35_2",
					Offset = 350,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_35_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_35_3",
					Offset = 351,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_35_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_35_5",
					Offset = 353,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_36_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_36_1",
					Offset = 354,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_36_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_36_2",
					Offset = 355,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_36_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_36_3",
					Offset = 356,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_36_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_36_5",
					Offset = 358,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_37_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_37_1",
					Offset = 359,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_37_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_37_2",
					Offset = 360,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_37_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_37_3",
					Offset = 361,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_37_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_37_5",
					Offset = 363,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_38_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_38_1",
					Offset = 364,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_38_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_38_2",
					Offset = 365,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_38_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_38_3",
					Offset = 366,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_38_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_38_5",
					Offset = 368,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_39_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_39_1",
					Offset = 369,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_39_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_39_2",
					Offset = 370,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_39_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_39_3",
					Offset = 371,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_39_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_39_5",
					Offset = 373,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_40_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_40_1",
					Offset = 374,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_40_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_40_2",
					Offset = 375,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_40_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_40_3",
					Offset = 376,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_40_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_40_5",
					Offset = 378,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_41_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_41_1",
					Offset = 379,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_41_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_41_2",
					Offset = 380,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_41_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_41_3",
					Offset = 381,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_41_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_41_5",
					Offset = 383,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_42_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_42_1",
					Offset = 384,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_42_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_42_2",
					Offset = 385,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_42_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_42_3",
					Offset = 386,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_42_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_42_5",
					Offset = 388,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_43_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_43_1",
					Offset = 389,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_43_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_43_2",
					Offset = 390,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_43_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_43_3",
					Offset = 391,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_43_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_43_5",
					Offset = 393,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_44_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_44_1",
					Offset = 394,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_44_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_44_2",
					Offset = 395,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_44_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_44_3",
					Offset = 396,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_44_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_44_5",
					Offset = 398,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_45_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_45_1",
					Offset = 399,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_45_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_45_2",
					Offset = 400,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_45_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_45_3",
					Offset = 401,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_45_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_45_5",
					Offset = 403,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_46_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_46_1",
					Offset = 404,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_46_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_46_2",
					Offset = 405,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_46_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_46_3",
					Offset = 406,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_46_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_46_5",
					Offset = 408,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_47_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_47_1",
					Offset = 409,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_47_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_47_2",
					Offset = 410,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_47_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_47_3",
					Offset = 411,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_47_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_47_5",
					Offset = 413,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_48_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_48_1",
					Offset = 414,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_48_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_48_2",
					Offset = 415,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_48_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_48_3",
					Offset = 416,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_48_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_48_5",
					Offset = 418,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_49_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_49_1",
					Offset = 419,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_49_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_49_2",
					Offset = 420,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_49_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_49_3",
					Offset = 421,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_49_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_49_5",
					Offset = 423,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_50_1
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_50_1",
					Offset = 424,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_50_2
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_50_2",
					Offset = 425,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_50_3
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_50_3",
					Offset = 426,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_50_5
				new UpdateField {
					Flags = UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_50_5",
					Offset = 428,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_1_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_1_ENTRYID",
					Offset = 429,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_1_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_1_ENCHANTMENT",
					Offset = 430,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_2_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_2_ENTRYID",
					Offset = 431,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_2_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_2_ENCHANTMENT",
					Offset = 432,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_3_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_3_ENTRYID",
					Offset = 433,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_3_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_3_ENCHANTMENT",
					Offset = 434,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_4_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_4_ENTRYID",
					Offset = 435,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_4_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_4_ENCHANTMENT",
					Offset = 436,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_5_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_5_ENTRYID",
					Offset = 437,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_5_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_5_ENCHANTMENT",
					Offset = 438,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_6_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_6_ENTRYID",
					Offset = 439,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_6_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_6_ENCHANTMENT",
					Offset = 440,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_7_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_7_ENTRYID",
					Offset = 441,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_7_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_7_ENCHANTMENT",
					Offset = 442,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_8_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_8_ENTRYID",
					Offset = 443,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_8_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_8_ENCHANTMENT",
					Offset = 444,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_9_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_9_ENTRYID",
					Offset = 445,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_9_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_9_ENCHANTMENT",
					Offset = 446,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_10_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_10_ENTRYID",
					Offset = 447,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_10_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_10_ENCHANTMENT",
					Offset = 448,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_11_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_11_ENTRYID",
					Offset = 449,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_11_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_11_ENCHANTMENT",
					Offset = 450,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_12_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_12_ENTRYID",
					Offset = 451,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_12_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_12_ENCHANTMENT",
					Offset = 452,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_13_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_13_ENTRYID",
					Offset = 453,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_13_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_13_ENCHANTMENT",
					Offset = 454,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_14_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_14_ENTRYID",
					Offset = 455,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_14_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_14_ENCHANTMENT",
					Offset = 456,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_15_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_15_ENTRYID",
					Offset = 457,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_15_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_15_ENCHANTMENT",
					Offset = 458,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_16_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_16_ENTRYID",
					Offset = 459,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_16_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_16_ENCHANTMENT",
					Offset = 460,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_17_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_17_ENTRYID",
					Offset = 461,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_17_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_17_ENCHANTMENT",
					Offset = 462,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_18_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_18_ENTRYID",
					Offset = 463,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_18_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_18_ENCHANTMENT",
					Offset = 464,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_19_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_19_ENTRYID",
					Offset = 465,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_19_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_19_ENCHANTMENT",
					Offset = 466,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.CHOSEN_TITLE
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "CHOSEN_TITLE",
					Offset = 467,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.FAKE_INEBRIATION
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "FAKE_INEBRIATION",
					Offset = 468,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.PAD_0
				new UpdateField {
					Flags = UpdateFieldFlags.None,
					Group = ObjectTypeId.Player,
					Name = "PAD_0",
					Offset = 469,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.INV_SLOT_HEAD
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "INV_SLOT_HEAD",
					Offset = 470,
					Size = 46,
					Type = UpdateFieldType.Guid
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.PACK_SLOT_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "PACK_SLOT_1",
					Offset = 516,
					Size = 32,
					Type = UpdateFieldType.Guid
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.BANK_SLOT_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "BANK_SLOT_1",
					Offset = 548,
					Size = 56,
					Type = UpdateFieldType.Guid
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.BANKBAG_SLOT_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "BANKBAG_SLOT_1",
					Offset = 604,
					Size = 14,
					Type = UpdateFieldType.Guid
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.VENDORBUYBACK_SLOT_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "VENDORBUYBACK_SLOT_1",
					Offset = 618,
					Size = 24,
					Type = UpdateFieldType.Guid
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.KEYRING_SLOT_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "KEYRING_SLOT_1",
					Offset = 642,
					Size = 64,
					Type = UpdateFieldType.Guid
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.FARSIGHT
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "FARSIGHT",
					Offset = 706,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// PlayerFields._FIELD_KNOWN_TITLES
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "_FIELD_KNOWN_TITLES",
					Offset = 708,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// PlayerFields._FIELD_KNOWN_TITLES1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "_FIELD_KNOWN_TITLES1",
					Offset = 710,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// PlayerFields._FIELD_KNOWN_TITLES2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "_FIELD_KNOWN_TITLES2",
					Offset = 712,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// PlayerFields.XP
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "XP",
					Offset = 714,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.NEXT_LEVEL_XP
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "NEXT_LEVEL_XP",
					Offset = 715,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.SKILL_INFO_1_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "SKILL_INFO_1_1",
					Offset = 716,
					Size = 384,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.CHARACTER_POINTS
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "CHARACTER_POINTS",
					Offset = 1100,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.TRACK_CREATURES
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "TRACK_CREATURES",
					Offset = 1101,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.TRACK_RESOURCES
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "TRACK_RESOURCES",
					Offset = 1102,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.BLOCK_PERCENTAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "BLOCK_PERCENTAGE",
					Offset = 1103,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.DODGE_PERCENTAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "DODGE_PERCENTAGE",
					Offset = 1104,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.PARRY_PERCENTAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "PARRY_PERCENTAGE",
					Offset = 1105,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.EXPERTISE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "EXPERTISE",
					Offset = 1106,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.OFFHAND_EXPERTISE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "OFFHAND_EXPERTISE",
					Offset = 1107,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.CRIT_PERCENTAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "CRIT_PERCENTAGE",
					Offset = 1108,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.RANGED_CRIT_PERCENTAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "RANGED_CRIT_PERCENTAGE",
					Offset = 1109,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.OFFHAND_CRIT_PERCENTAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "OFFHAND_CRIT_PERCENTAGE",
					Offset = 1110,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.SPELL_CRIT_PERCENTAGE1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "SPELL_CRIT_PERCENTAGE1",
					Offset = 1111,
					Size = 7,
					Type = UpdateFieldType.Float
				},
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.SHIELD_BLOCK
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "SHIELD_BLOCK",
					Offset = 1118,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.SHIELD_BLOCK_CRIT_PERCENTAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "SHIELD_BLOCK_CRIT_PERCENTAGE",
					Offset = 1119,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.MASTERY
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MASTERY",
					Offset = 1120,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.EXPLORED_ZONES_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "EXPLORED_ZONES_1",
					Offset = 1121,
					Size = 144,
					Type = UpdateFieldType.ByteArray
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.REST_STATE_EXPERIENCE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "REST_STATE_EXPERIENCE",
					Offset = 1265,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.COINAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "COINAGE",
					Offset = 1266,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// PlayerFields.MOD_DAMAGE_DONE_POS
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_DAMAGE_DONE_POS",
					Offset = 1268,
					Size = 7,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.MOD_DAMAGE_DONE_NEG
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_DAMAGE_DONE_NEG",
					Offset = 1275,
					Size = 7,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.MOD_DAMAGE_DONE_PCT
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_DAMAGE_DONE_PCT",
					Offset = 1282,
					Size = 7,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.MOD_HEALING_DONE_POS
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_HEALING_DONE_POS",
					Offset = 1289,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.MOD_HEALING_PCT
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_HEALING_PCT",
					Offset = 1290,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.MOD_HEALING_DONE_PCT
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_HEALING_DONE_PCT",
					Offset = 1291,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.MOD_SPELL_POWER_PCT
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_SPELL_POWER_PCT",
					Offset = 1292,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.MOD_TARGET_RESISTANCE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_TARGET_RESISTANCE",
					Offset = 1293,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.MOD_TARGET_PHYSICAL_RESISTANCE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_TARGET_PHYSICAL_RESISTANCE",
					Offset = 1294,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.PLAYER_FIELD_BYTES
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "PLAYER_FIELD_BYTES",
					Offset = 1295,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// PlayerFields.SELF_RES_SPELL
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "SELF_RES_SPELL",
					Offset = 1296,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.PVP_MEDALS
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "PVP_MEDALS",
					Offset = 1297,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.BUYBACK_PRICE_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "BUYBACK_PRICE_1",
					Offset = 1298,
					Size = 12,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.BUYBACK_TIMESTAMP_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "BUYBACK_TIMESTAMP_1",
					Offset = 1310,
					Size = 12,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.KILLS
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "KILLS",
					Offset = 1322,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.LIFETIME_HONORBALE_KILLS
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "LIFETIME_HONORBALE_KILLS",
					Offset = 1323,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.PLAYER_FIELD_BYTES2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "PLAYER_FIELD_BYTES2",
					Offset = 1324,
					Size = 1,
					Type = UpdateFieldType.Unk322
				},
				// PlayerFields.WATCHED_FACTION_INDEX
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "WATCHED_FACTION_INDEX",
					Offset = 1325,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.COMBAT_RATING_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "COMBAT_RATING_1",
					Offset = 1326,
					Size = 26,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.ARENA_TEAM_INFO_1_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "ARENA_TEAM_INFO_1_1",
					Offset = 1352,
					Size = 21,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.BATTLEGROUND_RATING
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "BATTLEGROUND_RATING",
					Offset = 1373,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.MAX_LEVEL
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MAX_LEVEL",
					Offset = 1374,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.DAILY_QUESTS_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "DAILY_QUESTS_1",
					Offset = 1375,
					Size = 25,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.RUNE_REGEN_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "RUNE_REGEN_1",
					Offset = 1400,
					Size = 4,
					Type = UpdateFieldType.Float
				},
				null,
				null,
				null,
				// PlayerFields.NO_REAGENT_COST_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "NO_REAGENT_COST_1",
					Offset = 1404,
					Size = 3,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				// PlayerFields.GLYPH_SLOTS_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "GLYPH_SLOTS_1",
					Offset = 1407,
					Size = 9,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.GLYPHS_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "GLYPHS_1",
					Offset = 1416,
					Size = 9,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.GLYPHS_ENABLED
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "GLYPHS_ENABLED",
					Offset = 1425,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.PET_SPELL_POWER
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "PET_SPELL_POWER",
					Offset = 1426,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.RESEARCHING_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "RESEARCHING_1",
					Offset = 1427,
					Size = 8,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.RESERACH_SITE_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "RESERACH_SITE_1",
					Offset = 1435,
					Size = 8,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// PlayerFields.PROFESSION_SKILL_LINE_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "PROFESSION_SKILL_LINE_1",
					Offset = 1443,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// PlayerFields.UI_HIT_MODIFIER
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "UI_HIT_MODIFIER",
					Offset = 1445,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.UI_SPELL_HIT_MODIFIER
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "UI_SPELL_HIT_MODIFIER",
					Offset = 1446,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.HOME_REALM_TIME_OFFSET
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "HOME_REALM_TIME_OFFSET",
					Offset = 1447,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.MOD_HASTE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_HASTE",
					Offset = 1448,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.MOD_RANGED_HASTE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_RANGED_HASTE",
					Offset = 1449,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.MOD_PET_HASTE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_PET_HASTE",
					Offset = 1450,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.MOD_HASTE_REGEN
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_HASTE_REGEN",
					Offset = 1451,
					Size = 1,
					Type = UpdateFieldType.Float
				},
			},
			#endregion

			#region GameObject
			new UpdateField[]{
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// GameObjectFields.OBJECT_FIELD_CREATED_BY
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.GameObject,
					Name = "OBJECT_FIELD_CREATED_BY",
					Offset = 8,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// GameObjectFields.DISPLAYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.GameObject,
					Name = "DISPLAYID",
					Offset = 10,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// GameObjectFields.FLAGS
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.GameObject,
					Name = "FLAGS",
					Offset = 11,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// GameObjectFields.PARENTROTATION
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.GameObject,
					Name = "PARENTROTATION",
					Offset = 12,
					Size = 4,
					Type = UpdateFieldType.Float
				},
				null,
				null,
				null,
				// GameObjectFields.DYNAMIC
				new UpdateField {
					Flags = UpdateFieldFlags.Dynamic,
					Group = ObjectTypeId.GameObject,
					Name = "DYNAMIC",
					Offset = 16,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// GameObjectFields.FACTION
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.GameObject,
					Name = "FACTION",
					Offset = 17,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// GameObjectFields.LEVEL
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.GameObject,
					Name = "LEVEL",
					Offset = 18,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// GameObjectFields.BYTES_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.GameObject,
					Name = "BYTES_1",
					Offset = 19,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
			},
			#endregion

			#region DynamicObject
			new UpdateField[]{
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// DynamicObjectFields.CASTER
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.DynamicObject,
					Name = "CASTER",
					Offset = 8,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// DynamicObjectFields.BYTES
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.DynamicObject,
					Name = "BYTES",
					Offset = 10,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// DynamicObjectFields.SPELLID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.DynamicObject,
					Name = "SPELLID",
					Offset = 11,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// DynamicObjectFields.RADIUS
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.DynamicObject,
					Name = "RADIUS",
					Offset = 12,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// DynamicObjectFields.CASTTIME
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.DynamicObject,
					Name = "CASTTIME",
					Offset = 13,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
			},
			#endregion

			#region Corpse
			new UpdateField[]{
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// CorpseFields.OWNER
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Corpse,
					Name = "OWNER",
					Offset = 8,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// CorpseFields.PARTY
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Corpse,
					Name = "PARTY",
					Offset = 10,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// CorpseFields.DISPLAY_ID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Corpse,
					Name = "DISPLAY_ID",
					Offset = 12,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// CorpseFields.ITEM
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Corpse,
					Name = "ITEM",
					Offset = 13,
					Size = 19,
					Type = UpdateFieldType.UInt32
				},
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				// CorpseFields.BYTES_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Corpse,
					Name = "BYTES_1",
					Offset = 32,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// CorpseFields.BYTES_2
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Corpse,
					Name = "BYTES_2",
					Offset = 33,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// CorpseFields.FLAGS
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Corpse,
					Name = "FLAGS",
					Offset = 34,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// CorpseFields.DYNAMIC_FLAGS
				new UpdateField {
					Flags = UpdateFieldFlags.Dynamic,
					Group = ObjectTypeId.Corpse,
					Name = "DYNAMIC_FLAGS",
					Offset = 35,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
			},
			#endregion

		};

	}

}

