using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.RealmServer.Tests.Network;
using WCell.Constants;
using WCell.RealmServer.Network;
using NHibernate.Mapping;
using WCell.Util;

namespace WCell.RealmServer.Tests.Stress
{
	/// <summary>
	/// This class defines a set of random actions that will be randomly called upon the server to emulate
	/// a chaos-stresstest.
	/// </summary>
	public class Randomizer
	{
		public static RealmServerOpCode[] OpCodes;

		private static RealmServerOpCode[] m_opCodePool;
		private static int m_opCodeIndex;

		public Randomizer()
		{
		}

		public void Initialize()
		{
			var handlers = RealmPacketMgr.Instance.Handlers;
			var opcodes = new List<RealmServerOpCode>(500);
			for (int i = 0; i < handlers.Length; i++)
			{
				if (handlers[i] != null)
					opcodes.Add((RealmServerOpCode)i);
			}
			OpCodes = opcodes.ToArray();
			RefillOpCodePool();
		}

		public static RealmServerOpCode GetRandomOpCode()
		{
			if (m_opCodeIndex == m_opCodePool.Length)
			{
				RefillOpCodePool();
			}
			return m_opCodePool[m_opCodeIndex++];
		}

		private static void RefillOpCodePool()
		{
			m_opCodeIndex = 0;
			m_opCodePool = OpCodes.ToArray();
			m_opCodePool.Shuffle();
		}

		public void SendRandomPacket(TestFakeClient client)
		{

		}
	}
}
