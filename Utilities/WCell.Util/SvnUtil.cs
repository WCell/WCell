using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WCell.Util
{
	/// <summary>
	/// Utility class for interacting with SVN repositories on disk.
	/// </summary>
	public static class SvnUtil
	{
		/// <summary>
		/// Attempts to get the latest revision number of a checked out SVN repository on disk.
		/// </summary>
		/// <param name="svnRoot">the root of the repository</param>
		/// <returns>the number of the repository revision, or 0 if a problem occured</returns>
		public static int GetVersionNumber(string svnRoot)
		{
			int repoRevision = 0;

			var entriesFile = Path.Combine(svnRoot, ".svn/entries");
			var lines = Utility.ReadLines(entriesFile, 3, true);
			var versionStr = lines[2];
			int.TryParse(versionStr, out repoRevision);

			return repoRevision;





		}
	}
}
