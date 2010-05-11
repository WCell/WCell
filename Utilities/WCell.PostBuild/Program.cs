using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WCell.PostBuild.Docs;
using WCell.RealmServer.Commands;

namespace WCell.PostBuild
{
	/// <summary>
	/// Stuff to be executed after WCell has been built: Auto-create documentation etc.
	/// </summary>
	public class Program
	{
		public const string DocsDir = "../Docs/";

		static void Main(string[] args)
		{
			CommandDocs.CreateCommandDocs(DocsDir);
		}
	}
}
