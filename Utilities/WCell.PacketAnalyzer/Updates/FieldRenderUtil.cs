using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.Util;

namespace WCell.PacketAnalysis.Updates
{
    /// <summary>
    /// Returns the amount of fields rendered (one field is 4 bytes)
    /// </summary>
    public delegate uint RenderHandler(FieldRenderInfo renderer, byte[] value, out string strVal);

    public static class FieldRenderUtil
    {
        public static readonly Dictionary<HighId, ObjectTypeId> HighIdMap = new Dictionary<HighId, ObjectTypeId>();
        //public static readonly Dictionary<OldHighId, ObjectTypeId> OldHighIdMap = new Dictionary<OldHighId, ObjectTypeId>();
        public static readonly Dictionary<ObjectTypeId, Type> EnumTypeMap = new Func<Dictionary<ObjectTypeId, Type>>(() =>
        {
            var map = new Dictionary<ObjectTypeId, Type>();

            map.Add(ObjectTypeId.Object, typeof(ObjectFields));
            map.Add(ObjectTypeId.Unit, typeof(UnitFields));
            map.Add(ObjectTypeId.Player, typeof(PlayerFields));

            map.Add(ObjectTypeId.Item, typeof(ItemFields));
            map.Add(ObjectTypeId.Container, typeof(ContainerFields));

            map.Add(ObjectTypeId.GameObject, typeof(GameObjectFields));
            map.Add(ObjectTypeId.DynamicObject, typeof(DynamicObjectFields));
            map.Add(ObjectTypeId.Corpse, typeof(CorpseFields));

            return map;
        })();

        public static readonly Dictionary<ObjectTypeId, FieldRenderer> FieldRenderers = new Dictionary<ObjectTypeId, FieldRenderer>();

        public static readonly Dictionary<ObjectTypeId, object[]> FieldValues = new Dictionary<ObjectTypeId, object[]>();

        public static readonly Dictionary<object, RenderHandler> CustomRenderers = new Dictionary<object, RenderHandler>();

        public static readonly RenderHandler[] TypeRenderers
            = new RenderHandler[(int)Utility.GetMaxEnum<UpdateFieldType>() + 1];

        public static bool Initialized = false;

        [Initialization(InitializationPass.Third)]
        public static void Init()
        {
            if (!Initialized)
            {
                Initialized = true;
                InitRenderers();
                InitValuesAndTypes();
                InitArrays();
            }
        }

        public static uint GetSizeof(UpdateFieldType type)
        {
            if (//type == UpdateFieldType.UInt64 ||
                type == UpdateFieldType.Guid)
            {
                return 2;
            }
            return 1;
        }

        public static object[] GetValues(ObjectTypeId objectType)
        {
            return FieldValues[objectType];
        }

        /// <summary>
        /// Sets the given RenderHandler for startField and fieldCount fields after it
        /// </summary>
        public static void SetRenderer(object startField, uint fieldCount, RenderHandler renderer)
        {
            Set(CustomRenderers, renderer, startField, fieldCount);
        }

        private static void Set<T>(Dictionary<object, T> map, T val, object startField, uint fieldCount)
        {
            var values = Enum.GetValues(startField.GetType());

            var set = false;
            var i = 0;
            foreach (var field in values)
            {
                if (field.Equals(startField))
                {
                    set = true;
                }

                if (set)
                {
                    map[field] = val;

                    if (++i == fieldCount)
                    {
                        break;
                    }
                }
            }
        }

        public static string GetFriendlyName(ObjectTypeId type, uint field)
        {
            var infos = FieldRenderers[type];
            if (infos == null)
            {
                throw new Exception("Invalid ObjectTypeId: " + type);
            }

            var info = infos.GetFieldInfo(field);
            if (info == null)
            {
                throw new Exception(string.Format("Invalid Field " + field + " for Type " + type));
            }
            return info.Name.ToFriendlyName();
        }

        public static string GetFriendlyName(object field)
        {
            return field.ToString().ToFriendlyName();
        }

