using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;

namespace WCell.Core.Database
{
	public class WCellRecord<T> : ActiveRecordBase<T> where T : ActiveRecordBase
	{
		public bool New
		{
			get;
			set;
		}


		#region Overrides
		public override void Save()
		{
			if (New)
			{
				Create();
			}
			else
			{
				Update();
			}
		}

		public override void Create()
		{
			base.Create();
			New = false;
		}

		public override void Delete()
		{
			if (!New)
			{
				base.Delete();
			}
		}
		public override void SaveAndFlush()
		{
			if (New)
			{
				CreateAndFlush();
			}
			else
			{
				UpdateAndFlush();
			}
		}

		public override void CreateAndFlush()
		{
			base.CreateAndFlush();
			New = false;
		}

		public override void DeleteAndFlush()
		{
			if (!New)
			{
				base.DeleteAndFlush();
			}
		}
		#endregion
	}
}
