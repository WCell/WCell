/*************************************************************************
 *
 *   file		: Owner.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 20:38:52 +0100 (to, 14 jan 2010) $

 *   revision		: $Rev: 1195 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Cell.Core;
using WCell.Constants;
using WCell.Core.Paths;
using WCell.Core.Terrain.Paths;
using WCell.RealmServer.Items;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer
{
    /// <summary>
    /// Class for all type extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Adds all elements from the collection to the hash set.
        /// </summary>
        /// <typeparam name="T">the type of the elements</typeparam>
        /// <param name="set">the hash set to add to\being extended</param>
        /// <param name="elements">the elements to add</param>
        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> elements)
        {
            foreach (var element in elements)
            {
                set.Add(element);
            }
        }

        public static bool ImplementsType(this Type targetType, Type baseType)
        {
            return baseType.IsAssignableFrom(targetType);
        }



		#region ItemDamage
		public static float TotalMin(this DamageInfo[] damages)
		{
		    return damages.Sum(dmg => dmg.Minimum);
		}

		public static float TotalMax(this DamageInfo[] damages)
		{
		    return damages.Sum(dmg => dmg.Maximum);
		}

		public static DamageSchoolMask AllSchools(this DamageInfo[] damages)
		{
		    return damages.Aggregate(DamageSchoolMask.None, (current, dmg) => current | dmg.School);
		}
		#endregion

		public static bool IsValid(this IWorldLocation location)
		{
			return location.Position.X != 0 && location.Map != null && location.Phase != 0;
		}

		public static void AddChecked(this XElement element, string name, int value)
        {
            if (value != 0)
            {
                element.Add(new XElement(name, value));
            }
        }

        public static void AddChecked(this XElement element, string name, float value)
        {
            if (value != 0f)
            {
                element.Add(new XElement(name, value));
            }
		}

		public static int ReadInt32(this XElement element, string name)
		{
			XElement ele = element.Element(name);
			if (ele != null)
			{
				return int.Parse(ele.Value);
			}
			return 0;
		}

		public static uint ReadUInt32(this XElement element, string name)
		{
			XElement ele = element.Element(name);
			if (ele != null)
			{
				var strVal = ele.Value;
				uint result;
				if (!uint.TryParse(strVal, out result))
				{
					result = (uint)int.Parse(ele.Value);
				}
				return result;
			}
			return 0;
		}

        public static uint ReadUInt32(this XElement element)
        {
            uint ret;
            uint.TryParse(element.Value, out ret);
            return ret;
        }

        public static T ReadEnum<T>(this XElement element, string name)
        {
            string str = ReadString(element, name);
            return (T)Enum.Parse(typeof(T), str);
        }

		public static bool ReadBoolean(this XElement element, string name)
		{
			XElement ele = element.Element(name);
			if (ele != null)
			{
				var strVal = ele.Value;
                return strVal == "1" || strVal.Equals("true", StringComparison.InvariantCultureIgnoreCase);
			}
			return false;
		}

        public static string ReadString(this XElement element, string name)
        {
            XElement nameElement = element.Element(name);
            if (nameElement != null)
            {
                return nameElement.Value;
            }
            return string.Empty;
        }

        public static float ReadFloat(this XElement element, string name)
        {
            XElement ele = element.Element(name);
            if (ele != null)
            {
                return float.Parse(ele.Value);
            }
            return 0f;
		}

        public static float ReadFloat(this XElement element, string name, float defaultValue)
        {
            XElement ele = element.Element(name);
            if (ele != null)
            {
                return float.Parse(ele.Value);
            }
            return defaultValue;
        }

        public static Vector3 ReadLocation(this XElement node, string xyzPrefix, bool upperCase)
        {
            var x = ReadFloat(node, xyzPrefix + (upperCase ? "X" : "x"));
            var y = ReadFloat(node, xyzPrefix + (upperCase ? "Y" : "y"));
			var z = ReadFloat(node, xyzPrefix + (upperCase ? "Z" : "z"));
			//return new Vector3(x, z, y);
			return new Vector3(x, y, z);
        }
    



		public static XmlNode Add(this XmlNode el, string name, Vector3 pos)
		{
			var str = pos.X + "," + pos.Y + "," + pos.Z;
			return Add(el, name, str);
		}

		public static XmlNode Add(this XmlNode el, string name, Vector4 pos)
		{
			var str = pos.X + "," + pos.Y + "," + pos.Z + "," + pos.W;
			return Add(el, name, str);
		}

		public static XmlNode Add(this XmlNode el, string name, object value, params object[] args)
		{
			var doc = el.OwnerDocument ?? el as XmlDocument;

		    var node = el.AppendChild(doc.CreateElement(name));
			node.AppendChild(doc.CreateTextNode(string.Format(value.ToString(), args)));
			return node;
		}

		public static XmlNode AddAttr(this XmlNode el, string name, object value, params object[] args)
		{
			var doc = el.OwnerDocument ?? el as XmlDocument;

		    var attr = el.Attributes.Append(doc.CreateAttribute(name));
			attr.AppendChild(doc.CreateTextNode(string.Format(value.ToString(), args)));
			return attr;
		}

		public static XmlElement Add(this XmlNode el, string name)
		{
			var doc = el.OwnerDocument ?? el as XmlDocument;

		    return el.AppendChild(doc.CreateElement(name)) as XmlElement;
		}

		public static int GetAttrInt32(this XElement element, string name)
		{
			return int.Parse(element.Attribute(name).Value);
		}

		public static uint GetAttrUInt32(this XElement element, string name)
		{
			uint result;
			if (!uint.TryParse(element.Attribute(name).Value, out result)) {
				result = (uint)int.Parse(element.Attribute(name).Value);
			}
			return result;
		}

		public static Vector3 GetLocation(this byte[] bytes, uint index)
		{
			return new Vector3(bytes.GetFloat(index), bytes.GetFloat(index + 1), bytes.GetFloat(index + 2));
		}

		public static float GetDist(this IHasPosition pos, IHasPosition pos2)
		{
			return pos.Position.GetDistance(pos2.Position);
		}

		public static float GetDistSq(this IHasPosition pos, IHasPosition pos2)
		{
			return pos.Position.DistanceSquared(pos2.Position);
		}

		public static float GetDistSq(this IHasPosition pos, Vector3 pos2)
		{
			return pos.Position.DistanceSquared(pos2);
		}
    }
}