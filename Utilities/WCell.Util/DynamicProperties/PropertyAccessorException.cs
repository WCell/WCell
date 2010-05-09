//
// Author: James Nies
// Date: 3/22/2005
// Description: Exception that can be thrown from the PropertyAccessor
//		class.
//
// *** This code was written by James Nies and has been provided to you, ***
// *** free of charge, for your use.  I assume no responsibility for any ***
// *** undesired events resulting from the use of this code or the		 ***
// *** information that has been provided with it .						 ***
//

using System;

namespace WCell.Util.DynamicProperties
{
	/// <summary>
	/// PropertyAccessorException class.
	/// </summary>
	public class PropertyAccessorException : Exception
	{
		public PropertyAccessorException(string message)
			: base(message)
		{
		}

		public PropertyAccessorException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
