using WCell.RealmServer.Global;
using WCell.RealmServer.Waypoints;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Util.Graphics;

namespace WCell.Tools.Commands
{
    public class WPCommand : ToolCommand
    {
        protected override void Initialize()
        {
            Init("WPs");
        }

        public class RandomWPCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Random", "R");
                EnglishParamInfo = "[<min> [<max> [<mindist> [<maxdist>]]]]";
            }

            public override void Process(CmdTrigger<ToolCmdArgs> trigger)
            {
                var text = trigger.Text;
                var min = text.NextInt(3);
                var max = text.NextInt(5);
                var minDist = text.NextFloat(5);
                var maxDist = text.NextFloat(10);

                var gen = new RandomWaypointGenerator();
                var wps = gen.GenerateWaypoints(new EmptyTerrain(), Vector3.Zero, min, max, minDist, maxDist);
                trigger.Reply(wps.ToString("\n"));
            }
        }
    }
}