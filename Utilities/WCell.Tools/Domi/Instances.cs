using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using WCell.RealmServer;
using WCell.RealmServer.Content;
using WCell.RealmServer.Database;
using WCell.Tools.Code;
using WCell.RealmServer.Global;
using WCell.RealmServer.Instances;
using WCell.Constants;
using WCell.Util.Toolshed;
using System.IO;

namespace WCell.Tools.Domi
{
	public static class Instances
	{
		[Tool]
		public static void WriteInstanceStubs()
		{
			var dir = ToolConfig.DefaultAddonSourceDir + "Instances/";

			RealmDBMgr.Initialize();
			ContentMgr.Initialize();
			World.InitializeWorld();
			InstanceMgr.Initialize();

			foreach (var instance in InstanceMgr.InstanceInfos)
			{
				var className = instance.Id.ToString();
				var baseClass = instance.Type == MapType.Raid ? typeof(RaidInstance).Name : typeof(DungeonInstance).Name;
				var file = dir + className + ".cs";
				if (!File.Exists(file))
				{
					using (var writer = new CodeFileWriter(file, "WCell.Addons.Default.Instances",
					                                       className,
					                                       "class",
					                                       ": " + baseClass,
					                                       "WCell.RealmServer.Instances"))
					{

					}
				}
			}
		}

        [Tool]
        public static void MergeInstancesXML()
        {
            var path = RealmServerConfiguration.GetContentPath("Instances.xml");
            //If the file isnt there we dont need to waste our time
            if (!File.Exists(path)) return;

            var newPath = RealmServerConfiguration.GetContentPath("Instances.new.xml");

            //Copy the instances.xml into Instances.new.xml
            File.Copy(path, newPath, true);

            //If the backup was successful remove the original file
            if (File.Exists(newPath))
            {
                File.Delete(path);
            }

            var oldPath = RealmServerConfiguration.GetContentPath("Instances.old.xml");

            //Make sure we have two vaild files
            if (File.Exists(newPath) && File.Exists(oldPath))
            {
                var oldConfigReader = new XmlTextReader(oldPath);
                var newConfigReader = new XmlTextReader(newPath);

                //Create a data set from the old instances xml file
                var oldDataSet = new DataSet();
                oldDataSet.ReadXml(oldConfigReader);

                //And another data set from the new instances xml file
                var newDataSet = new DataSet();
                newDataSet.ReadXml(newConfigReader);

                //Heres the magic! Merge the two datasets
                oldDataSet.Merge(newDataSet, true);

                //var configFile = ds.GetXml();

                //Save out changes, were done merging
                var xmlWriter = new XmlTextWriter(path, Encoding.UTF8);

                //prevent data from being appended
                xmlWriter.BaseStream.SetLength(0);
                //pretty print
                xmlWriter.Formatting = Formatting.Indented;
                oldDataSet.WriteXml(xmlWriter);
                xmlWriter.Close();
#if DEBUG
                var intPath =  RealmServerConfiguration.GetContentPath("Instances.merged.xml");
                //Save out changes, were done merging
                var xmlIntWriter = new XmlTextWriter(intPath, Encoding.UTF8);

                //prevent data from being appended
                xmlIntWriter.BaseStream.SetLength(0);
                //pretty print
                xmlIntWriter.Formatting = Formatting.Indented;
                oldDataSet.WriteXml(xmlIntWriter);
                xmlIntWriter.Close();
#endif

                Console.WriteLine("Completed merging Instance.xml documents!");

                //Prune old nodes that no longer exist
                var oldXmlDoc = new XmlDocument();
                oldXmlDoc.Load(path);

                //Get the root node of the old instances file
                var node = oldXmlDoc.DocumentElement;

                if (node == null)
                {
                    Console.WriteLine("Could not find xml root element of {0}", oldPath);
                    return;
                }

                var nodesToRemove = new List<XmlNode>();

                //Iterate over all of the nodes in the document
                foreach (XmlNode node1 in node.ChildNodes)
                {
                    //Used to test if the instance name has a match in the instances enum
                    //and the old instances document
                    var ok = false;
                    foreach (XmlNode node2 in node1.ChildNodes)
                    {
                        //We only need the instance names
                        if (node2.Name != "Name") continue;

                        //Lower case for case insensitive comparison
                        var nodeInstanceName = node2.InnerText.ToLower();

                        //Iterate over all of the instances in the new version
                        foreach (var instance in InstanceMgr.InstanceInfos)
                        {
                            //Lower case for case insensitive comparison
                            var enumInstanceName = instance.Id.ToString().ToLower();

                            //if we find a match mark it as ok so it isnt added
                            //to the list for removal
                            if (nodeInstanceName == enumInstanceName)
                                ok = true;
                        }

                        if (ok == false)
                        {
                            Console.WriteLine("Removing node {0} which no longer exists", node2.InnerText);
                            //Queue the parent node and its children for removal
                            //To get rid of <Setting> <Name> </Name> <Type> </Type> </Setting>
                            nodesToRemove.Add(node1);
                        }
                        else
                            ok = false; //Reset the bool to false ready for the next iteration
                    }
                }

                Console.WriteLine("Removing {0} nodes and children", nodesToRemove.Count);
                //Iterate over our list of nodes to kiss goodbye!
                foreach (var xmlNode in nodesToRemove)
                {
                    //Remove all children
                    xmlNode.RemoveAll();
                    //Sayonara node!
                    node.RemoveChild(xmlNode);
                }

                nodesToRemove.Clear();

                var namesNodes = new List<XmlNode>();
                //Iterate over all of the nodes in the document
                foreach (XmlNode node1 in node.ChildNodes)
                {

                    foreach (XmlNode node2 in node1.ChildNodes)
                    {
                        //Used to test if the instance name is a duplicate
                        var ok = true;

                        //We only need the instance names
                        if (node2.Name != "Name") continue;

                        foreach (var namesNode in namesNodes)
                        {
                            if (namesNode.InnerText == node2.InnerText)
                            {
                                nodesToRemove.Add(node1);
                                ok = false;
                                break;
                            }

                        }
                        if (ok)
                            namesNodes.Add(node2);

                    }
                }

                //Iterate over our list of nodes to kiss goodbye!
                foreach (var xmlNode in nodesToRemove)
                {
                    //Remove all children
                    xmlNode.RemoveAll();
                    //Sayonara node!
                    node.RemoveChild(xmlNode);
                }

                nodesToRemove.Clear();

                //Save out changes, were done pruning
                var writer = new XmlTextWriter(path, Encoding.UTF8);

                //prevent data from being appended
                writer.BaseStream.SetLength(0);
                //pretty print
                writer.Formatting = Formatting.Indented;
                oldXmlDoc.Save(writer);
                writer.Close();

                //If the merge was successful remove the temporary file
                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(oldPath);
                        File.Delete(newPath);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Instances.old.xml and/or Instances.new.xml deletion failed.");
                    }

                }

            }
            else
                Console.WriteLine("No work to do here!");
        }
	}
}