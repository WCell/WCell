using System.Text.RegularExpressions;
using WCell.Core;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Chat
{
    /// <summary>
    /// Utility class for creating and filtering chat messages.
    /// </summary>
    public static class ChatUtility
    {
        public static readonly Regex ControlCodeRegex = new Regex(@"\|(cff[0-9a-fA-F]{6})|(\|[rh])", RegexOptions.Compiled);

        //  |cffffff00|Hquest:10704:70|h[How to Break Into the Arcatraz]|h|r
        //  |cffa335ee|Hitem:28506:2564:2729:2945:0:0:0:1659302763|h[Gloves of Dexterous Manipulation]|h|r
        //  |cff71d5ff|Hspell:32549|h[Leatherworking]|h|r
        public static readonly Regex AllowedControlRegex =
            new Regex(@"\|cff[0-9a-fA-F]{6}\|H(item|quest|spell|achievement|trade)(\:\d+)+\|h\[[\w\d]([^\|\t\r\n\0\]]*)\]\|h\|r",
                RegexOptions.Compiled);

        /// <summary>
        /// Strips all color controls from a string.
        /// </summary>
        /// <param name="msg">the message to strip colors from</param>
        /// <returns>a string with any color controls removed from it</returns>
        public static string Strip(string msg)
        {
            return ControlCodeRegex.Replace(msg, "");
        }

        /// <summary>
        /// Colorizes the given message with the given color.
        /// </summary>
        /// <param name="msg">the message to colorize</param>
        /// <param name="color">the color value</param>
        /// <returns>a colorized string</returns>
        public static string Colorize(string msg, Color color)
        {
            return Colorize(msg, color, true);
        }

        /// <summary>
        /// Colorizes the given message with the given color.
        /// </summary>
        /// <param name="msg">the message to colorize</param>
        /// <param name="color">the color value</param>
        /// <param name="enclose">whether to only have this string in the given color or to let the color code open for following text</param>
        /// <returns>a colorized string</returns>
        public static string Colorize(string msg, Color color, bool enclose)
        {
            return "|cff" + color.GetHexCode() + msg + (enclose ? "|r" : "");
        }

        /// <summary>
        /// Colorizes the given message with the given color.
        /// </summary>
        /// <param name="msg">the message to colorize</param>
        /// <param name="colorRgb">the color value</param>
        /// <returns>a colorized string</returns>
        public static string Colorize(string msg, string colorRgb)
        {
            return Colorize(msg, colorRgb, true);
        }

        /// <summary>
        /// Colorizes the given message with the given color.
        /// </summary>
        /// <param name="msg">the message to colorize</param>
        /// <param name="colorRgb">the color value</param>
        /// <param name="enclose">whether to only have this string in the given color or to let the color code open for following text</param>
        /// <returns>a colorized string</returns>
        public static string Colorize(string msg, string colorRgb, bool enclose)
        {
            return "|cff" + colorRgb + msg + (enclose ? "|r" : "");
        }

        /// <summary>
        /// Colorizes the given message with the given color.
        /// </summary>
        /// <param name="msg">the message to colorize</param>
        /// <param name="red">the red color value</param>
        /// <param name="green">the green color value</param>
        /// <param name="blue">the blue color value</param>
        /// <returns>a colorized string</returns>
        public static string Colorize(string msg, string red, string green, string blue)
        {
            return Colorize(msg, red, green, blue, true);
        }

        /// <summary>
        /// Colorizes the given message with the given color.
        /// </summary>
        /// <param name="msg">the message to colorize</param>
        /// <param name="red">the red color value</param>
        /// <param name="green">the green color value</param>
        /// <param name="blue">the blue color value</param>
        /// <param name="enclose">whether to only have this string in the given color or to let the color code open for following text</param>
        /// <returns>a colorized string</returns>
        public static string Colorize(string msg, string red, string green, string blue, bool enclose)
        {
            return "|cff" + red + green + blue + msg + (enclose ? "|r" : "");
        }

        /// <summary>
        /// Filters a string, removing any illegal control characters or character sequences.
        /// </summary>
        public static void Purify(ref string msg)
        {
            var escapeCount = 0;

            for (int i = 0; i < msg.Length; i++)
            {
                var c = msg[i];

                // space (chr code 32) is the start of normal characters in the ASCII table
                // 31 and lower are control character
                if (c < ' ')
                {
                    // don't bother with this kind of cheating
                    msg = "";
                    break;
                }
                else if (c != '|')
                {
                    if (escapeCount % 2 != 0)
                    {
                        // uneven amount of escape characters
                        // msg = msg.Insert(i, "|");

                        // don't bother with this kind of cheating
                        msg = "";
                        break;
                    }
                    escapeCount = 0;
                }
                else
                {
                    if (escapeCount % 2 == 0)
                    {
                        var found = false;
                        var match = AllowedControlRegex.Match(msg, i);
                        while (match.Success)
                        {
                            found = true;
                            i += match.Length;
                            match = match.NextMatch();
                        }

                        if (!found)
                        {
                            escapeCount++;
                        }
                        else
                        {
                            escapeCount = 0;
                        }
                    }
                    else
                    {
                        escapeCount++;
                    }
                }
            }
        }
    }
}