        #region Rendering

        private static void InitRenderers()
        {
            TypeRenderers[(int)UpdateFieldType.None] = RenderDefault;
            TypeRenderers[(int)UpdateFieldType.UInt32] = RenderUInt32;
            //TypeRenderers[(int)UpdateFieldType.UInt64] = RenderUInt64;
            TypeRenderers[(int)UpdateFieldType.TwoInt16] = RenderTwoInt16;
            TypeRenderers[(int)UpdateFieldType.Float] = RenderFloat;
            TypeRenderers[(int)UpdateFieldType.ByteArray] = RenderByteArray;

            TypeRenderers[(int)UpdateFieldType.Guid] = RenderGUID;
        }

        public static uint RenderDefault(FieldRenderInfo field, byte[] values, out string strVal)
        {
            return RenderUInt32(field, values, out strVal);
        }

        public static uint RenderUInt32(FieldRenderInfo field, byte[] values, out string strVal)
        {
            var val = values.GetUInt32(field.Index);
            strVal = val.ToString();
            return 1;
        }

        public static uint RenderFloat(FieldRenderInfo field, byte[] values, out string strVal)
        {
            float val = values.GetFloat(field.Index);
            strVal = val.ToString();
            return 1;
        }

        public static uint RenderTwoInt16(FieldRenderInfo field, byte[] values, out string strVal)
        {
            byte[] rawField = values.GetBytes(field.Index, 4);
            var val1 = BitConverter.ToUInt16(rawField, 0);
            var val2 = BitConverter.ToUInt16(rawField, 2);
            //var val1 = values.GetUInt16(field.Index);
            //var val2 = values.GetUInt16(field.Index + 2);

            strVal = "Low: " + val1 + ", High: " + val2;
            return 1;
        }

        public static uint RenderByteArray(FieldRenderInfo field, byte[] values, out string strVal)
        {
            var bytes = values.GetBytes(field.Index, 4);

            string[] s = new string[4];
            for (int i = 0; i < s.Length; i++)
            {
                s[i] = string.Format("{0:X2}", bytes[i]);
            }

            strVal = s.ToString(", ");
            return 1;
        }

        public static uint RenderUInt64(FieldRenderInfo field, byte[] values, out string strVal)
        {
            var val = values.GetUInt64(field.Index);
            strVal = val.ToString();
            return 2;
        }

        public static uint RenderGUID(FieldRenderInfo field, byte[] values, out string strVal)
        {
            var val = values.GetUInt64(field.Index);
            strVal = new EntityId(val).ToString();
            return 2;
        }

        #endregion Rendering

        #region Misc

