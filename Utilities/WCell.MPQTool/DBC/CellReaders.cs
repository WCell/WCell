using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.MPQTool.DBC
{
	public delegate int CellReader(DBCReader reader, byte[] bytes, uint index, out object value);
}