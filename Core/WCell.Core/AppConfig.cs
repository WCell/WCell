/*************************************************************************
 *
 *   file		: Configuration.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-08-20 14:41:59 +0800 (Wed, 20 Aug 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 607 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using NLog;
using WCell.Core.Localization;
using System.IO;

namespace WCell.Core
{
	/// <summary>
	/// Defines a configuration made up of key/value values
	/// </summary>
	public class AppConfig : IEnumerable<string>
	{
		protected static Logger s_log = LogManager.GetCurrentClassLogger();
		public const string NullString = "None";

		private readonly Configuration m_cfg;
		/// <summary>
		/// Whether to save after adding/changing values
		/// </summary>
		public bool SaveOnChange;

		public readonly FileInfo ExecutableFile;

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="executablePath">The path of the executable whose App-config to load</param>
		public AppConfig(string executablePath)
		{
			try
			{
				m_cfg = ConfigurationManager.OpenExeConfiguration((ExecutableFile = new FileInfo(executablePath)).FullName);
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Cannot load AppConfig for {0}", executablePath), e);
			}

			LoadConfigDefaults();
		}

		/// <summary>
		/// Loads default values in the configuration if they don't already exist
		/// </summary>
		protected virtual void LoadConfigDefaults()
		{
		}

		private int GetNoneNesting(string val)
		{
			int num1 = 0;
			int num2 = val.Length;
			if (num2 > 1)
			{
				while ((val[num1] == '(') && (val[(num2 - num1) - 1] == ')'))
				{
					num1++;
				}
				if ((num1 > 0) &&
					(string.Compare(NullString, 0, val, num1, num2 - (2 * num1), StringComparison.Ordinal) != 0))
				{
					num1 = 0;
				}
			}
			return num1;
		}

		public bool AddValue(string key, string value)
		{
			m_cfg.AppSettings.Settings.Add(key, value);

			return true;
		}

		public string GetValue(string key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			string cfgVal = m_cfg.AppSettings.Settings[key].Value;

			if (cfgVal == null)
			{
				s_log.Error(string.Format(Resources.KeyNotFound, key));

				return "";
			}

			switch (GetNoneNesting(cfgVal))
			{
				case 0:
					return cfgVal;

				case 1:
					return null;
			}

			return cfgVal.Substring(1, cfgVal.Length - 2);
		}

		public bool GetBool(string key)
		{
			bool b;

			if (!Boolean.TryParse(GetValue(key), out b))
			{
				// Log an error and return false;

				return false;
			}

			return b;
		}

		public bool SetValue(string key, object value)
		{
		    if (m_cfg.AppSettings.Settings[key] != null)
			{
				m_cfg.AppSettings.Settings.Remove(key);
				m_cfg.AppSettings.Settings.Add(key, value.ToString());

				return true;
			}
		    return false;
		}

	    /// <summary>
		/// Creates a config entry with the supplied value if one doesn't already exist
		/// </summary>
		/// <param name="key">the key</param>
		/// <param name="value">the value</param>
		public void CreateValue(string key, object value)
		{
			KeyValueConfigurationElement cfgVal = m_cfg.AppSettings.Settings[key];

			if (cfgVal == null)
			{
				m_cfg.AppSettings.Settings.Add(key, value.ToString());
				if (SaveOnChange)
				{
					m_cfg.Save(ConfigurationSaveMode.Full);
				}
			}
		}

		public void OverrideValue(string key, string value)
		{
			KeyValueConfigurationElement cfgVal = m_cfg.AppSettings.Settings[key];

			if (cfgVal == null)
			{
				m_cfg.AppSettings.Settings.Add(key, value);
			}
			else
			{
				cfgVal.Value = value;
			}

			if (SaveOnChange)
			{
				m_cfg.Save(ConfigurationSaveMode.Full);
			}
		}

		public void Save()
		{
			m_cfg.Save(ConfigurationSaveMode.Full);
		}

		public string GetFullPath(string file)
		{
			if (!Path.IsPathRooted(file))
			{
				return Path.Combine(ExecutableFile.Directory.FullName, file);
			}
			return file;
		}

		#region IEnumerable members

		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			string configLine = "";

			foreach (string cfgEle in m_cfg.AppSettings.Settings.AllKeys)
			{
				configLine = cfgEle + ": " + m_cfg.AppSettings.Settings[cfgEle].Value;

				yield return configLine;
			}
		}

		/// <summary>
		/// Get an enumerator that represents the key/value pairs of this configuration
		/// </summary>
		/// <returns>an IEnumerator object to enumerate through this configuration</returns>
		public IEnumerator GetEnumerator()
		{
			StringCollection strCol = new StringCollection();
			string configLine = "";

			foreach (string cfgEle in m_cfg.AppSettings.Settings.AllKeys)
			{
				configLine = cfgEle + ": " + m_cfg.AppSettings.Settings[cfgEle].Value;

				strCol.Add(configLine);
			}

			return (IEnumerator)strCol.GetEnumerator();
		}

		#endregion
	}
}