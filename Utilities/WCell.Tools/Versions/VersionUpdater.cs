using System.IO;
using WCell.MPQTool;
using WCell.Tools.Code;
using WCell.Tools.FileCreators;
using WCell.Tools.Ralek;
using WCell.Tools.Ralek.UpdateFields;

namespace WCell.Tools.Versions
{
	/// <summary>
	/// Generic utility class to easily make WCell work with the latest Client version
	/// </summary>
	public static class VersionUpdater
	{
		private static WoWFile _wowFile;
		public static UpdateFieldExtractor _extractor;

		public static WoWFile WoWFile
		{
			get
			{
				if (_wowFile == null)
				{
					_wowFile = new WoWFile(ToolConfig.WoWFileLocation);
					_extractor = new UpdateFieldExtractor(_wowFile);
				}
				return _wowFile;
			}
		}

		public static UpdateFieldExtractor Extractor
		{
			get { return _extractor; }
		}

		public static void SetWowDir(string dir)
		{
			_wowFile = new WoWFile(dir);
			_extractor = new UpdateFieldExtractor(_wowFile);
		}

		public static void DumpDBCs()
		{
			DBCTool.Dump(Path.GetDirectoryName(_wowFile.FileName), true, false);
		}

		/// <summary>
		/// WARNING: This re-generates code-files to comply with the current client-version
		/// </summary>
		public static void DoUpdate(bool dumpDBCs)
		{
			if (dumpDBCs)
			{
				DumpDBCs();
			}
			DoUpdate();
		}

		/// <summary>
		/// WARNING: This re-generates code-files to comply with the current client-version
		/// </summary>
		public static void DoUpdate()
		{
			ExtractUpdateFields();
			ExtractSpellFailures();

			WCellEnumWriter.WriteAllEnums();
			WriteWCellInfo();
		}

		public static void ExtractUpdateFields()
		{
			var updateFieldFile = Path.Combine(ToolConfig.WCellConstantsUpdates, "UpdateFieldEnums.cs");
			Extractor.DumpEnums(updateFieldFile);

			var mgr = new UpdateFieldWriter(_extractor.Extract());
			mgr.Write();
		}

		public static void ExtractSpellFailures()
		{
			var spellFailedReasonFile = Path.Combine(ToolConfig.WCellConstantsRoot, "Spells/SpellFailedReason.cs");
			SpellFailureExtractor.Extract(WoWFile, spellFailedReasonFile);
		}

		public static void WriteWCellInfo()
		{
			using (var writer = new CodeFileWriter(Path.Combine(ToolConfig.WCellConstantsRoot, "WCellInfo.cs"),
			                                       "WCell.Constants", "WCellInfo", "static class"))
			{
				writer.WriteSummary(@"The official codename of the current WCell build");
				writer.WriteLine("public const string Codename = \"Amethyst\";");
				writer.WriteLine();
				writer.WriteSummary(@"The color of the current WCell codename");
				writer.WriteLine(@"public const ChatColor CodenameColor = ChatColor.Purple;");
				writer.WriteLine();
				writer.WriteSummary("The version of the WoW Client that is currently supported.");
				writer.WriteLine("public static readonly ClientVersion RequiredVersion = new ClientVersion({0}, {1}, {2}, {3});",
				                 WoWFile.Version.Major, WoWFile.Version.Minor, WoWFile.Version.Revision, WoWFile.Version.Build);
				writer.WriteLine();
			}
		}
	}
}