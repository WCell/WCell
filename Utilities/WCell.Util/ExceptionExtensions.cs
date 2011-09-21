using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WCell.Util
{
	public static class ExceptionExtensions
	{
		public static List<string> GetAllMessages(this Exception ex)
		{
			var msgs = new List<string>();
			do
			{
				if (!(ex is TargetInvocationException))
				{
					msgs.Add(ex.Message);
					if ((ex is ReflectionTypeLoadException))
					{
						msgs.Add("###########################################");
						msgs.Add("LoaderExceptions:");
						foreach (var lex in ((ReflectionTypeLoadException)ex).LoaderExceptions)
						{
							msgs.Add(lex.GetType().FullName + ":");
							msgs.AddRange(lex.GetAllMessages());
							if (lex is FileNotFoundException)
							{
								var asmName = ((FileNotFoundException)lex).FileName;
								var asms = Utility.GetMatchingAssemblies(asmName);
								if (asms.Count() > 0)
								{
									msgs.Add("Found matching Assembly: " + asms.ToString("; ") +
										" - Make sure to compile against the correct version.");
								}
								else
								{
									msgs.Add("Did not find any matching Assembly - Make sure to load the required Assemblies before loading this one.");
								}
							}
							msgs.Add("");
						}
						msgs.Add("#############################################");
					}
				}
				ex = ex.InnerException;
			} while (ex != null);
			return msgs;
		}
	}
}
