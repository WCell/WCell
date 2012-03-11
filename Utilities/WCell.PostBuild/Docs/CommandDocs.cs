using System;
using System.Collections.Generic;
using System.IO;
using WCell.AuthServer.Commands;
using WCell.RealmServer;
using WCell.RealmServer.Commands;
using WCell.Tools;
using WCell.Tools.Commands;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Util.NLog;
using RealmServ = WCell.RealmServer.RealmServer;

namespace WCell.PostBuild.Docs
{
    public static class CommandDocs
    {
        public const string DefaultFile = "Commands.txt";

        public static void CreateCommandDocs(string dir)
        {
            RealmServ.EntryLocation = Path.GetFullPath(ToolConfig.WCellRealmServerConsoleExe);
            var realmServ = RealmServ.Instance; // make sure to create the RealmServ instance first

            LogUtil.SetupConsoleLogging();

            Console.WriteLine("Output Directory: " + new DirectoryInfo(dir).FullName);

            RealmServerConfiguration.Instance.AutoSave = false;
            RealmServerConfiguration.Initialize();

            RealmCommandHandler.Initialize();
            AuthCommandHandler.Initialize();
            ToolCommandHandler.Initialize();

            CreateCommandDocs(Path.Combine(dir, "RealmServer"), RealmCommandHandler.Instance.Commands);
            CreateCommandDocs(Path.Combine(dir, "AuthServer"), AuthCommandHandler.Instance.Commands);
            CreateCommandDocs(Path.Combine(dir, "Tools"), ToolCommandHandler.Instance.Commands);
        }

        public static void CreateCommandDocs<CA>(string dir, IEnumerable<Command<CA>> cmds)
            where CA : ICmdArgs
        {
            var dirInfo = new DirectoryInfo(dir);
            dirInfo.MKDirs();
            using (var writer = new StreamWriter(Path.Combine(dirInfo.FullName, DefaultFile), false))
            {
                foreach (var cmd in cmds)
                {
                    WriteCommand(writer, cmd, "");
                    writer.WriteLine("########################################");
                    writer.WriteLine();
                }
            }
        }

        public static void WriteCommand<C>(TextWriter writer, BaseCommand<C> cmd, string indent)
            where C : ICmdArgs
        {
            writer.WriteLine(indent + cmd.Name);
            indent += "\t";
            writer.WriteLine(indent + "Aliases: {0}", cmd.Aliases.ToString(", "));

            writer.WriteLine(indent + "Usage: {0}", cmd.CreateUsage());
            writer.WriteLine(indent + "Description: {0}", cmd.GetDescription(null));

            var command = cmd as RealmServerCommand;
            if (command != null)
            {
                writer.WriteLine(indent + "Needs Char: {0}", command.RequiresCharacter);
                writer.WriteLine(indent + "Required Targets: {0}", command.TargetTypes);
            }
            writer.WriteLine();

            if (cmd.SubCommands.Count > 0)
            {
                writer.WriteLine(indent + "SubCommands:");
                writer.WriteLine();
                indent += "\t";
                foreach (var subCmd in cmd.SubCommands)
                {
                    WriteCommand(writer, subCmd, indent);
                }
            }
        }
    }
}