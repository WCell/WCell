using System;

///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 15/06/2010
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
					Type = UpdateFieldType.UInt32
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
				// ObjectFields.PADDING
				new UpdateField {
					Flags = UpdateFieldFlags.None,
					Group = ObjectTypeId.Object,
					Name = "PADDING",
					Offset = 5,
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
				// ItemFields.OWNER
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "OWNER",
					Offset = 6,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// ItemFields.CONTAINED
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "CONTAINED",
					Offset = 8,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// ItemFields.CREATOR
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "CREATOR",
					Offset = 10,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// ItemFields.GIFTCREATOR
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "GIFTCREATOR",
					Offset = 12,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// ItemFields.STACK_COUNT
				new UpdateField {
					Flags = UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.ItemOwner,
					Group = ObjectTypeId.Item,
					Name = "STACK_COUNT",
					Offset = 14,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.DURATION
				new UpdateField {
					Flags = UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.ItemOwner,
					Group = ObjectTypeId.Item,
					Name = "DURATION",
					Offset = 15,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.SPELL_CHARGES
				new UpdateField {
					Flags = UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.ItemOwner,
					Group = ObjectTypeId.Item,
					Name = "SPELL_CHARGES",
					Offset = 16,
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
					Offset = 21,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.ENCHANTMENT_1_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_1_1",
					Offset = 22,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_1_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_1_3",
					Offset = 24,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_2_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_2_1",
					Offset = 25,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_2_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_2_3",
					Offset = 27,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_3_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_3_1",
					Offset = 28,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_3_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_3_3",
					Offset = 30,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_4_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_4_1",
					Offset = 31,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_4_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_4_3",
					Offset = 33,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_5_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_5_1",
					Offset = 34,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_5_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_5_3",
					Offset = 36,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_6_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_6_1",
					Offset = 37,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_6_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_6_3",
					Offset = 39,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_7_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_7_1",
					Offset = 40,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_7_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_7_3",
					Offset = 42,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_8_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_8_1",
					Offset = 43,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_8_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_8_3",
					Offset = 45,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_9_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_9_1",
					Offset = 46,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_9_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_9_3",
					Offset = 48,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_10_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_10_1",
					Offset = 49,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_10_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_10_3",
					Offset = 51,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_11_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_11_1",
					Offset = 52,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_11_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_11_3",
					Offset = 54,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.ENCHANTMENT_12_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_12_1",
					Offset = 55,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// ItemFields.ENCHANTMENT_12_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "ENCHANTMENT_12_3",
					Offset = 57,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// ItemFields.PROPERTY_SEED
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "PROPERTY_SEED",
					Offset = 58,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.RANDOM_PROPERTIES_ID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "RANDOM_PROPERTIES_ID",
					Offset = 59,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.DURABILITY
				new UpdateField {
					Flags = UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.ItemOwner,
					Group = ObjectTypeId.Item,
					Name = "DURABILITY",
					Offset = 60,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.MAXDURABILITY
				new UpdateField {
					Flags = UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.ItemOwner,
					Group = ObjectTypeId.Item,
					Name = "MAXDURABILITY",
					Offset = 61,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.CREATE_PLAYED_TIME
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Item,
					Name = "CREATE_PLAYED_TIME",
					Offset = 62,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ItemFields.PAD
				new UpdateField {
					Flags = UpdateFieldFlags.None,
					Group = ObjectTypeId.Item,
					Name = "PAD",
					Offset = 63,
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
				// ContainerFields.NUM_SLOTS
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Container,
					Name = "NUM_SLOTS",
					Offset = 64,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// ContainerFields.ALIGN_PAD
				new UpdateField {
					Flags = UpdateFieldFlags.None,
					Group = ObjectTypeId.Container,
					Name = "ALIGN_PAD",
					Offset = 65,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// ContainerFields.SLOT_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Container,
					Name = "SLOT_1",
					Offset = 66,
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
				// UnitFields.CHARM
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "CHARM",
					Offset = 6,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.SUMMON
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "SUMMON",
					Offset = 8,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.CRITTER
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Unit,
					Name = "CRITTER",
					Offset = 10,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.CHARMEDBY
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "CHARMEDBY",
					Offset = 12,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.SUMMONEDBY
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "SUMMONEDBY",
					Offset = 14,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.CREATEDBY
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "CREATEDBY",
					Offset = 16,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.TARGET
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "TARGET",
					Offset = 18,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.CHANNEL_OBJECT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "CHANNEL_OBJECT",
					Offset = 20,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// UnitFields.CHANNEL_SPELL
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "CHANNEL_SPELL",
					Offset = 22,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.BYTES_0
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "BYTES_0",
					Offset = 23,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// UnitFields.HEALTH
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "HEALTH",
					Offset = 24,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER1",
					Offset = 25,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER2
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER2",
					Offset = 26,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER3",
					Offset = 27,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER4
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER4",
					Offset = 28,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER5
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER5",
					Offset = 29,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER6
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER6",
					Offset = 30,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER7
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER7",
					Offset = 31,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER8
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER8",
					Offset = 32,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER9
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "POWER9",
					Offset = 33,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXHEALTH
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXHEALTH",
					Offset = 34,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER1",
					Offset = 35,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER2
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER2",
					Offset = 36,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER3",
					Offset = 37,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER4
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER4",
					Offset = 38,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER5
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER5",
					Offset = 39,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER6
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER6",
					Offset = 40,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER7
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER7",
					Offset = 41,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER8
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER8",
					Offset = 42,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MAXPOWER9
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXPOWER9",
					Offset = 43,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POWER_REGEN_FLAT_MODIFIER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POWER_REGEN_FLAT_MODIFIER",
					Offset = 44,
					Size = 9,
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
				// UnitFields.POWER_REGEN_INTERRUPTED_FLAT_MODIFIER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POWER_REGEN_INTERRUPTED_FLAT_MODIFIER",
					Offset = 53,
					Size = 9,
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
				// UnitFields.LEVEL
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "LEVEL",
					Offset = 62,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.FACTIONTEMPLATE
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "FACTIONTEMPLATE",
					Offset = 63,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.VIRTUAL_ITEM_SLOT_ID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "VIRTUAL_ITEM_SLOT_ID",
					Offset = 64,
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
					Offset = 67,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.FLAGS_2
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "FLAGS_2",
					Offset = 68,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.AURASTATE
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "AURASTATE",
					Offset = 69,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.BASEATTACKTIME
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "BASEATTACKTIME",
					Offset = 70,
					Size = 2,
					Type = UpdateFieldType.UInt32
				},
				null,
				// UnitFields.RANGEDATTACKTIME
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Unit,
					Name = "RANGEDATTACKTIME",
					Offset = 72,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.BOUNDINGRADIUS
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "BOUNDINGRADIUS",
					Offset = 73,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.COMBATREACH
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "COMBATREACH",
					Offset = 74,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.DISPLAYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "DISPLAYID",
					Offset = 75,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NATIVEDISPLAYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "NATIVEDISPLAYID",
					Offset = 76,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MOUNTDISPLAYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MOUNTDISPLAYID",
					Offset = 77,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MINDAMAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Unit,
					Name = "MINDAMAGE",
					Offset = 78,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.MAXDAMAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Unit,
					Name = "MAXDAMAGE",
					Offset = 79,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.MINOFFHANDDAMAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Unit,
					Name = "MINOFFHANDDAMAGE",
					Offset = 80,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.MAXOFFHANDDAMAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Unit,
					Name = "MAXOFFHANDDAMAGE",
					Offset = 81,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.BYTES_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "BYTES_1",
					Offset = 82,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// UnitFields.PETNUMBER
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "PETNUMBER",
					Offset = 83,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.PET_NAME_TIMESTAMP
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "PET_NAME_TIMESTAMP",
					Offset = 84,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.PETEXPERIENCE
				new UpdateField {
					Flags = UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "PETEXPERIENCE",
					Offset = 85,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.PETNEXTLEVELEXP
				new UpdateField {
					Flags = UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "PETNEXTLEVELEXP",
					Offset = 86,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.DYNAMIC_FLAGS
				new UpdateField {
					Flags = UpdateFieldFlags.Dynamic,
					Group = ObjectTypeId.Unit,
					Name = "DYNAMIC_FLAGS",
					Offset = 87,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.MOD_CAST_SPEED
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MOD_CAST_SPEED",
					Offset = 88,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.CREATED_BY_SPELL
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "CREATED_BY_SPELL",
					Offset = 89,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NPC_FLAGS
				new UpdateField {
					Flags = UpdateFieldFlags.Dynamic,
					Group = ObjectTypeId.Unit,
					Name = "NPC_FLAGS",
					Offset = 90,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NPC_EMOTESTATE
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "NPC_EMOTESTATE",
					Offset = 91,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.STAT0
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "STAT0",
					Offset = 92,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.STAT1
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "STAT1",
					Offset = 93,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.STAT2
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "STAT2",
					Offset = 94,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.STAT3
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "STAT3",
					Offset = 95,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.STAT4
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "STAT4",
					Offset = 96,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POSSTAT0
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POSSTAT0",
					Offset = 97,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POSSTAT1
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POSSTAT1",
					Offset = 98,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POSSTAT2
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POSSTAT2",
					Offset = 99,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POSSTAT3
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POSSTAT3",
					Offset = 100,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.POSSTAT4
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POSSTAT4",
					Offset = 101,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NEGSTAT0
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "NEGSTAT0",
					Offset = 102,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NEGSTAT1
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "NEGSTAT1",
					Offset = 103,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NEGSTAT2
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "NEGSTAT2",
					Offset = 104,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NEGSTAT3
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "NEGSTAT3",
					Offset = 105,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.NEGSTAT4
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "NEGSTAT4",
					Offset = 106,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.RESISTANCES
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.BeastLore,
					Group = ObjectTypeId.Unit,
					Name = "RESISTANCES",
					Offset = 107,
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
					Offset = 114,
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
					Offset = 121,
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
					Offset = 128,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.BASE_HEALTH
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "BASE_HEALTH",
					Offset = 129,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.BYTES_2
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "BYTES_2",
					Offset = 130,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// UnitFields.ATTACK_POWER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "ATTACK_POWER",
					Offset = 131,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.ATTACK_POWER_MODS
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "ATTACK_POWER_MODS",
					Offset = 132,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// UnitFields.ATTACK_POWER_MULTIPLIER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "ATTACK_POWER_MULTIPLIER",
					Offset = 133,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.RANGED_ATTACK_POWER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "RANGED_ATTACK_POWER",
					Offset = 134,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// UnitFields.RANGED_ATTACK_POWER_MODS
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "RANGED_ATTACK_POWER_MODS",
					Offset = 135,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// UnitFields.RANGED_ATTACK_POWER_MULTIPLIER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "RANGED_ATTACK_POWER_MULTIPLIER",
					Offset = 136,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.MINRANGEDDAMAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "MINRANGEDDAMAGE",
					Offset = 137,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.MAXRANGEDDAMAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "MAXRANGEDDAMAGE",
					Offset = 138,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.POWER_COST_MODIFIER
				new UpdateField {
					Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
					Group = ObjectTypeId.Unit,
					Name = "POWER_COST_MODIFIER",
					Offset = 139,
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
					Offset = 146,
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
					Offset = 153,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.HOVERHEIGHT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "HOVERHEIGHT",
					Offset = 154,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// UnitFields.MAXITEMLEVEL
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Unit,
					Name = "MAXITEMLEVEL",
					Offset = 155,
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
				// PlayerFields.DUEL_ARBITER
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "DUEL_ARBITER",
					Offset = 156,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// PlayerFields.FLAGS
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "FLAGS",
					Offset = 158,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.GUILDID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "GUILDID",
					Offset = 159,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.GUILDRANK
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "GUILDRANK",
					Offset = 160,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.GUILDLEVEL
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "GUILDLEVEL",
					Offset = 161,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.BYTES
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "BYTES",
					Offset = 162,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// PlayerFields.BYTES_2
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "BYTES_2",
					Offset = 163,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// PlayerFields.BYTES_3
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "BYTES_3",
					Offset = 164,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// PlayerFields.DUEL_TEAM
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "DUEL_TEAM",
					Offset = 165,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.GUILD_TIMESTAMP
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "GUILD_TIMESTAMP",
					Offset = 166,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_1_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_1_1",
					Offset = 167,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_1_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_1_2",
					Offset = 168,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_1_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_1_3",
					Offset = 169,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_1_4
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_1_4",
					Offset = 171,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_2_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_2_1",
					Offset = 172,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_2_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_2_2",
					Offset = 173,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_2_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_2_3",
					Offset = 174,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_2_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_2_5",
					Offset = 176,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_3_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_3_1",
					Offset = 177,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_3_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_3_2",
					Offset = 178,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_3_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_3_3",
					Offset = 179,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_3_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_3_5",
					Offset = 181,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_4_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_4_1",
					Offset = 182,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_4_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_4_2",
					Offset = 183,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_4_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_4_3",
					Offset = 184,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_4_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_4_5",
					Offset = 186,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_5_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_5_1",
					Offset = 187,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_5_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_5_2",
					Offset = 188,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_5_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_5_3",
					Offset = 189,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_5_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_5_5",
					Offset = 191,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_6_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_6_1",
					Offset = 192,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_6_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_6_2",
					Offset = 193,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_6_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_6_3",
					Offset = 194,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_6_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_6_5",
					Offset = 196,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_7_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_7_1",
					Offset = 197,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_7_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_7_2",
					Offset = 198,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_7_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_7_3",
					Offset = 199,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_7_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_7_5",
					Offset = 201,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_8_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_8_1",
					Offset = 202,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_8_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_8_2",
					Offset = 203,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_8_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_8_3",
					Offset = 204,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_8_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_8_5",
					Offset = 206,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_9_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_9_1",
					Offset = 207,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_9_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_9_2",
					Offset = 208,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_9_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_9_3",
					Offset = 209,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_9_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_9_5",
					Offset = 211,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_10_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_10_1",
					Offset = 212,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_10_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_10_2",
					Offset = 213,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_10_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_10_3",
					Offset = 214,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_10_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_10_5",
					Offset = 216,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_11_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_11_1",
					Offset = 217,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_11_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_11_2",
					Offset = 218,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_11_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_11_3",
					Offset = 219,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_11_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_11_5",
					Offset = 221,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_12_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_12_1",
					Offset = 222,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_12_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_12_2",
					Offset = 223,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_12_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_12_3",
					Offset = 224,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_12_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_12_5",
					Offset = 226,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_13_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_13_1",
					Offset = 227,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_13_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_13_2",
					Offset = 228,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_13_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_13_3",
					Offset = 229,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_13_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_13_5",
					Offset = 231,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_14_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_14_1",
					Offset = 232,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_14_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_14_2",
					Offset = 233,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_14_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_14_3",
					Offset = 234,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_14_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_14_5",
					Offset = 236,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_15_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_15_1",
					Offset = 237,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_15_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_15_2",
					Offset = 238,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_15_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_15_3",
					Offset = 239,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_15_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_15_5",
					Offset = 241,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_16_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_16_1",
					Offset = 242,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_16_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_16_2",
					Offset = 243,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_16_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_16_3",
					Offset = 244,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_16_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_16_5",
					Offset = 246,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_17_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_17_1",
					Offset = 247,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_17_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_17_2",
					Offset = 248,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_17_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_17_3",
					Offset = 249,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_17_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_17_5",
					Offset = 251,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_18_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_18_1",
					Offset = 252,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_18_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_18_2",
					Offset = 253,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_18_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_18_3",
					Offset = 254,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_18_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_18_5",
					Offset = 256,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_19_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_19_1",
					Offset = 257,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_19_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_19_2",
					Offset = 258,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_19_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_19_3",
					Offset = 259,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_19_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_19_5",
					Offset = 261,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_20_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_20_1",
					Offset = 262,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_20_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_20_2",
					Offset = 263,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_20_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_20_3",
					Offset = 264,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_20_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_20_5",
					Offset = 266,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_21_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_21_1",
					Offset = 267,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_21_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_21_2",
					Offset = 268,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_21_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_21_3",
					Offset = 269,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_21_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_21_5",
					Offset = 271,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_22_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_22_1",
					Offset = 272,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_22_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_22_2",
					Offset = 273,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_22_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_22_3",
					Offset = 274,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_22_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_22_5",
					Offset = 276,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_23_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_23_1",
					Offset = 277,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_23_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_23_2",
					Offset = 278,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_23_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_23_3",
					Offset = 279,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_23_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_23_5",
					Offset = 281,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_24_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_24_1",
					Offset = 282,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_24_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_24_2",
					Offset = 283,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_24_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_24_3",
					Offset = 284,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_24_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_24_5",
					Offset = 286,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_25_1
				new UpdateField {
					Flags = UpdateFieldFlags.GroupOnly,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_25_1",
					Offset = 287,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_25_2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_25_2",
					Offset = 288,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.QUEST_LOG_25_3
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_25_3",
					Offset = 289,
					Size = 2,
					Type = UpdateFieldType.TwoInt16
				},
				null,
				// PlayerFields.QUEST_LOG_25_5
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "QUEST_LOG_25_5",
					Offset = 291,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_1_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_1_ENTRYID",
					Offset = 292,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_1_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_1_ENCHANTMENT",
					Offset = 293,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_2_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_2_ENTRYID",
					Offset = 294,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_2_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_2_ENCHANTMENT",
					Offset = 295,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_3_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_3_ENTRYID",
					Offset = 296,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_3_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_3_ENCHANTMENT",
					Offset = 297,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_4_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_4_ENTRYID",
					Offset = 298,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_4_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_4_ENCHANTMENT",
					Offset = 299,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_5_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_5_ENTRYID",
					Offset = 300,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_5_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_5_ENCHANTMENT",
					Offset = 301,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_6_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_6_ENTRYID",
					Offset = 302,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_6_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_6_ENCHANTMENT",
					Offset = 303,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_7_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_7_ENTRYID",
					Offset = 304,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_7_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_7_ENCHANTMENT",
					Offset = 305,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_8_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_8_ENTRYID",
					Offset = 306,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_8_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_8_ENCHANTMENT",
					Offset = 307,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_9_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_9_ENTRYID",
					Offset = 308,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_9_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_9_ENCHANTMENT",
					Offset = 309,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_10_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_10_ENTRYID",
					Offset = 310,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_10_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_10_ENCHANTMENT",
					Offset = 311,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_11_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_11_ENTRYID",
					Offset = 312,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_11_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_11_ENCHANTMENT",
					Offset = 313,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_12_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_12_ENTRYID",
					Offset = 314,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_12_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_12_ENCHANTMENT",
					Offset = 315,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_13_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_13_ENTRYID",
					Offset = 316,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_13_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_13_ENCHANTMENT",
					Offset = 317,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_14_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_14_ENTRYID",
					Offset = 318,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_14_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_14_ENCHANTMENT",
					Offset = 319,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_15_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_15_ENTRYID",
					Offset = 320,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_15_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_15_ENCHANTMENT",
					Offset = 321,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_16_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_16_ENTRYID",
					Offset = 322,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_16_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_16_ENCHANTMENT",
					Offset = 323,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_17_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_17_ENTRYID",
					Offset = 324,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_17_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_17_ENCHANTMENT",
					Offset = 325,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_18_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_18_ENTRYID",
					Offset = 326,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_18_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_18_ENCHANTMENT",
					Offset = 327,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.VISIBLE_ITEM_19_ENTRYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_19_ENTRYID",
					Offset = 328,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.VISIBLE_ITEM_19_ENCHANTMENT
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "VISIBLE_ITEM_19_ENCHANTMENT",
					Offset = 329,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.CHOSEN_TITLE
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "CHOSEN_TITLE",
					Offset = 330,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.FAKE_INEBRIATION
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Player,
					Name = "FAKE_INEBRIATION",
					Offset = 331,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.INV_SLOT_HEAD
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "INV_SLOT_HEAD",
					Offset = 332,
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
					Offset = 378,
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
					Offset = 410,
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
					Offset = 466,
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
					Offset = 480,
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
					Offset = 504,
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
					Offset = 568,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// PlayerFields._FIELD_KNOWN_TITLES
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "_FIELD_KNOWN_TITLES",
					Offset = 570,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// PlayerFields._FIELD_KNOWN_TITLES1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "_FIELD_KNOWN_TITLES1",
					Offset = 572,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// PlayerFields._FIELD_KNOWN_TITLES2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "_FIELD_KNOWN_TITLES2",
					Offset = 574,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// PlayerFields.XP
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "XP",
					Offset = 576,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.NEXT_LEVEL_XP
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "NEXT_LEVEL_XP",
					Offset = 577,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.SKILL_INFO_1_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "SKILL_INFO_1_1",
					Offset = 578,
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
				// PlayerFields.CHARACTER_POINTS1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "CHARACTER_POINTS1",
					Offset = 962,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.CHARACTER_POINTS2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "CHARACTER_POINTS2",
					Offset = 963,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.TRACK_CREATURES
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "TRACK_CREATURES",
					Offset = 964,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.TRACK_RESOURCES
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "TRACK_RESOURCES",
					Offset = 965,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.BLOCK_PERCENTAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "BLOCK_PERCENTAGE",
					Offset = 966,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.DODGE_PERCENTAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "DODGE_PERCENTAGE",
					Offset = 967,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.PARRY_PERCENTAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "PARRY_PERCENTAGE",
					Offset = 968,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.EXPERTISE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "EXPERTISE",
					Offset = 969,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.OFFHAND_EXPERTISE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "OFFHAND_EXPERTISE",
					Offset = 970,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.CRIT_PERCENTAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "CRIT_PERCENTAGE",
					Offset = 971,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.RANGED_CRIT_PERCENTAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "RANGED_CRIT_PERCENTAGE",
					Offset = 972,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.OFFHAND_CRIT_PERCENTAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "OFFHAND_CRIT_PERCENTAGE",
					Offset = 973,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.SPELL_CRIT_PERCENTAGE1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "SPELL_CRIT_PERCENTAGE1",
					Offset = 974,
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
					Offset = 981,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.SHIELD_BLOCK_CRIT_PERCENTAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "SHIELD_BLOCK_CRIT_PERCENTAGE",
					Offset = 982,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.MASTERY
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MASTERY",
					Offset = 983,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.EXPLORED_ZONES_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "EXPLORED_ZONES_1",
					Offset = 984,
					Size = 128,
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
				// PlayerFields.REST_STATE_EXPERIENCE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "REST_STATE_EXPERIENCE",
					Offset = 1112,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.COINAGE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "COINAGE",
					Offset = 1113,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.MOD_DAMAGE_DONE_POS
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_DAMAGE_DONE_POS",
					Offset = 1114,
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
					Offset = 1121,
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
					Offset = 1128,
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
					Offset = 1135,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.MOD_HEALING_PCT
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_HEALING_PCT",
					Offset = 1136,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.MOD_HEALING_DONE_PCT
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_HEALING_DONE_PCT",
					Offset = 1137,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.MOD_SPELL_POWER_PCT
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_SPELL_POWER_PCT",
					Offset = 1138,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// PlayerFields.MOD_TARGET_RESISTANCE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_TARGET_RESISTANCE",
					Offset = 1139,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.MOD_TARGET_PHYSICAL_RESISTANCE
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MOD_TARGET_PHYSICAL_RESISTANCE",
					Offset = 1140,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.PLAYER_FIELD_BYTES
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "PLAYER_FIELD_BYTES",
					Offset = 1141,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// PlayerFields.SELF_RES_SPELL
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "SELF_RES_SPELL",
					Offset = 1142,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.PVP_MEDALS
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "PVP_MEDALS",
					Offset = 1143,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.BUYBACK_PRICE_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "BUYBACK_PRICE_1",
					Offset = 1144,
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
					Offset = 1156,
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
					Offset = 1168,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// PlayerFields.TODAY_CONTRIBUTION
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "TODAY_CONTRIBUTION",
					Offset = 1169,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.YESTERDAY_CONTRIBUTION
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "YESTERDAY_CONTRIBUTION",
					Offset = 1170,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.LIFETIME_HONORBALE_KILLS
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "LIFETIME_HONORBALE_KILLS",
					Offset = 1171,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.PLAYER_FIELD_BYTES2
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "PLAYER_FIELD_BYTES2",
					Offset = 1172,
					Size = 1,
					Type = UpdateFieldType.Unk322
				},
				// PlayerFields.WATCHED_FACTION_INDEX
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "WATCHED_FACTION_INDEX",
					Offset = 1173,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.COMBAT_RATING_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "COMBAT_RATING_1",
					Offset = 1174,
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
					Offset = 1200,
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
				// PlayerFields.HONOR_CURRENCY
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "HONOR_CURRENCY",
					Offset = 1221,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.ARENA_CURRENCY
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "ARENA_CURRENCY",
					Offset = 1222,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.MAX_LEVEL
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "MAX_LEVEL",
					Offset = 1223,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.DAILY_QUESTS_1
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "DAILY_QUESTS_1",
					Offset = 1224,
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
					Offset = 1249,
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
					Offset = 1253,
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
					Offset = 1256,
					Size = 6,
					Type = UpdateFieldType.UInt32
				},
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
					Offset = 1262,
					Size = 6,
					Type = UpdateFieldType.UInt32
				},
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
					Offset = 1268,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// PlayerFields.PET_SPELL_POWER
				new UpdateField {
					Flags = UpdateFieldFlags.Private,
					Group = ObjectTypeId.Player,
					Name = "PET_SPELL_POWER",
					Offset = 1269,
					Size = 1,
					Type = UpdateFieldType.UInt32
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
				// GameObjectFields.OBJECT_FIELD_CREATED_BY
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.GameObject,
					Name = "OBJECT_FIELD_CREATED_BY",
					Offset = 6,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// GameObjectFields.DISPLAYID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.GameObject,
					Name = "DISPLAYID",
					Offset = 8,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// GameObjectFields.FLAGS
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.GameObject,
					Name = "FLAGS",
					Offset = 9,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// GameObjectFields.PARENTROTATION
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.GameObject,
					Name = "PARENTROTATION",
					Offset = 10,
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
					Offset = 14,
					Size = 1,
					Type = UpdateFieldType.TwoInt16
				},
				// GameObjectFields.FACTION
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.GameObject,
					Name = "FACTION",
					Offset = 15,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// GameObjectFields.LEVEL
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.GameObject,
					Name = "LEVEL",
					Offset = 16,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// GameObjectFields.BYTES_1
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.GameObject,
					Name = "BYTES_1",
					Offset = 17,
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
				// DynamicObjectFields.CASTER
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.DynamicObject,
					Name = "CASTER",
					Offset = 6,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// DynamicObjectFields.BYTES
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.DynamicObject,
					Name = "BYTES",
					Offset = 8,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// DynamicObjectFields.SPELLID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.DynamicObject,
					Name = "SPELLID",
					Offset = 9,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// DynamicObjectFields.RADIUS
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.DynamicObject,
					Name = "RADIUS",
					Offset = 10,
					Size = 1,
					Type = UpdateFieldType.Float
				},
				// DynamicObjectFields.CASTTIME
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.DynamicObject,
					Name = "CASTTIME",
					Offset = 11,
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
				// CorpseFields.OWNER
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Corpse,
					Name = "OWNER",
					Offset = 6,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// CorpseFields.PARTY
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Corpse,
					Name = "PARTY",
					Offset = 8,
					Size = 2,
					Type = UpdateFieldType.Guid
				},
				null,
				// CorpseFields.DISPLAY_ID
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Corpse,
					Name = "DISPLAY_ID",
					Offset = 10,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// CorpseFields.ITEM
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Corpse,
					Name = "ITEM",
					Offset = 11,
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
					Offset = 30,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// CorpseFields.BYTES_2
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Corpse,
					Name = "BYTES_2",
					Offset = 31,
					Size = 1,
					Type = UpdateFieldType.ByteArray
				},
				// CorpseFields.GUILD
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Corpse,
					Name = "GUILD",
					Offset = 32,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// CorpseFields.FLAGS
				new UpdateField {
					Flags = UpdateFieldFlags.Public,
					Group = ObjectTypeId.Corpse,
					Name = "FLAGS",
					Offset = 33,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// CorpseFields.DYNAMIC_FLAGS
				new UpdateField {
					Flags = UpdateFieldFlags.Dynamic,
					Group = ObjectTypeId.Corpse,
					Name = "DYNAMIC_FLAGS",
					Offset = 34,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
				// CorpseFields.PAD
				new UpdateField {
					Flags = UpdateFieldFlags.None,
					Group = ObjectTypeId.Corpse,
					Name = "PAD",
					Offset = 35,
					Size = 1,
					Type = UpdateFieldType.UInt32
				},
			},
			#endregion

		};

	}

}

