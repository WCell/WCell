using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WCell.PostBuild.Docs;
using WCell.RealmServer;
using WCell.RealmServer.Commands;

using RealmServ = WCell.RealmServer.RealmServer;

namespace WCell.PostBuild
{
	/// <summary>
	/// Stuff to be executed after WCell has been built: Auto-create documentation etc.
	/// </summary>
	public class Program
	{
		//private const string CWD = "../../Utilities/WCell.PostBuild";
		private const string CWD = ".";
		const string DocsDir = "../Docs/";

		static int Main(string[] args)
		{
			Directory.SetCurrentDirectory(CWD);

			RealmServ.EntryLocation = "WCell.RealmServerConsole.exe";
			var realm = RealmServ.Instance;		// do this to enforce creation of a RealmServer instance which again loads the Config
			RealmServerConfiguration.Initialize();

			CommandDocs.CreateCommandDocs(DocsDir);
			return 0;
		}
	}
}
