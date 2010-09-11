using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.Data;
using System.Collections.Generic;
using WCell.RealmServer.Content;

namespace WCell.RealmServer.Gossips
{
	public class GossipText
	{
		public GossipText()
		{
		}

		public GossipText(string text, float probability)
		{
			TextMale = TextFemale = text;
			Probability = probability;
			Language = ChatLanguage.Universal;
		}

		public GossipText(string text, float probability, ChatLanguage lang)
		{
			TextMale = TextFemale = text;
			Probability = probability;
			Language = lang;
		}

		/// <summary>
		/// $N = Character name
		/// </summary>
		public string TextMale, TextFemale;
		public ChatLanguage Language;
		public float Probability;

		[NotPersistent]
		public EmoteType[] Emotes = new EmoteType[6];

		public EmoteType Emote1
		{
			get { return Emotes[0]; }
			set { Emotes[0] = value; }
		}

		public EmoteType Emote2
		{
			get { return Emotes[1]; }
			set { Emotes[1] = value; }
		}

		public EmoteType Emote3
		{
			get { return Emotes[2]; }
			set { Emotes[2] = value; }
		}

		public EmoteType Emote4
		{
			get { return Emotes[3]; }
			set { Emotes[3] = value; }
		}

		public EmoteType Emote5
		{
			get { return Emotes[4]; }
			set { Emotes[4] = value; }
		}

		public EmoteType Emote6
		{
			get { return Emotes[5]; }
			set { Emotes[5] = value; }
		}
	}

	/// <summary>
	/// The Id is used by the client to find this entry in its cache.
	/// </summary>
	public interface IGossipEntry
	{
		uint GossipId { get; }

		GossipText[] GossipEntries { get; }
	}

	[DataHolder]
	public class GossipEntry : IDataHolder, IGossipEntry
	{
		public uint GossipId { get; set; }

		private GossipText[] m_Entries;

		public GossipEntry()
		{
		}
		
		public GossipEntry(uint id, string text)
		{
			GossipId = id;
			GossipEntries = new[] { new GossipText(text, 1) };
			FinalizeDataHolder();
		}

		public GossipEntry(uint id, params string[] texts)
		{
			GossipId = id;
			GossipEntries = new GossipText[texts.Length];
			var chance = 1f / texts.Length;
			for (var i = 0; i < texts.Length; i++)
			{
				var text = texts[i];
				GossipEntries[i] = new GossipText(text, chance);
			}
			FinalizeDataHolder();
		}

		public GossipEntry(uint id, ChatLanguage lang, params string[] texts)
		{
			GossipId = id;
			GossipEntries = new GossipText[texts.Length];
			var chance = 1f / texts.Length;
			for (var i = 0; i < texts.Length; i++)
			{
				var text = texts[i];
				GossipEntries[i] = new GossipText(text, chance, lang);
			}
			FinalizeDataHolder();
		}

		public GossipEntry(uint id, params GossipText[] entries)
		{
			GossipId = id;
			GossipEntries = entries;
			FinalizeDataHolder();
		}

		[Persistent(8)]
		public GossipText[] GossipEntries
		{
			get { return m_Entries; }
			set { m_Entries = value; }
		}

		public uint GetId()
		{
			return GossipId;
		}

		public DataHolderState DataHolderState
		{
			get;
			set;
		}

		public void FinalizeDataHolder()
		{
			if (m_Entries == null)
			{
				ContentMgr.OnInvalidDBData("Entries is null in: " + this);
				return;
			}

			if (GossipId > 0)
			{
				var list = new List<GossipText>();
				foreach (var entry in m_Entries)
				{
					if (!string.IsNullOrEmpty(entry.TextFemale) || !string.IsNullOrEmpty(entry.TextMale))
					{
						list.Add(entry);
					}
				}
				m_Entries = list.ToArray();

				GossipMgr.NPCTexts[GossipId] = this;
			}
			else
			{
				ContentMgr.OnInvalidDBData("Invalid id: " + this);
			}
		}

		public static IEnumerable<GossipEntry> GetAllDataHolders()
		{
			var list = new List<GossipEntry>(GossipMgr.NPCTexts.Count);
			foreach (var text in GossipMgr.NPCTexts.Values)
			{
				if (text is GossipEntry)
				{
					list.Add((GossipEntry)text);
				}
			}
			return list;
		}

		public override string ToString()
		{
			return GetType().Name + " (Id: " + GossipId + ")";
		}
	}

	[DataHolder]
	public class NPCGossipRelation : IDataHolder
	{
		public NPCId NPCId;

		public uint TextId;

		public uint GetId()
		{
			return (uint)NPCId;
		}

		public DataHolderState DataHolderState
		{
			get;
			set;
		}

		public void FinalizeDataHolder()
		{
			var entry = NPCMgr.GetEntry(NPCId);
			if (entry != null && entry.DefaultGossip == null)
			{
				var menu = new GossipMenu(TextId);
				GossipMgr.gossipCount++;

				entry.DefaultGossip = menu;

				if (entry.NPCFlags.HasAnyFlag(NPCFlags.SpiritHealer))
				{
					entry.NPCFlags |= NPCFlags.Gossip;
				}
				else
				{
					entry.NPCFlags &= ~NPCFlags.Gossip;
				}
			}
		}
	}
}