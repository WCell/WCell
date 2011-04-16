using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Data
{
	public delegate object GetDataHolderHandler(object id);

	public delegate void SetDataHolderHandler(object holder);
}