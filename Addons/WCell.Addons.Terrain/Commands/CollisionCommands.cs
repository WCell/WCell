using System;
using WCell.RealmServer.Commands;
using WCell.Util.Commands;

namespace WCell.Addons.Terrain.Commands
{
    public class DumpChunkCommand : RealmServerCommand
    {
        protected DumpChunkCommand() { }

        protected override void Initialize()
        {
            Init("DumpChunk", "DmpChnk");
            EnglishDescription = "Dumps the height-map values contained in the current chunk to a text file.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var target = trigger.Args.Character;
            var position = target.Position;

           // WorldMap.DumpMapTileChunk(target.MapId, position);
            
            trigger.Reply(String.Format("Map: {0}", target.Map));

            var zone = target.Zone;
            string zoneStr;
            if (zone != null)
            {
                zoneStr = zone.Id.ToString();

                while ((zone = zone.ParentZone) != null)
                {
                    zoneStr += " in " + zone;
                }
            }
            else
            {
                zoneStr = "<null>";
            }
            trigger.Reply(String.Format("Zone: {0}", zoneStr));
            trigger.Reply(String.Format("Position X: {0}, Y: {1}, Z: {2}, O: {3}", target.Position.X, target.Position.Y,
                                        target.Position.Z, target.Orientation));
            trigger.Reply("Chunk Dumped.");
        }
    }
}