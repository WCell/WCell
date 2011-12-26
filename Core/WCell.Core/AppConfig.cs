/*************************************************************************
 *
 *   file		: Configuration.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-08-20 14:41:59 +0800 (Wed, 20 Aug 2008) $
 
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using NLog;
using WCell.Core.Localization;

namespace WCell.Core
{
	/// <summary>
	/// Defines a configuration made up of key/value values
	/// </summary>
	public sealed class AppConfig : IEnumerable<string>
	{
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
		public const string NullString = "None";

		private readonly Configuration _cfg;

	    private readonly FileInfo _executableFile;

        public FileInfo ExecutableFile
	    {
	        get { return _executableFile; }
	    }

	    /// <summary>
	    /// Whether to save after adding/changing values
	    /// </summary>
	    public bool SaveOnChange { get; set; }

	    /// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="executablePath">The path of the executable whose App-config to load</param>
		public AppConfig(string executablePath)
		{
			try
			{
				_cfg = ConfigurationManager.OpenExeConfiguration((_executableFile = new FileInfo(executablePath)).FullName);
			}
			catch (Exception e)
			{
				throw new Exception(string.Format(CultureInfo.CurrentCulture, "Cannot load AppConfig for {0}", executablePath), e);
			}

			LoadConfigDefaults();
		}

	    

	    /// <summary>
		/// Loads default values in the configuration if they don't already exist
		/// </summary>
		private static void LoadConfigDefaults()
		{
		}

		private static int GetNoneNesting(string val)
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
			_cfg.AppSettings.Settings.Add(key, value);

			return true;
		}

		public string GetValue(string key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			string cfgVal = _cfg.AppSettings.Settings[key].Value;

			if (cfgVal == null)
			{
				Log.Error(string.Format(CultureInfo.CurrentCulture, WCell_Core.KeyNotFound, key));

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
		    if (_cfg.AppSettings.Settings[key] != null)
			{
				_cfg.AppSettings.Settings.Remove(key);
				_cfg.AppSettings.Settings.Add(key, value.ToString());

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
			KeyValueConfigurationElement cfgVal = _cfg.AppSettings.Settings[key];

			if (cfgVal == null)
			{
				_cfg.AppSettings.Settings.Add(key, value.ToString());
				if (SaveOnChange)
				{
					_cfg.Save(ConfigurationSaveMode.Full);
				}
			}
		}

		public void OverrideValue(string key, string value)
		{
			KeyValueConfigurationElement cfgVal = _cfg.AppSettings.Settings[key];

			if (cfgVal == null)
			{
				_cfg.AppSettings.Settings.Add(key, value);
			}
			else
			{
				cfgVal.Value = value;
			}

			if (SaveOnChange)
			{
				_cfg.Save(ConfigurationSaveMode.Full);
			}
		}

		public void Save()
		{
			_cfg.Save(ConfigurationSaveMode.Full);
		}

		public string GetFullPath(string file)
		{
			if (!Path.IsPathRooted(file))
			{
			    Debug.Assert(_executableFile.Directory != null, "ExecutableFile.Directory != null");
			    return Path.Combine(_executableFile.Directory.FullName, file);
			}
		    return file;
		}

		#region IEnumerable members

		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
		    foreach (string cfgElement in _cfg.AppSettings.Settings.AllKeys)
			{
			    string configLine = cfgElement + ": " + _cfg.AppSettings.Settings[cfgElement].Value;

			    yield return configLine;
			}
		}

		/// <summary>
		/// Get an enumerator that represents the key/value pairs of this configuration
		/// </summary>
		/// <returns>an IEnumerator object to enumerate through this configuration</returns>
		public IEnumerator GetEnumerator()
		{
			var strCol = new StringCollection();

            foreach (string cfgElement in _cfg.AppSettings.Settings.AllKeys)
			{
			    string configLine = cfgElement + ": " + _cfg.AppSettings.Settings[cfgElement].Value;

			    strCol.Add(configLine);
			}

		    return (IEnumerator)strCol.GetEnumerator();
		}

		#endregion
	}
}