using WCell.Util.Threading;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Global
{
	public partial class Region
	{
		public static IMessage GetInitializeCharacterTask(Character chr, Region rgn)
		{
			var initTask = new Message2<Character, Region>();

			initTask.Parameter1 = chr;
			initTask.Parameter2 = rgn;

			initTask.Callback = ((initChr, initRgn) =>
			{
				initRgn.AddObjectNow(chr);
				//Region.s_log.Debug("Owner added to the region");

				//Region.s_log.Debug("Owner initialized");
			});

			return initTask;
		}

		public static IMessage GetRemoveObjectTask(WorldObject obj, Region rgn)
		{
		    var moveTask = new Message2<WorldObject, Region>();

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