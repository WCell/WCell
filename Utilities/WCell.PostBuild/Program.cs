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
		public const string DocsDir = "Docs/";

		static void Main(string[] args)
		{
			Console.WriteLine("Performing Post-Build steps (Docs in: {0}) ...", new DirectoryInfo(DocsDir).FullName);

			CommandDocs.CreateCommandDocs(DocsDir);

			Console.WriteLine("Done. - Please press ANY key to continue...");
			Console.ReadKey();
		}
	}
}
