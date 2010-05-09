using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Misc
{
	/// <summary>
	/// Contains custom data to be appended to existing objects.
	/// Allows addons to attach custom data to core-defined classes.
	/// </summary>
	public struct DataAttachment
	{
		private Dictionary<Type, object> _dataMap;

		public D AttachData<D>()
			where D : class, new()
		{
			if (_dataMap == null)
			{
				_dataMap = new Dictionary<Type, object>(3);
			}
			var data = new D();
			_dataMap[typeof(D)] = data;
			return data;
		}

		public void AttachData<D>(D data)
			where D : class
		{
			if (_dataMap == null)
			{
				_dataMap = new Dictionary<Type, object>(3);
			}
			_dataMap[typeof(D)] = data;
		}

		public D GetData<D>()
			where D : class
		{
			if (_dataMap == null)
			{
				return null;
			}
			return _dataMap[typeof(D)] as D;
		}
	}
}
