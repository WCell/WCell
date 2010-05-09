using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Updates;
using System.IO;
using WCell.Tools.Code;
using WCell.Constants;
using WCell.Tools.Ralek;
using WCell.Tools.Ralek.UpdateFields;
using WCell.Util;
using WCell.Util.Toolshed;

namespace WCell.Tools.FileCreators
{
	/// <summary>
	/// Auto-creates the VisibilityManager that decides Visibility of values
	/// </summary>
	public class UpdateFieldWriter
	{
		public static string OutputFileLocation = ToolConfig.WCellConstantsRoot + "/Updates/UpdateFields.cs";

		private readonly UpdateField[][] m_fields;
		private readonly string UpdateFieldTypeName = typeof(UpdateField).Name;

		public UpdateFieldWriter(UpdateField[][] fields)
		{
			m_fields = fields;
		}

		public void Write()
		{
			Write(OutputFileLocation);
		}

		public void Write(string fileName)
		{
			using (var writer = new CodeFileWriter(fileName,
												   "WCell.Constants.Updates", "UpdateFields",
												   CodeFileWriter.StaticTag + " " + CodeFileWriter.Class,
												   "",
												   "System"))
			{
				writer.ExecuteSafely(() =>
				{
					writer.WriteIndent("public static readonly ");
					writer.Array(UpdateFieldTypeName, "AllFields", 2, ";", () =>
					{
						for (var i = 0; i < m_fields.Length; i++)
						{
							var fieldArr = m_fields[i];
							writer.WriteRegion(((ObjectTypeId)i).ToString());
							writer.NewArray("UpdateField", ",", () =>
							{
								foreach (var field in fieldArr)
								{
									if (field != null)
									{
										writer.WriteCommentLine(field.FullName);
										var flags = Utility.GetSetIndicesEnum(field.Flags);

										var args = new[]
														{
															new KeyValuePair<string, object> ("Flags",flags.TransformList((flag) =>
				                     	                                                       		                			           				 "UpdateFieldFlags." +
				                     	                                                       		                			           				 flag)
				                     	                                                       		                			           				.
				                     	                                                       		                			           				ToString
				                     	                                                       		                			           				(" | "))
				                     	                                                       		                			           		,
				                     	                                                       		                			           		new KeyValuePair
				                     	                                                       		                			           			<
				                     	                                                       		                			           			string
				                     	                                                       		                			           			,
				                     	                                                       		                			           			object
				                     	                                                       		                			           			>(
				                     	                                                       		                			           			"Group",
				                     	                                                       		                			           			"ObjectTypeId." +
				                     	                                                       		                			           			field
				                     	                                                       		                			           				.
				                     	                                                       		                			           				Group)
				                     	                                                       		                			           		,
				                     	                                                       		                			           		new KeyValuePair
				                     	                                                       		                			           			<
				                     	                                                       		                			           			string
				                     	                                                       		                			           			,
				                     	                                                       		                			           			object
				                     	                                                       		                			           			>(
				                     	                                                       		                			           			"Name",
				                     	                                                       		                			           			"\"" +
				                     	                                                       		                			           			field
				                     	                                                       		                			           				.
				                     	                                                       		                			           				Name +
				                     	                                                       		                			           			"\"")
				                     	                                                       		                			           		,
				                     	                                                       		                			           		new KeyValuePair
				                     	                                                       		                			           			<
				                     	                                                       		                			           			string
				                     	                                                       		                			           			,
				                     	                                                       		                			           			object
				                     	                                                       		                			           			>(
				                     	                                                       		                			           			"Offset",
				                     	                                                       		                			           			field
				                     	                                                       		                			           				.
				                     	                                                       		                			           				Offset)
				                     	                                                       		                			           		,
				                     	                                                       		                			           		new KeyValuePair
				                     	                                                       		                			           			<
				                     	                                                       		                			           			string
				                     	                                                       		                			           			,
				                     	                                                       		                			           			object
				                     	                                                       		                			           			>(
				                     	                                                       		                			           			"Size",
				                     	                                                       		                			           			field
				                     	                                                       		                			           				.
				                     	                                                       		                			           				Size)
				                     	                                                       		                			           		,
				                     	                                                       		                			           		new KeyValuePair
				                     	                                                       		                			           			<
				                     	                                                       		                			           			string
				                     	                                                       		                			           			,
				                     	                                                       		                			           			object
				                     	                                                       		                			           			>(
				                     	                                                       		                			           			"Type",
				                     	                                                       		                			           			"UpdateFieldType." +
				                     	                                                       		                			           			field
				                     	                                                       		                			           				.
				                     	                                                       		                			           				Type)
				                     	                                                       		                			           	};
										writer.NewInit(
											UpdateFieldTypeName,
											args, ",");
									}
									else
									{
										writer.WriteLine(
											"null,");
									}
								}
							});
							writer.WriteEndRegion();
							writer.WriteLine();
						}
					});
					writer.WriteLine();

					//writer.WriteStaticMethod("Init", () => {

					//});

					//writer.WriteStaticCTor(() => {
					//    writer.Call("Init");
					//});
				});
			}
		}

		public static void WriteFromWowFile(string wowFileLocation)
		{
			using (var file = new WoWFile(wowFileLocation))
			{
				Write(file);
			}
		}

		[NoTool]
		public static void Write(WoWFile file)
		{
			var fields = UpdateFieldExtractor.Extract(file);
			var mgr = new UpdateFieldWriter(fields);
			mgr.Write();
		}
	}
}