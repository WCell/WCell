using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.MPQTool.DBC.Compare
{
	[Flags]
	public enum CompareResultType
	{
		Identical = 0x00000000,

		/// <summary>
		/// Amount of columns changed.
		/// Structural change.
		/// </summary>
		ColCount = 0x0000001,

		/// <summary>
		/// The length of a single row changed.
		/// Structural change.
		/// </summary>
		RowLength = 0x00000002,

		/// <summary>
		/// Content change (nothing that requires software updates)
		/// </summary>
		RowCount = 0x0000004,

		/// <summary>
		/// New file
		/// </summary>
		New = 0x00008,

		/// <summary>
		/// Deprecated file (not existant anymore)
		/// </summary>
		Deprecated = 0x000010
	}
}