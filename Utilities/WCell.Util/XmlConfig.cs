/*************************************************************************
 *
 *   file		: XmlConfig.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-29 22:09:43 +0800 (Sun, 29 Jun 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 540 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace WCell.Util
{
	[Serializable]
	/// <summary>
	/// This class represents an XML configuration file that is serializable.
	/// </summary>
	/// <example>
	/// The following is an example of how to derive a class from <see cref="XmlConfig"/>.
	/// <code lang="C#">
	/// 
	/// //SerializableAttribute is inherited
	/// public class MyConfig : XmlConfig
	/// {
	///		int m_someInt = 23432;
	///		string m_someStr = "XmlConfig r0x my s0x!";
	///	
	///		public int SomeInt
	///		{
	///			get
	///			{
	///				return m_someInt;
	///			}
	///			set
	///			{
	///				m_someInt = value;
	///			}
	///		}
	/// 
	///		public string SomeStr
	///		{
	///			get
	///			{
	///				return m_someStr;
	///			}
	///			set
	///			{
	///				m_someStr = value;
	///			}
	///		}
	/// 
	///		public MyConfig()
	///		{
	///		}
	/// 
	///		public override void Assign(XmlConfig cfg)
	///		{
	///			base.Assign(cfg);
	///			
	///			MyConfig mcfg;
	///			if((mcfg = cfg as MyConfig) != null)
	///			{
	///				m_someInt = mcfg.m_someInt;
	///				m_someStr = mcfg.m_someStr;
	///			}
	///		}
	///	
	///		[STAThread]
	///		static void Main()
	///		{
	///			MyConfig cfg = new MyConfig();
	///			cfg.Save(); //default's to config.xml
	/// 
	///			MyConfig cfg2 = new MyConfig();
	///			cfg2.Load(); //default's to config.xml
	/// 
	///			MyConfig cfg3 = XmlConfig.Load("config.xml", typeof(MyConfig));
	///		}
	/// }
	/// </code>
	/// The resulting configuration file:
	/// <code lang="XML">
	/// 
	/// &lt;?xml version="1.0" encoding="utf-8"?&gt;
	/// &lt;MyConfig xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"&gt;
	///		&lt;FileName&gt;config.xml&lt;/FileName&gt;
	///		&lt;SomeInt&gt;23432&lt;/SomeInt&gt;
	///		&lt;SomeStr&gt;XmlConfig r0x my s0x!&lt;/SomeStr&gt;
	/// &lt;/MyConfig&gt;
	/// </code>
	/// </example>
	public class XmlConfig<T> : XmlConfigBase
		where T : XmlConfigBase
	{
		protected XmlConfig()
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fileName">The name of the configuration file.</param>
		public XmlConfig(string fileName)
		{
			m_filename = fileName;
		}

		public XmlConfig(XmlConfigBase parentConfig)
		{
			m_parentConfig = parentConfig;
		}

		/// <summary>
		/// Returns whether or not the file exists
		/// </summary>
		public virtual bool FileExists(string path)
		{
			//return File.Exists((path != "" ? path + "\\" : "") + m_fname);
			return File.Exists((String.IsNullOrEmpty(path) ? "" : path + "\\") + m_filename);
		}

		/// <summary>
		/// Writes the configuration file to disk.
		/// </summary>
		public override void Save()
		{
			if (m_parentConfig != null)
			{
				m_parentConfig.Save();
			}
			else
			{
				var ser = new XmlSerializer(GetType());
				var path = Path.GetDirectoryName(m_filename);

				if (path.Length > 0 && !Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				//have to use TextWriter so the resulting XML file's format isn't all jacked up
				//I dno wtf XmlWriter's problem is but it sux when it comes to proper formatting of whitespace
				//and indentation
				using (TextWriter writer = new StreamWriter(m_filename, false, Encoding.UTF8))
				{
					ser.Serialize(writer, this);
				}
			}
		}

		/// <summary>
		/// Writes the configuration file to disk with the specified name.
		/// </summary>
		/// <param name="fileName">The name of the file on disk to write to.</param>
		public override void SaveAs(string fileName)
		{
			m_filename = fileName;
			Save();
		}

		/// <summary>
		/// Writes the configuration file to disk with the specified name.
		/// </summary>
		/// <param name="fileName">The name of the file on disk to write to.</param>
		/// <param name="location">The directory to write the file to.</param>
		public virtual void SaveAs(string fileName, string location)
		{
			if (String.IsNullOrEmpty(location))
			{
				throw new ArgumentException("Location cannot be be null or empty!", "location");
			}

			m_filename = fileName;

			XmlSerializer ser = new XmlSerializer(GetType());

			if (!Directory.Exists(location))
			{
				Directory.CreateDirectory(location);
			}
			if (location[location.Length - 1] != Path.DirectorySeparatorChar)
			{
				location += Path.DirectorySeparatorChar;
			}

			location += m_filename;

			using (TextWriter writer = new StreamWriter(location, false, Encoding.UTF8))
			{
				ser.Serialize(writer, this);
				writer.Close();
			}
		}

		protected override void OnLoad()
		{
		}

		/// <summary>
		/// Returns the serialized XML of this XmlConfig for further processing, etc.
		/// </summary>
		public override string ToString()
		{
			//XmlSerializer ser = new XmlSerializer(GetType());
			//string output;

			//using (MemoryStream stream = new MemoryStream())
			//{
			//    ser.Serialize(stream, this);
			//    output = Encoding.Unicode.GetString(stream.ToArray());
			//}

			//return output;
			return FileName;
		}

		public static T Load(string filename)
		{
			T cfg;
			var ser = new XmlSerializer(typeof(T));
			using (var rdr = XmlReader.Create(filename))
			{
				cfg = (T)ser.Deserialize(rdr);
			}
			cfg.FileName = filename;
			(((XmlConfig<T>)(XmlConfigBase)cfg)).OnLoad();
			return cfg;
		}

		public static ICollection<T> LoadAll(string dir)
		{
			var list = new List<T>();
			LoadAll(dir, list);
			return list;
		}

		public static ICollection<T> LoadAll(DirectoryInfo dir)
		{
			var list = new List<T>();
			LoadAll(dir, list);
			return list;
		}

		public static void LoadAll(string dir, ICollection<T> cfgs)
		{
			LoadAll(new DirectoryInfo(dir), cfgs);
		}

		public static void LoadAll(DirectoryInfo dir, ICollection<T> cfgs)
		{
			foreach (var file in dir.GetFileSystemInfos())
			{
				if (file is DirectoryInfo)
				{
					LoadAll((DirectoryInfo)file, cfgs);
				}
				else
				{
					if (file.Extension.EndsWith("xml", StringComparison.InvariantCultureIgnoreCase))
					{
						try
						{
							var def = Load(file.FullName);
							cfgs.Add(def);
						}
						catch (Exception e)
						{
							e = new Exception("Error when loading XML-file: " + file, e);
#if DEBUG
							Debugger.Break();
#endif
							throw e;
						}
					}
				}
			}
		}
	}

	public abstract class XmlConfigBase
	{
		/// <summary>
		/// The file name of the configuration file.
		/// </summary>
		protected string m_filename;
		protected XmlConfigBase m_parentConfig;

		[XmlIgnore]
		public string FileName
		{
			get
			{
				return m_filename;
			}
			set
			{
				m_filename = value;
			}
		}

		[XmlIgnore]
		public string ActualFile
		{
			get
			{
				if (m_parentConfig != null)
				{
					return m_parentConfig.FileName;
				}
				return m_filename;
			}
		}

		public abstract void Save();

		public abstract void SaveAs(string filename);

		protected abstract void OnLoad();
	}
}