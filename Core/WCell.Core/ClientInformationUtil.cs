/*************************************************************************
 *
 *   file		    : ClientInformationUtil.cs
 *   copyright      : (C) The WCell Team
 *   email		    : info@wcell.org
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using WCell.Constants;

namespace WCell.Core
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class ClientTypeUtility
    {
        private static readonly Dictionary<string, ClientType> TypeMap =
            new Dictionary<string, ClientType>(StringComparer.OrdinalIgnoreCase)
                {
                    {"WoWT", ClientType.Test},
                    {"WoWB", ClientType.Beta},
                    {"WoW\0", ClientType.Normal},
                    {"WoWI", ClientType.Installing}
                };

        /// <summary>
        /// Looks up an enumeration for the given string.
        /// </summary>
        /// <param name="locale">string representation to lookup</param>
        /// <returns>Returns the matching enum member or the <see cref="ClientType.Invalid"/></returns>
        public static ClientType Lookup(string clientInstallationType)
        {
            if (string.IsNullOrWhiteSpace(clientInstallationType))
            {
                return ClientType.Invalid;
            }

            try
            {
                clientInstallationType = clientInstallationType.Substring(0, 4);
            }
            catch (ArgumentOutOfRangeException)
            {
                return ClientType.Invalid;
            }

            ClientType clientType;
            if (!TypeMap.TryGetValue(clientInstallationType, out clientType))
            {
                return ClientType.Invalid;
            }
            return clientType;
        }

    }

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class ClientLocaleUtility
    {
        private static readonly Dictionary<string, ClientLocale> LocaleMap =
            new Dictionary<string, ClientLocale>(StringComparer.OrdinalIgnoreCase)
                {
                    {"enUS", ClientLocale.English},
                    {"enGB", ClientLocale.English},
                    {"koKR", ClientLocale.Korean},
                    {"frFR", ClientLocale.French},
                    {"deDE", ClientLocale.German},
                    {"zhCN", ClientLocale.ChineseSimplified},
                    {"zhTW", ClientLocale.ChineseTraditional},
                    {"esES", ClientLocale.Spanish},
                    {"esMX", ClientLocale.Spanish},
                    {"ruRU", ClientLocale.Russian},
                };

        /// <summary>
        /// Looks up an enumeration for the given string.
        /// </summary>
        /// <param name="locale">string representation to lookup</param>
        /// <returns>Returns the matching enum member or the <see cref="WCellConstants.DefaultLocale"/></returns>
        public static ClientLocale Lookup(string locale)
        {
            if (string.IsNullOrWhiteSpace(locale))
            {
                return WCellConstants.DefaultLocale;
            }

            try
            {
                locale = locale.Substring(0, 4);
            }
            catch (ArgumentOutOfRangeException)
            {
                return WCellConstants.DefaultLocale;
            }

            ClientLocale clientLocale;
            if (!LocaleMap.TryGetValue(locale, out clientLocale))
            {
                return WCellConstants.DefaultLocale;
            }
            return clientLocale;
        }
    }
}
