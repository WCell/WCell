/*************************************************************************
 *
 *   file		: StringUtils.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-02-16 12:30:51 +0800 (Mon, 16 Feb 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 757 $
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

namespace WCell.Util
{
    ///<summary>
    ///</summary>
    public static class StringUtils
    {
        /// <summary>
        /// Combines the strings of a string array into one delimited string
        /// </summary>
        /// <param name="inputArray">The string array to combine</param>
        /// <param name="delimiter">The delimited</param>
        /// <returns>A string of the delimited strings</returns>
        public static string ToDelimitedString(this string[] inputArray, string delimiter)
        {
            string returnSz;

            if (inputArray.Length > 1)
            {
                returnSz = String.Join(delimiter, inputArray);
            }
            else
            {
                return inputArray[0];
            }

            return returnSz;
        }

        /// <summary>
        /// Combines the strings of an List&lt;string&gt; into one delimited string
        /// </summary>
        /// <param name="inputArray">The List&lt;string&gt; to combine</param>
        /// <param name="delimiter">The delimited</param>
        /// <returns>A string of the delimited strings</returns>
        public static string ToDelimitedString(this List<string> inputArray, string delimiter)
        {
            string returnSz;

            if (inputArray.Count > 1)
            {
                returnSz = String.Join(delimiter, inputArray.ToArray());
            }
            else
            {
                return inputArray[0];
            }

            return returnSz;
        }

        /// <summary>
        /// Combines the strings of a string array into a string, which resembles a list
        /// </summary>
        /// <param name="szArray">The string array to combine</param>
        /// <returns>A string which resembles a list, using commas, and follows the rules of English grammar</returns>
        public static string GetReadableList(this string[] szArray)
        {
            if (szArray.Length == 0) return "none";
            if (szArray.Length == 1) return szArray[0];

            string list = String.Join(";", szArray);

            list = list.Replace(";", ", ");

            int lastSplitPos = list.LastIndexOf(", ");

            list = list.Insert(lastSplitPos + 2, "and ");

            return list;
        }

        /// <summary>
        /// Safely splits a string without erroring if the delimiter is not present
        /// </summary>
        /// <param name="inputSz">The string to split</param>
        /// <param name="delimiter">The character to split on</param>
        /// <returns>A string array of the split string</returns>
        public static string[] Split(this string inputSz, char delimiter)
        {
            if (String.IsNullOrEmpty(inputSz)) return new string[] {};

            if (inputSz.IndexOf(delimiter) == -1) return new[] {inputSz};

            return inputSz.Split(delimiter);
        }

        /// <summary>
        /// Converts a byte array to a period-delimited string
        /// </summary>
        /// <param name="inputArray">the byte array to convert</param>
        /// <returns>a period-delimited string of the converted byte array</returns>
        public static string ToReadableIPAddress(this byte[] inputArray)
        {
            if (inputArray.Length != 4)
            {
                return "not an IP address";
            }

            var retString = new StringBuilder();

            retString.Append(inputArray[0].ToString()).Append('.');
            retString.Append(inputArray[1].ToString()).Append('.');
            retString.Append(inputArray[2].ToString()).Append('.');
            retString.Append(inputArray[3].ToString());

            return retString.ToString();
        }

        /// <summary>
        /// Converts a random string (aBcDeFG) to a capitalized string (Abcdefg)
        /// </summary>
        public static string ToCapitalizedString(this string input)
        {
            if (input.Length == 0)
                return input;

            var newName = new StringBuilder();

            newName.Append(char.ToUpper(input[0]));
            for (int i = 1; i < input.Length; i++)
            {
                newName.Append(char.ToLower(input[i]));
            }

            return newName.ToString();
        }

        /// <summary>
        /// Capitalizes the string and also considers (and removes) special characters, such as "_"
        /// </summary>
        public static string ToFriendlyName(this string input)
        {
            var newName = new StringBuilder();

            bool upper = true;
            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == '_')
                {
                    c = ' ';
                }

                c = upper ? char.ToUpper(c) : char.ToLower(c);
                upper = c == ' ';
				
                newName.Append(c);
            }

            return newName.ToString();
        }

        public static string ToCamelCase(this string input)
        {
            var newName = new StringBuilder();

            bool upper = true;
            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == '_')
                {
                    c = ' ';
                }

                c = upper ? char.ToUpper(c) : char.ToLower(c);

                if (c == ' ')
                {
                    upper = true;
                    continue;
                }

                upper = false;

                newName.Append(c);
            }

            return newName.ToString();
        }

        public static string Format(this string input, params object[] args)
        {
            return string.Format(input, args);
        }
    }
}