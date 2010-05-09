using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using WCell.Util.Variables;

namespace WCell.Core.Addons
{
	/// <summary>
	/// Simple plugin that cannot be unloaded (the script component will make up for that)
	/// </summary>
	public interface IWCellAddon
	{
		/// <summary>
		/// The culture-invariant name of this Addon
		/// </summary>
		string Name { get; }

		/// <summary>
		/// A shorthand name of the Addon that does not contain any spaces.
		/// Used as unique ID for this Addon.
		/// </summary>
		string ShortName { get; }

		/// <summary>
		/// The name of the Author
		/// </summary>
		string Author { get; }

		/// <summary>
		/// Website (where this Addon can be found)
		/// </summary>
		string Website { get; }

		/// <summary>
		/// The Configuration to be used. May be null if this Addon is not configurable.
		/// </summary>
		IConfiguration Config { get; }

		/// <summary>
		/// The localized name, in the given culture
		/// </summary>
		string GetLocalizedName(CultureInfo culture);

		/// <summary>
		/// Tears down the Addon, releases all resources and disconnects from the core.
		/// </summary>
		void TearDown();
	}
}
