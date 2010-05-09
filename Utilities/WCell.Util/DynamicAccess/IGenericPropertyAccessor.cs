//
// Author: James Nies
// Contributor: Tobias Hertkorn
// Date: 5/31/2005
// Description: The GenericPropertyAccessor class uses this interface 
//		for creating a type at runtime for accessing an individual
//		property on a target object.
//
// *** This code was written by James Nies and has been provided to you, ***
// *** free of charge, for your use.  I assume no responsibility for any ***
// *** undesired events resulting from the use of this code or the		 ***
// *** information that has been provided with it .						 ***
//

using System;

namespace WCell.Util.DynamicAccess
{
	/// <summary>
	/// The IPropertyAccessor interface defines a property
	/// accessor.
	/// </summary>
	public interface IGenericPropertyAccessor<T, V>
	{
		/// <summary>
		/// Gets the value stored in the property for 
		/// the specified target.
		/// </summary>
		/// <param name="target">Object to retrieve
		/// the property from.</param>
		/// <returns>Property value.</returns>
		V Get(T target);

		/// <summary>
		/// Sets the value for the property of
		/// the specified target.
		/// </summary>
		/// <param name="target">Object to set the
		/// property on.</param>
		/// <param name="value">Property value.</param>
		void Set(T target, V value);
	}
}