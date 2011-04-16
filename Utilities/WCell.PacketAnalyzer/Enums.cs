using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.PacketAnalysis
{
	public enum SimpleType : byte
	{
		Byte,
		//SByte,
		UShort,
		Short,
		UInt,
		Int,
		ULong,
		Long,
		Float,
		Vector3,
        PackedVector3,
		Vector4,
		Guid,
		PackedGuid,
		/// <summary>
		/// 0-terminated string
		/// </summary>
		CString,
		/// <summary>
		/// String that is preceeded with its length in one byte
		/// </summary>
		PascalStringByte,
		/// <summary>
		/// String that is preceeded with its length in a UShort
		/// </summary>
		PascalStringUShort,
		/// <summary>
		/// String that is preceeded with its length in a UInt
		/// </summary>
		PascalStringUInt,

        PackedDate,
        UnixTime,
		Count,

        

		NotSimple = 0xFF
	}

	public enum PacketSegmentStructureType
	{
		Simple,
		Complex,
		List,
		PacketSegmentType,
		Switch,
		Count
	}
}