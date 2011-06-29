using System;
using WCell.RealmServer.Commands;
using WCell.Util.Commands;
using WCell.Util.Graphics;

namespace WCell.Collision.Addon
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

            WorldMap.DumpMapTileChunk(target.MapId, position);
            
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

    public class WMOLOSCommand : RealmServerCommand
    {
        protected WMOLOSCommand () { }


        protected override void Initialize()
        {
            Init("WMOLOS", "LOS");
            EnglishDescription =
                "Checks if there is a building between you and the selected target, and if so returns the distance to that building." + 
                "If no target is selected, uses (x + 10, y + 10, z) as the target position.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var curChar = trigger.Args.Character;
            var target = trigger.Args.SelectedUnitOrGO;

            float? dist;
            if (target != null)
            {
                dist = curChar.Map.Terrain.QueryWMOCollision(curChar.Position, target.Position);
            }
            else
            {
                var targetPos = new Vector3(curChar.Position.X + 10.0f, curChar.Position.Y + 10.0f, curChar.Position.Z);
                dist = curChar.Map.Terrain.QueryWMOCollision(curChar.Position, targetPos);
            }

            if (dist == null)
            {
                trigger.Reply("Has LOS.");
            }
            else
            {
                trigger.Reply("No LOS");
            }
        }
    }

    public class WMOHeightCommand : RealmServerCommand
    {
        protected WMOHeightCommand() { }


        protected override void Initialize()
        {
            Init("WMOHeight", "WMOH");
            EnglishDescription =
                "Checks if there is a building between you and the selected target, and if so returns the distance to that building." +
                "If no target is selected, uses (x + 10, y + 10, z) as the target position.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var curChar = trigger.Args.Character;
            var height = curChar.Map.Terrain.QueryWMOHeight(curChar.Position);

            if (height == null)
            {
                trigger.Reply("No height found.");
            }
            else
            {
                trigger.Reply(String.Format("Height: {0}", height));
            }
        }
    }
}