        private static void InitArrays()
        {
            HighIdMap.Add(HighId.Item, ObjectTypeId.Item);
            HighIdMap.Add(HighId.Player, ObjectTypeId.Player);
            HighIdMap.Add(HighId.GameObject, ObjectTypeId.GameObject);
            HighIdMap.Add(HighId.Transport, ObjectTypeId.GameObject);

            HighIdMap.Add(HighId.Pet, ObjectTypeId.Unit);
            HighIdMap.Add(HighId.DynamicObject, ObjectTypeId.DynamicObject);
            HighIdMap.Add(HighId.DynamicObject_2, ObjectTypeId.DynamicObject);
            HighIdMap.Add(HighId.Corpse, ObjectTypeId.Corpse);
            HighIdMap.Add(HighId.MoTransport, ObjectTypeId.GameObject);

            // Add Plain Units
            HighIdMap.Add(HighId.Unit, ObjectTypeId.Unit);
            HighIdMap.Add(HighId.Unit_F4, ObjectTypeId.Unit);
            HighIdMap.Add(HighId.Unit_F7, ObjectTypeId.Unit);

            // Add Vehicle Units
            HighIdMap.Add(HighId.Unit_F1_Vehicle, ObjectTypeId.Unit);
            HighIdMap.Add(HighId.Unit_F4_Vehicle, ObjectTypeId.Unit);
            HighIdMap.Add(HighId.Unit_F7_Vehicle, ObjectTypeId.Unit);

            HighIdMap.Add(HighId.GameObject_F4, ObjectTypeId.GameObject);
            HighIdMap.Add(HighId.GameObject_F7, ObjectTypeId.GameObject);
            HighIdMap.Add(HighId.GameObject4, ObjectTypeId.GameObject);

            HighIdMap.Add(HighId.Corpse2, ObjectTypeId.Corpse);
            HighIdMap.Add(HighId.Corpse3, ObjectTypeId.Corpse);

            //OldHighIdMap.Add(OldHighId.Item, ObjectTypeId.Item);
            //OldHighIdMap.Add(OldHighId.Container, ObjectTypeId.Container);
            //OldHighIdMap.Add(OldHighId.Player, ObjectTypeId.Player);
            //OldHighIdMap.Add(OldHighId.GameObject, ObjectTypeId.GameObject);
            //OldHighIdMap.Add(OldHighId.Transport, ObjectTypeId.GameObject);
            //OldHighIdMap.Add(OldHighId.Unit, ObjectTypeId.Unit);
            //OldHighIdMap.Add(OldHighId.DynamicObject, ObjectTypeId.DynamicObject);
            //OldHighIdMap.Add(OldHighId.Corpse, ObjectTypeId.Corpse);

            FieldRenderers.Add(ObjectTypeId.Object, CreateRenderer(ObjectTypeId.Object));
            FieldRenderers.Add(ObjectTypeId.Item, CreateRenderer(ObjectTypeId.Item));
            FieldRenderers.Add(ObjectTypeId.Container, CreateRenderer(ObjectTypeId.Container));
            FieldRenderers.Add(ObjectTypeId.Player, CreateRenderer(ObjectTypeId.Player));
            FieldRenderers.Add(ObjectTypeId.GameObject, CreateRenderer(ObjectTypeId.GameObject));
            FieldRenderers.Add(ObjectTypeId.Unit, CreateRenderer(ObjectTypeId.Unit));
            FieldRenderers.Add(ObjectTypeId.DynamicObject, CreateRenderer(ObjectTypeId.DynamicObject));
            FieldRenderers.Add(ObjectTypeId.Corpse, CreateRenderer(ObjectTypeId.Corpse));
        }

        private static FieldRenderer CreateRenderer(ObjectTypeId enumType)
        {
            return new FieldRenderer(enumType);
        }

        public static FieldRenderer GetRenderer(ObjectTypeId objectType)
        {
            return FieldRenderers[objectType];
        }

        #endregion Misc

        private static void InitValuesAndTypes()
        {
            AddValues(ObjectTypeId.Object, ObjectTypeId.None);

            AddValues(ObjectTypeId.Item, ObjectTypeId.Object);
            AddValues(ObjectTypeId.Container, ObjectTypeId.Item);

            AddValues(ObjectTypeId.Unit, ObjectTypeId.Object);
            AddValues(ObjectTypeId.Player, ObjectTypeId.Unit);

            AddValues(ObjectTypeId.GameObject, ObjectTypeId.Object);
            AddValues(ObjectTypeId.DynamicObject, ObjectTypeId.Object);
            AddValues(ObjectTypeId.Corpse, ObjectTypeId.Object);

            InitCustomRenderers();
        }

        private static void AddValues(ObjectTypeId objectType, ObjectTypeId includedEnumType)
        {
            object[] included;
            if (includedEnumType != ObjectTypeId.None)
            {
                included = FieldValues[includedEnumType];
            }
            else
            {
                included = null;
            }

            var arr = Enum.GetValues(EnumTypeMap[objectType]);
            var obj = new object[arr.Length + (included != null ? included.Length : 0)];
            int i = 0;

            if (included != null)
            {
                foreach (var val in included)
                {
                    obj[i++] = val;
                }
            }
            foreach (var val in arr)
            {
                obj[i++] = val;
            }

            FieldValues[objectType] = obj;
        }

