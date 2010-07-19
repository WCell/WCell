using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Factions;
using WCell.Util;
using WCell.Util.Threading;

namespace WCell.RealmServer.Entities
{
	public interface ITransportInfo : IFactionMember, IWorldLocation, INamedEntity, IContextHandler
	{
		float Orientation
		{
			get;
		}
	}
}