using System.Runtime.InteropServices;
using WCell.Constants.Pets;
using WCell.Constants.Spells;

namespace WCell.Constants.Pets
{
	[StructLayout(LayoutKind.Explicit, Size = 4)]
	public struct PetActionEntry
	{
		private const uint ActionMask = 0x00FFFFFF;
		private const uint TypeMask = 0xFF000000;

		[FieldOffset(0)]
		public uint Raw;

		[FieldOffset(0)]
		private uint m_ActionId;

		[FieldOffset(3)]
		public PetActionType Type;

		public PetActionEntry(uint raw)
		{
			Type = 0;
			m_ActionId = 0;
			Raw = raw;
		}

		int ActionId
		{
			get { return (int)(m_ActionId & ActionMask); }
			set
			{
				m_ActionId &= ~ActionMask;
				m_ActionId |= ((uint)value & ActionMask);
			}
		}

		public PetAction Action
		{
			get { return (PetAction)ActionId; }
			set { ActionId = (int)value; }
		}

		public SpellId SpellId
		{
			get { return (SpellId)ActionId; }
			set { ActionId = (int)value; }
		}

		public PetAttackMode AttackMode
		{
			get { return (PetAttackMode)((byte)ActionId); }
			set { ActionId = (int)value; }
		}

		public bool IsAutoCastEnabled
		{
			get
			{
				return (Type & PetActionType.IsAutoCastEnabled) != 0;
			}
			set
			{
				if (value)
				{
					Type |= PetActionType.IsAutoCastEnabled;
				}
				else
				{
					Type &= ~PetActionType.IsAutoCastEnabled;
				}
			}
		}

		public bool IsAutoCastAllowed
		{
			get
			{
				return (Type & PetActionType.IsAutoCastAllowed) != 0;
			}
			set
			{
				if (value)
				{
					Type |= PetActionType.IsAutoCastAllowed;
				}
				else
				{
					Type &= ~PetActionType.IsAutoCastAllowed;
				}
			}
		}

		public override string ToString()
		{
			return Type + ": " + m_ActionId;
		}

		public static implicit operator PetActionEntry(uint data)
		{
			return new PetActionEntry
			{
				ActionId = (int)GetActionId(data),
				Type = GetType(data)
			};
		}

		public static implicit operator uint(PetActionEntry entry)
		{
			return entry.Raw;
		}

		public static uint GetActionId(uint data)
		{
			return data & 0x00FFFFFF;
		}

		public static PetActionType GetType(uint data)
		{
			return (PetActionType)((data & 0xFF000000) >> 24);
		}
	}
}