        #region Custom Renderers

        private static void InitCustomRenderers()
        {
            SetupObjectRenderers();
            SetupItemRenderers();
            SetupPlayerRenderers();
            SetupGameObjectRenderers();
            SetupUnitRenderers();
            SetupDynamicObjectRenderers();
            SetupCorpseRenderers();
        }

        private static void SetupObjectRenderers()
        {
            CustomRenderers[ObjectFields.TYPE] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                strVal = ((ObjectTypes)value.GetUInt32(renderer.Index)).ToString();
                return 1;
            };
        }

        private static void SetupItemRenderers()
        {
            CustomRenderers[ItemFields.FLAGS] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                strVal = ((ItemFlags)value.GetUInt32(renderer.Index)).ToString();
                return 1;
            };
        }

        #region Unit Renderers

        private static void SetupUnitRenderers()
        {
            CustomRenderers[UnitFields.FACTIONTEMPLATE] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                strVal = ((FactionTemplateId)value.GetUInt32(renderer.Index)).ToString();
                return 1;
            };

            CustomRenderers[UnitFields.FLAGS] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                strVal = ((UnitFlags)value.GetUInt32(renderer.Index)).ToString();
                return 1;
            };

            CustomRenderers[UnitFields.FLAGS_2] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                strVal = ((UnitFlags2)value.GetUInt32(renderer.Index)).ToString();
                return 1;
            };

            CustomRenderers[UnitFields.BYTES_0] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                var val = value.GetBytes(renderer.Index, 4);
                var str = new List<string>();
                str.Add("Race: " + (RaceId)val[0]);
                str.Add("Class: " + (ClassId)val[1]);
                str.Add("Gender: " + (GenderType)val[2]);
                str.Add("PowerType: " + (PowerType)val[3]);
                strVal = str.ToString(", ");
                return 1;
            };

            CustomRenderers[UnitFields.NPC_FLAGS] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                var val = (NPCFlags)value.GetUInt32(renderer.Index);
                strVal = val.ToString();
                return 1;
            };

            CustomRenderers[UnitFields.BYTES_1] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                var val = value.GetBytes(renderer.Index, 4);
                var str = new List<string>();
                str.Add("StandState: " + (StandState)val[0]);
                str.Add("PetTalentPoints: " + val[1]);
                str.Add("StateFlag: " + (StateFlag)val[2]);
                str.Add("UFB_1_4: " + val[3]);
                strVal = str.ToString(", ");
                return 1;
            };

            CustomRenderers[UnitFields.BYTES_2] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                var val = value.GetBytes(renderer.Index, 4);
                var str = new List<string>();
                str.Add("SheathType: " + (SheathType)val[0]);
                str.Add("PVP State: " + (PvPState)val[1]);
                str.Add("PetState: " + (PetState)val[2]);
                str.Add("ShapeShift: " + (ShapeshiftForm)val[3]);
                strVal = str.ToString(", ");
                return 1;
            };

            CustomRenderers[UnitFields.AURASTATE] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                var val = (AuraStateMask)value.GetUInt32(renderer.Index);
                strVal = val.ToString();
                return 1;
            };

            CustomRenderers[UnitFields.DYNAMIC_FLAGS] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                strVal = ((UnitDynamicFlags)value.GetUInt32(renderer.Index)).ToString();
                return 1;
            };
        }

        #endregion Unit Renderers

        #region Player Renderers

        private static void SetupPlayerRenderers()
        {
            CustomRenderers[PlayerFields.FLAGS] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                strVal = ((PlayerFlags)value.GetUInt32(renderer.Index)).ToString();
                return 1;
            };

            CustomRenderers[PlayerFields.BYTES] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                var val = value.GetBytes(renderer.Index, 4);
                List<string> str = new List<string>();

                str.Add("Skin: " + val[0]);
                str.Add("Face: " + val[1]);
                str.Add("HairStyle: " + val[2]);
                str.Add("HairColor: " + val[3]);
                strVal = str.ToString(", ");
                return 1;
            };

            CustomRenderers[PlayerFields.BYTES_2] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                var val = value.GetBytes(renderer.Index, 4);
                List<string> str = new List<string>();

                str.Add("FacialHair: " + val[0]);
                str.Add("PB2_2: " + val[1]);
                str.Add("Bankbag Slots: " + val[2]);
                str.Add("RestState: " + (RestState)val[3]);
                strVal = str.ToString(", ");
                return 1;
            };

            CustomRenderers[PlayerFields.BYTES_3] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                var val = value.GetBytes(renderer.Index, 4);
                List<string> str = new List<string>();

                str.Add("Gender: " + (GenderType)val[0]);
                str.Add("Drunkeness: " + val[1]);
                str.Add("PB3_3: " + val[2]);
                str.Add("PvPRank: " + val[3]);
                strVal = str.ToString(", ");
                return 1;
            };

            CustomRenderers[PlayerFields.PLAYER_FIELD_BYTES] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                var val = value.GetBytes(renderer.Index, 4);
                List<string> str = new List<string>();

                str.Add("PFB_1: " + val[0]);
                str.Add("PFB_2: " + val[1]);
                str.Add("PFB_3: " + val[2]);
                str.Add("PFB_4: " + val[3]);
                strVal = str.ToString(", ");
                return 1;
            };

            CustomRenderers[PlayerFields.PLAYER_FIELD_BYTES2] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                var val = value.GetBytes(renderer.Index, 4);
                var str = new List<string>
				          	{
				          		"PFB2_1: " + val[0],
								"PFB2_2: " + val[1],
								"PFB2_3: " + val[2],
								"PFB2_4: " + val[3]
				          	};

                strVal = str.ToString(", ");
                return 1;
            };

            //var mod = (int)(PlayerFields.SKILL_INFO_1_1)%2;
            SetRenderer(PlayerFields.SKILL_INFO_1_1, 384, (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                var i = renderer.Index * 4;
                var skillId = (SkillId)value.GetUInt16AtByte(i);
                var abandonable = value.GetUInt16AtByte(i += 2) != 0;
                var current = value.GetUInt16AtByte(i += 2);
                var max = value.GetUInt16AtByte(i);

                strVal = string.Format("{0} (#{1}{2}) - {3}/{4}", skillId, (int)skillId, abandonable ? " /CanAbandon" : "", current, max);
                return 2;
            });
        }

        #endregion Player Renderers

        private static void SetupGameObjectRenderers()
        {
            CustomRenderers[GameObjectFields.BYTES_1] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                var val = value.GetBytes(renderer.Index, 4);
                var str = new List<string>
                          	{
                          		"State: " + val[0],
                          		"TypeId: " + (GameObjectType) val[1],
                          		"ArtKit: " + val[2],
                          		"AnimProgress: " + val[3]
                          	};
                strVal = str.ToString(", ");
                return 1;
            };
        }

        private static void SetupDynamicObjectRenderers()
        {
        }

        private static void SetupCorpseRenderers()
        {
            CustomRenderers[CorpseFields.DYNAMIC_FLAGS] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                strVal = ((CorpseDynamicFlags)value.GetUInt32(renderer.Index)).ToString();
                return 1;
            };

            CustomRenderers[CorpseFields.FLAGS] = (FieldRenderInfo renderer, byte[] value, out string strVal) =>
            {
                strVal = ((CorpseFlags)value.GetUInt32(renderer.Index)).ToString();
                return 1;
            };
        }

        #endregion Custom Renderers

        public static string GetFieldStr(ObjectTypeId typeId, int index)
        {
            var renderer = FieldRenderers[typeId];
            var fieldInfo = renderer.GetFieldInfo((uint)index);
            return string.Format("{0} ({1})", fieldInfo.Name, fieldInfo.Index);
        }
    }
}