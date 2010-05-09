//
// Author: James Nies
// Date: 3/22/2005
// Description: Class for testing the PropertyAccessor.
//
// *** This code was written by James Nies and has been provided to you, ***
// *** free of charge, for your use.  I assume no responsibility for any ***
// *** undesired events resulting from the use of this code or the		 ***
// *** information that has been provided with it .						 ***
//

using System;
using System.Collections;

namespace WCell.Util.DynamicProperties
{
	/// <summary>
	/// PropertyAccessorTestObject.
	/// </summary>
	public class PropertyAccessorTestObject
	{
		public PropertyAccessorTestObject()
		{
		}

		public int Int
		{
			get
			{
				return this.mInt;
			}
			set
			{
				this.mInt = value;
			}
		}

		public string String
		{
			get
			{
				return this.mString;
			}
			set
			{	
				this.mString = value;
			}
		}

		public sbyte Sbyte
		{
			get
			{
				return this.mSbyte;
			}
			set
			{
				this.mSbyte = value;
			}
		}

		public byte Byte
		{
			get
			{
				return this.mByte;
			}
			set
			{
				this.mByte = value;
			}
		}

		public char Char
		{
			get
			{
				return this.mChar;
			}
			set
			{
				this.mChar = value;
			}
		}

		public short Short
		{
			get
			{
				return this.mShort;
			}
			set
			{
				this.mShort = value;
			}
		}

		public ushort UShort
		{
			get
			{
				return this.mUshort;
			}
			set
			{
				this.mUshort = value;
			}
		}

		public long Long
		{
			get
			{
				return this.mLong;
			}
			set
			{
				this.mLong = value;
			}
		}

		public ulong ULong
		{
			get
			{
				return this.mUlong;
			}
			set
			{
				this.mUlong = value;
			}
		}

		public bool Bool
		{
			get
			{
				return this.mBool;
			}
			set
			{
				this.mBool = value;
			}
		}

		public double Double
		{
			get
			{
				return this.mDouble;
			}
			set
			{
				this.mDouble = value;
			}
		}

		public float Float
		{
			get
			{
				return this.mFloat;
			}
			set
			{
				this.mFloat = value;
			}
		}

		public DateTime DateTime
		{
			get
			{
				return this.mDateTime;
			}
			set
			{
				this.mDateTime = value;
			}
		}

		public decimal Decimal
		{
			get
			{
				return this.mDecimal;
			}
			set
			{
				this.mDecimal = value;
			}
		}

		public IList List
		{
			get
			{
				return this.mList;
			}
			set
			{
				this.mList = value;
			}
		}

		public int ReadOnlyInt
		{
			get
			{
				return this.mReadOnlyInt;
			}
		}

		public int WriteOnlyInt
		{
			set
			{
				this.mWriteOnlyInt = value;	
			}
		}

		private int mInt;
		private string mString;
		private sbyte mSbyte;
		private byte mByte;
		private char mChar;
		private short mShort;
		private ushort mUshort;
		private long mLong;
		private ulong mUlong;
		private bool mBool;
		private double mDouble;
		private float mFloat;
		private DateTime mDateTime;
		private decimal mDecimal;
		private IList mList;
		private int mReadOnlyInt = 0;
		private int mWriteOnlyInt;
	}
}
