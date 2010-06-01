using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Factions;
using WCell.Util.Collections;
using WCell.RealmServer.Global;
using WCell.RealmServer.Instances;

namespace WCell.RealmServer.Entities
{
	public class SynchronizedCharacterList : SynchronizedList<Character>, ICharacterSet
	{
		public SynchronizedCharacterList(FactionGroup group, ICollection<Character> chrs)
			: base(chrs)
		{
			FactionGroup = group;
		}

		public SynchronizedCharacterList(FactionGroup group)
			: base(5)
		{
			FactionGroup = group;
		}

		public SynchronizedCharacterList(int capacity, FactionGroup group)
			: base(capacity)
		{
			FactionGroup = group;
		}

		public FactionGroup FactionGroup
		{
			get;
			protected set;
		}

		/// <summary>
		/// Threadsafe iteration
		/// </summary>
		/// <param name="callback"></param>
		public void ForeachCharacter(Action<Character> callback)
		{
			EnterLock();

			try
			{
				for (var i = Count - 1; i >= 0; i--)
				{
					var chr = this[i];
					callback(chr);

					if (!chr.IsInWorld)
					{
						RemoveUnlocked(i);
					}
				}
			}
			finally
			{
				ExitLock();
			}
		}

		/// <summary>
		/// Creates a Copy of the set
		/// </summary>
		public Character[] GetCharacters()
		{
			return ToArray();
		}

		public void Send(RealmPacketOut packet)
		{
		    byte[] finalizedPacket = packet.GetFinalizedPacket();
			ForeachCharacter(chr => chr.Send(finalizedPacket));
		}
	}
}
