/*************************************************************************
 *
 *   file		: RoleGroupConfig.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-05-14 13:26:25 +0800 (Wed, 14 May 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 351 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using WCell.Intercommunication.DataTypes;
using WCell.Util;

namespace WCell.AuthServer.Privileges
{
	/// <summary>
	/// Provides storage/loading of role groups from an XML configuration file.
	/// </summary>
	[Serializable]
	public class RoleGroupConfig : XmlFile<RoleGroupConfig>
	{
		public static readonly string[] EmptyNameArr = new string[0];
		static List<string> EmptyNameList = EmptyNameArr.ToList();

		#region Properties

		/// <summary>
		/// A list of all role groups.
		/// </summary>
		[XmlArray("Privileges")]
		[XmlArrayItem("Privilege")]
		public RoleGroupInfo[] RoleGroups
		{
			get;
			set;
		}

		#endregion

		#region Methods
		/// <summary>
		/// Tries to load the specified configuration file, creating a default
		/// configuration file if the specified one does not exist.
		/// </summary>
		/// <param name="fname">the name of the configuration file to load</param>
		/// <returns>a <see cref="RoleGroupConfig" /> object representing the loaded file</returns>
		public static RoleGroupConfig LoadConfigOrDefault(string fname)
		{
			var confFolder = AuthServerConfiguration.GetFullPath(AuthServerConfiguration.ConfigDir);
			var privFile = Path.Combine(confFolder, fname);

		    var cfg = File.Exists(privFile) ? Load(privFile) : null;

			if (RoleGroupInfo.HighestRole == null ||
				RoleGroupInfo.HighestRole.IsStaff)
			{
				cfg = new RoleGroupConfig
				          {
				              RoleGroups = RoleGroupInfo.CreateDefaultGroups().ToArray()
				          };
			    cfg.SaveAs(fname, confFolder);
			}

			return cfg;
		}

		#endregion
	}
}