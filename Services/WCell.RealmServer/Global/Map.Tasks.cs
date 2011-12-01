using WCell.RealmServer.Entities;
using WCell.Util.Threading;

namespace WCell.RealmServer.Global
{
	public partial class Map
	{
		public static IMessage GetInitializeCharacterTask(Character chr, Map rgn)
		{
			var initTask = new Message2<Character, Map>();

			initTask.Parameter1 = chr;
			initTask.Parameter2 = rgn;

			initTask.Callback = ((initChr, initRgn) =>
			{
				initRgn.AddObjectNow(chr);
				//Map.s_log.Debug("Owner added to the map");

				//Map.s_log.Debug("Owner initialized");
			});

			return initTask;
		}

		public static IMessage GetRemoveObjectTask(WorldObject obj, Map rgn)
		{
		    var moveTask = new Message2<WorldObject, Map>();

			moveTask.Parameter1 = obj;
			moveTask.Parameter2 = rgn;

            moveTask.Callback = ((worldObj, objRgn) =>
            {
                objRgn.RemoveObjectNow(worldObj);
            });

			return moveTask;
		}
	}
}