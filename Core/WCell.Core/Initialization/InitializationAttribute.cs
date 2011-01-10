/*************************************************************************
 *
 *   file		: InitializationAttribute.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-04-03 04:09:16 +0200 (fr, 03 apr 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 855 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace WCell.Core.Initialization
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class InitializationAttribute : Attribute, IInitializationInfo
	{
		public InitializationAttribute()
		{
			IsRequired = true;
			Name = "";
			Pass = InitializationPass.Any;
		}

		public InitializationAttribute(string name)
		{
			IsRequired = true;
			Name = name;
			Pass = InitializationPass.Any;
		}

		public InitializationAttribute(InitializationPass pass)
		{
			IsRequired = true;
			Name = "";
			Pass = pass;
		}

		public InitializationAttribute(InitializationPass pass, string name)
		{
			IsRequired = true;
			Pass = pass;
			Name = name;
		}

		public InitializationPass Pass { get; private set; }

		public string Name { get; set; }

		public bool IsRequired { get; set; }
	}
}