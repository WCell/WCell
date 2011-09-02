using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using WCell.PacketAnalysis.Updates;
using WCell.Util;
using WCell.PacketAnalysis;
using WCell.PacketAnalysis.Logs;
using WCell.Core.Network;

namespace WCell.Tools.PATools
{
	public class PATool : XmlFile<PATool>
	{
		private DirectoryInfo m_selectedDir;
		private IStreamTarget m_output;
		private OpCodeValidator m_opcodeValidator;

		private LogParserType m_LogParserType;
		private LogParser m_LogParseHandler;
		private GenericLogParser m_parser;

		[XmlIgnore] public List<FileInfo> SelectedFiles = new List<FileInfo>();

		public PATool() : this("")
		{
		}

		public PATool(string filename) : base(filename)
		{
			m_opcodeValidator = DefaultOpCodeValidator;
		}

		public PATool(XmlFileBase parentCfg) : base(parentCfg)
		{
			m_opcodeValidator = DefaultOpCodeValidator;
		}

		public PATool(string filename, DirectoryInfo selectedDir, IStreamTarget outputTarget, LogParser defaultParser)
			: this(filename)
		{
			Init(null, filename, selectedDir, outputTarget, defaultParser);
		}

		public PATool(XmlFileBase parentCfg, DirectoryInfo selectedDir, IStreamTarget outputTarget, LogParser defaultParser)
			: this(parentCfg)
		{
			Init(parentCfg, null, selectedDir, outputTarget, defaultParser);
		}

		internal void Init(XmlFileBase parentCfg, string filename, DirectoryInfo selectedDir, IStreamTarget outputTarget,
		                   LogParser defaultParser)
		{
			m_parentConfig = parentCfg;
			m_filename = filename;
			m_selectedDir = selectedDir;
			m_LogParseHandler = defaultParser;
			m_output = outputTarget;
			OnLoad();
		}

		private bool DefaultOpCodeValidator(PacketId packetId)
		{
			var str = packetId.ToString();

			bool allow;

			if (OpCodeExcAndFilters.Count > 0)
			{
				// The id must not contain ALL of the ExcludeAnd-filters
				allow = false;
				foreach (var filter in OpCodeExcAndFilters)
				{
					if (str.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) < 0)
					{
						allow = true;
						break;
					}
				}
			}
			else
			{
				allow = true;
			}

