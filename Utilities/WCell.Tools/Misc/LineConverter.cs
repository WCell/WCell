using System.IO;
using System.Linq;
using WCell.Util.Toolshed;

namespace WCell.Tools.Misc
{
    public static class LineConverter
    {
        public static string[] StandardCodingTextFiles = new[] { ".sln", "proj", ".txt", ".cs", ".xml" };
        public static string LineEnding = "\r\n";

        /// <summary>
        /// Converts all line endings of all text files in wcell from crlf to lf
        /// </summary>
        [Tool]
        public static void ConvertLines()
        {
            ConvertLines(ToolConfig.WCellRoot, StandardCodingTextFiles);
        }

        public static void ConvertLines(string directory, params string[] suffixes)
        {
            foreach (var file in Directory.GetFileSystemEntries(directory))
            {
                if (Directory.Exists(file))
                {
                    ConvertLines(file, suffixes);
                }
                else
                {
                    // ReSharper disable AccessToModifiedClosure
                    if (suffixes.Any(suffix => suffix.Length > 0 && file.EndsWith(suffix)))
                    // ReSharper restore AccessToModifiedClosure
                    {
                        // found the right kind of file
                        var lines = File.ReadLines(file);
                        var text = string.Join(LineEnding, lines);
                        File.WriteAllText(file, text);
                    }
                }
            }
        }
    }
}