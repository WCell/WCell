/*************************************************************************
 *
 *   file		: XmlConfig.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-29 16:09:43 +0200 (sø, 29 jun 2008) $
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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Cell.Core.Localization;

namespace Cell.Core
{
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
    [Serializable]
    public class XmlConfig
    {
        /// <summary>
        /// The file name of the configuration file.
        /// </summary>
        private string _filename = "config.xml";

        /// <summary>
        /// Gets/Sets the name of the configuration file.
        /// </summary>
		[XmlIgnore]
        public virtual string FileName
        {
            get { return _filename; }
            set { _filename = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileName">The name of the configuration file.</param>
        public XmlConfig(string fileName)
        {
            _filename = fileName;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public XmlConfig()
        {
        }

        /// <summary>
        /// Returns whether or not the file exists
        /// </summary>
        public virtual bool FileExists(string path)
        {
            //return File.Exists((path != "" ? path + "\\" : "") + m_fname);
            return File.Exists((String.IsNullOrEmpty(path) ? "" : path + "\\") + _filename);
        }

        /// <summary>
        /// Writes the configuration file to disk.
        /// </summary>
        public virtual void Save()
        {
            XmlSerializer ser = new XmlSerializer(GetType());

            string path = Path.GetDirectoryName(_filename);

            if (!String.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //have to use TextWriter so the resulting XML file's format isn't all jacked up
            //I dno wtf XmlWriter's problem is but it sux when it comes to proper formatting of whitespace
            //and indentation
            using (TextWriter writer = new StreamWriter(_filename, false, Encoding.UTF8))
            {
                ser.Serialize(writer, this);
                writer.Close();
            }
        }

        /// <summary>
        /// Writes the configuration file to disk with the specified name.
        /// </summary>
        /// <param name="fileName">The name of the file on disk to write to.</param>
        public virtual void SaveAs(string fileName)
        {
            _filename = fileName;
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

            _filename = fileName;

            XmlSerializer ser = new XmlSerializer(GetType());

            if (!Directory.Exists(location))
            {
                Directory.CreateDirectory(location);
            }
            if (location[location.Length - 1] != Path.DirectorySeparatorChar)
            {
                location += Path.DirectorySeparatorChar;
            }

            location += _filename;

            using (TextWriter writer = new StreamWriter(location, false, Encoding.UTF8))
            {
                ser.Serialize(writer, this);
                writer.Close();
            }
        }

        /// <summary>
        /// Copies one <see cref="XmlConfig"/> derived class into another.
        /// </summary>
        /// <param name="cfg">The <see cref="XmlConfig"/> derived class to copy from.</param>
        /// <remarks>You MUST override this function in derived classes!</remarks>
        public virtual void Assign(XmlConfig config)
        {
            _filename = config._filename;
        }

		public virtual void OnLoad()
		{
		}

		public static T Load<T>(string filename) where T : XmlConfig
		{
			T cfg;
			XmlSerializer ser = new XmlSerializer(typeof(T));
			using (XmlReader rdr = XmlReader.Create(filename))
			{
				cfg = ser.Deserialize(rdr) as T;
			}
			cfg.FileName = filename;
			cfg.OnLoad();
			return cfg;
		}

        /// <summary>
        /// Loads a configuration file from disk.
        /// </summary>
        /// <param name="type">The type to be saved.</param>
        /// <remarks>This is most useful for subclasses saving data using a base classes schema.</remarks>
        public virtual void Load(Type type)
        {
            XmlConfig cfg;
            XmlSerializer ser = new XmlSerializer(type);
            using (XmlReader rdr = XmlReader.Create(_filename))
            {
                cfg = ser.Deserialize(rdr) as XmlConfig;
                rdr.Close();
            }

            if (cfg != null)
            {
                Assign(cfg);
            }
        }

        /// <summary>
        /// Loads a configuration file from disk.
        /// </summary>
        public virtual void Load()
        {
            Load(GetType());
        }

        /// <summary>
        /// Loads a configuration file from disk from the specified directory.
        /// </summary>
        public virtual void LoadFrom(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentException(Resources.PathCannotBeNull, "path");
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (path[path.Length - 1] != Path.DirectorySeparatorChar)
            {
                path += Path.DirectorySeparatorChar;
            }

            path += _filename;

            XmlConfig cfg;
            XmlSerializer ser = new XmlSerializer(GetType());
            using (XmlReader rdr = XmlReader.Create(path))
            {
                cfg = ser.Deserialize(rdr) as XmlConfig;
                rdr.Close();
            }

            if (cfg != null)
            {
                Assign(cfg);
            }
        }

        /// <summary>
        /// Returns the serialized XML of this XmlConfig for further processing, etc.
        /// </summary>
        /// <returns>The XML representation of this <see cref="XmlConfig"/> instance</returns>
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
    }
}