			if (allow)
			{
				if (OpCodeExcOrFilters.Count > 0)
				{
					// The id must not contain ANY of the ExcludeOr-filters
					foreach (var filter in OpCodeExcOrFilters)
					{
						if (str.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0)
						{
							allow = false;
							break;
						}
					}
				}

				if (allow)
				{
					if (OpCodeIncAndFilters.Count > 0)
					{
						// The id must contain ALL of the IncludeAnd-filters
						foreach (var filter in OpCodeIncAndFilters)
						{
							if (str.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) < 0)
							{
								allow = false;
								break;
							}
						}
					}

					if (allow)
					{
						if (OpCodeIncOrFilters.Count > 0)
						{
							// The id must contain ANY of the IncludeOr-filters
							allow = false;
							foreach (var filter in OpCodeIncOrFilters)
							{
								if (str.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0)
								{
									allow = true;
									break;
								}
							}
						}
					}
				}
			}
			return allow;
		}

		[XmlAttribute("SelectedDir")]
		public string SelectedDirPath
		{
			get { return m_selectedDir.FullName; }
			set { SetSelectedDirPath(value); }
		}

		public bool SetSelectedDirPath(string value)
		{
			if (m_selectedDir != null)
			{
				if (!Path.IsPathRooted(value))
				{
					value = Path.Combine(m_selectedDir.FullName, value);
				}
			}

			if (Directory.Exists(value))
			{
				var isNew = m_selectedDir == null;
				m_selectedDir = new DirectoryInfo(value);

				if (!isNew)
				{
					// auto clear and add all files from that directory, if this is not the first time its set (initialization)
					SelectedFiles.Clear();
					SelectedFiles.AddRange(m_selectedDir.GetFiles());
				}
				return true;
			}
			return false;
		}

		[XmlAttribute]
		public string OutputFilePath
		{
			get { return new FileInfo(m_output.Name).FullName; }
			set
			{
				if (!Path.IsPathRooted(value))
				{
					value = Path.Combine(m_selectedDir.FullName, value);
				}

				if (m_output == null)
				{
					m_output = new FileStreamTarget(value);
				}
				else
				{
					m_output.Name = value;
				}
			}
		}

		[XmlAttribute("Parser")]
		public LogParserType LogParserType
		{
			get { return m_LogParserType; }
			set
			{
				m_LogParserType = value;
				if ((LogParseHandler = LogConverter.GetParser(value)) == null)
				{
					throw new Exception("Invalid LogParser selected: " + m_LogParserType);
				}
			}
		}

		[XmlElement("OpCodeIncAndFilter")]
		public List<string> OpCodeIncAndFilters { get; set; }

		[XmlElement("OpCodeIncOrFilter")]
		public List<string> OpCodeIncOrFilters { get; set; }

		[XmlElement("OpCodeExcAndFilter")]
		public List<string> OpCodeExcAndFilters { get; set; }

		[XmlElement("OpCodeExcOrFilter")]
		public List<string> OpCodeExcOrFilters { get; set; }

		[XmlElement("SelectedFile")]
		public string[] SelectedFilePaths
		{
			get { return SelectedFiles.TransformArray(file => file.FullName); }
			set
			{
				if (value != null)
				{
					foreach (var path in value)
					{
						SelectedFiles.Add(new FileInfo(path));
					}
				}
			}
		}

		[XmlIgnore]
		public LogParser LogParseHandler
		{
			get
			{
				if (m_parser == null)
				{
					return m_LogParseHandler;
				}
				return m_parser.Parser;
			}
			set
			{
				if (m_parser == null)
				{
					m_LogParseHandler = value;
				}
				else
				{
					m_parser.Parser = value;
				}
			}
		}

		/// <summary>
		/// The target that the output should be written to.
		/// </summary>
		[XmlIgnore]
		public IStreamTarget Output
		{
			get { return m_output; }
			set { m_output = value; }
		}

		/// <summary>
		/// The selected Directory.
		/// Selecting a new directory will clear all selected files and all files from the new directory instead.
		/// </summary>
		[XmlIgnore]
		public DirectoryInfo SelectedDir
		{
			get { return m_selectedDir; }
			set
			{
				if (m_selectedDir != null && !m_selectedDir.Equals(value))
				{
					m_selectedDir = value;
					SelectedFiles.Clear();
					SelectedFiles.AddRange(m_selectedDir.GetFiles());
				}
			}
		}

		public void Parse()
		{
			Save();
			Output.Open();
			try
			{
				m_parser.Parse(SelectedFiles);
			}
			finally
			{
				Output.Close();
			}
		}

		private void HandlePackets(PacketParser parser)
		{
			parser.Dump(m_output.Writer);
			m_output.Writer.WriteLine();
			m_output.Writer.Flush();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="packet"></param>
		private void HandleUpdatePackets(ParsedUpdatePacket packet)
		{
			packet.Dump(m_output.Writer);
			m_output.Writer.WriteLine();
			m_output.Writer.Flush();
		}

		public bool AddSelectedFiles(int[] indices)
		{
			Array.Sort(indices);
			var files = m_selectedDir.GetFiles();

			foreach (var index in indices)
			{
				if (index >= files.Count())
				{
					return false;
				}
				var file = files[index];

				if (file.Exists && !SelectedFiles.Contains(file))
				{
					SelectedFiles.Add(file);
				}
			}
			return true;
		}

		public bool AddSelectedFile(string path)
		{
			if (!Path.IsPathRooted(path))
			{
				path = Path.Combine(m_selectedDir.FullName, path);
			}
			var file = new FileInfo(path);

			if (file.Exists && !SelectedFiles.Contains(file) && !path.Contains(OutputFilePath))
			{
				SelectedFiles.Add(file);
				return true;
			}
			return false;
		}

		public bool RemoveSelectedFiles(int[] indices)
		{
			Array.Sort(indices, (i1, i2) => i2 - i1);

			foreach (var index in indices)
			{
				if (index >= SelectedFiles.Count())
				{
					return false;
				}
				var file = SelectedFiles[index];

				SelectedFiles.Remove(file);
			}
			return true;
		}

		public bool RemoveSelectedFile(string path)
		{
			if (!Path.IsPathRooted(path))
			{
				path = Path.Combine(m_selectedDir.FullName, path);
			}

			var file = new FileInfo(path);
			return SelectedFiles.Remove(file);
		}

		public void ClearSelectedFiles()
		{
			SelectedFiles.Clear();
		}

		internal void _OnLoad(XmlFileBase parentCfg)
		{
			m_parentConfig = parentCfg;
			OnLoad();
		}

		protected override void OnLoad()
		{
			if (m_selectedDir == null)
			{
				m_selectedDir = new DirectoryInfo(ToolConfig.PAToolDefaultDir);
			}
			else
			{
				m_selectedDir = new DirectoryInfo(SelectedDirPath);
			}
			m_selectedDir.MKDirs();

			if (OutputFilePath == null)
			{
				OutputFilePath = ToolConfig.PAToolDefaultOutputFile;
			}

			if (OpCodeExcAndFilters == null)
			{
				OpCodeExcAndFilters = new List<string>();
			}
			if (OpCodeExcOrFilters == null)
			{
				OpCodeExcOrFilters = new List<string>();
			}
			if (OpCodeIncAndFilters == null)
			{
				OpCodeIncAndFilters = new List<string>();
			}
			if (OpCodeIncOrFilters == null)
			{
				OpCodeIncOrFilters = new List<string>();
			}
			if (SelectedFiles == null)
			{
				SelectedFiles = new List<FileInfo>();
			}

			foreach (var file in SelectedFiles.ToArray().Where(file => !file.Exists))
			{
				SelectedFiles.Remove(file);
			}
			
			m_parser = new GenericLogParser(m_LogParseHandler, new LogHandler(DefaultOpCodeValidator, HandlePackets, HandleUpdatePackets));

			//if (needsSave)
			//{
			//    Save();
			//}
		}
	}
}