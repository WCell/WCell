using System;

namespace WCell.Constants.GameObjects
{
	public static class GOConstants
	{
		public const int EntryFieldCount = 32;
		public const int MaxRotations = 4;
	}

    public enum GameObjectState
    {
        Disabled = 0x00,
        Enabled = 0x01,
		Destroyed = 0x02
    }

    [Flags]
    public enum GameObjectFlags // :ushort
    {
		None,
        /// <summary>
        /// 0x1
        /// Disables interaction while animated
        /// </summary>
        InUse = 0x01,
        /// <summary>
        /// 0x2
        /// Requires a key, spell, event, etc to be opened. 
        /// Makes "Locked" appear in tooltip
        /// </summary>
        Locked = 0x02,
        /// <summary>
        /// 0x4
        /// Objects that require a condition to be met before they are usable
        /// </summary>
        ConditionalInteraction = 0x04,
        /// <summary>
        /// 0x8
        /// any kind of transport? Object can transport (elevator, boat, car)
        /// </summary>
        Transport = 0x08,
        GOFlag_0x10 = 0x10,
        /// <summary>
        /// 0x20
        /// These objects never de-spawn, but typically just change state in response to an event
        /// Ex: doors
        /// </summary>
        DoesNotDespawn = 0x20,
        /// <summary>
        /// 0x40
        /// Typically, summoned objects. Triggered by spell or other events
        /// </summary>
        Triggered = 0x40,

        GOFlag_0x80 = 0x80,
        GOFlag_0x100 = 0x100,
		/// <summary>
		/// Use for GO type 33
		/// </summary>
        Damaged = 0x200,
        Destroyed = 0x400,

        GOFlag_0x800 = 0x800,
        GOFlag_0x1000 = 0x1000,
        GOFlag_0x2000 = 0x2000,
        GOFlag_0x4000 = 0x4000,
        GOFlag_0x8000 = 0x8000,

        Flag_0x10000 = 0x10000,
        Flag_0x20000 = 0x20000,
        Flag_0x40000 = 0x40000,
    }

    [Flags]
    public enum GameObjectDynamicFlagsLow
    {
        Deactivated = 0x00,
        Activated = 0x01,
    }

    [Flags]
    public enum GameObjectDynamicFlagsHigh
    {

    }
}