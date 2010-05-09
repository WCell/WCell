using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Misc;
using WCell.Tools.Code;
using System.IO;

namespace WCell.Tools.FileCreators
{
	public class ExperienceClassCreator : CodeFileWriter
	{
		public static readonly string File = ToolConfig.WCellConstantsRoot + "Xp/Experience.cs";

		public static readonly int MaxLevel = 200; //255;

		public ExperienceClassCreator()
			: base(File, "WCell.Constants.Xp", "Experience", "static class")
		{
		}

		public void Write()
		{
			throw new Exception("Currently not implemented");
			//WriteCommentLine("public static uint[,] XpTable = new uint[0,0];");

			//WriteIndent("public static readonly uint[,] XpTable = new uint[,]");

			//var perLine = 15;
			//OpenBracket();
			//for (int t = 0; t <= MaxLevel; t++)
			//{
			//    WriteCommentLine("Target Level " + t);
			//    OpenBracket();

			//    for (int r = 0; r <= MaxLevel; r++)
			//    {
			//        if (r % perLine == 0)
			//        {
			//            WriteCommentLine("Receiver Level " + r);
			//            WriteIndent("");
			//        }
			//        Write(" " + XpMgr.DefaultCalculator(t, r));
			//        if (r < MaxLevel)
			//        {
			//            Write(",");
			//            if (r % perLine == perLine - 1)
			//            {
			//                WriteLine();
			//            }
			//        }
			//    }

			//    WriteLine();
			//    CloseBracket(",");
			//}
			//CloseBracket(";");
		}

		public static void Create()
		{
			using (var creator = new ExperienceClassCreator())
			{
				creator.Write();
			}
		}
	}
}