using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Data
{
	/// <summary>
	/// Marks a Type to be persistent.
	/// Each implementing class can have an optional static method
	/// <![CDATA[IEnumerable<IDataHolder>GetAllDataHolders()]]>
	/// that is used for caching
	/// </summary>
	public interface IDataHolder
	{
		void FinalizeDataHolder();
	}

	public abstract class DataHolderBase : IDataHolder
	{
		public abstract void FinalizeDataHolder();
	}
}