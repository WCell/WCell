using System;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.Data;
using System.Collections.Generic;
using WCell.RealmServer.Content;

namespace WCell.RealmServer.Gossips
{
	/// <summary>
	/// The Id is used by the client to find this entry in its cache.
	/// </summary>
	public interface IGossipEntry
	{
		uint GossipId { get; }

		GossipTextBase[] GossipTexts { get; }

		/// <summary>
		/// dynamic gossip entries don't cache their texts
		/// </summary>
		bool IsDynamic { get; }
	}

	#region GossipText
	public class StaticGossipText : GossipTextBase
	{
		public string TextMale, TextFemale;

		public StaticGossipText()
		{
		}

		public StaticGossipText(string text, float probability, ChatLanguage lang = ChatLanguage.Universal) :
			base(probability, lang)
		{
			TextMale = TextFemale = text;
		}

		public override string GetTextMale(GossipConversation convo)
		{
			return TextMale;
		}
		public override string GetTextFemale(GossipConversation convo)
		{
			return TextFemale;
		}

		public override string ToString()
		{
			return "Text: " + TextFemale;	// female and male should both be about equivalent
		}
	}

	public delegate string GossipStringFactory(GossipConversation convo);
	public class DynamicGossipText : GossipTextBase
	{
		public DynamicGossipText(GossipStringFactory stringGetter, float probability = 1f, ChatLanguage lang = ChatLanguage.Universal)
			: base(probability, lang)
		{
			StringGetter = stringGetter;
		}

		public GossipStringFactory StringGetter
		{
			get;
			set;
		}

		public override string GetTextMale(GossipConversation convo)
		{
			if (convo == null) return "<invalid context>";
			return GetTextFemale(convo);
		}

		public override string GetTextFemale(GossipConversation convo)
		{
			if (convo == null) return "<invalid context>";
			return StringGetter(convo);
		}
	}

	public abstract class GossipTextBase
	{
		protected GossipTextBase()
		{
		}

		protected GossipTextBase(float probability, ChatLanguage lang = ChatLanguage.Universal)
		{
			Probability = probability;
			Language = lang;
		}

		/// <summary>
		/// $N = Character name
		/// </summary>
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

		public abstract string GetTextMale(GossipConversation convo);

		public abstract string GetTextFemale(GossipConversation convo);
	}
	#endregion

	#region GossipEntry
	public abstract class GossipEntry : IGossipEntry
	{
		public uint GossipId { get; set; }

		protected GossipTextBase[] m_Texts;

		protected GossipEntry()
		{
		}

		public abstract bool IsDynamic { get; }

		/// <summary>
		/// The texts of the StaticGossipEntry DataHolder are actually of type StaticGossipText
		/// </summary>
		[Persistent(8, ActualType = typeof(StaticGossipText))]
		public GossipTextBase[] GossipTexts
		{
			get { return m_Texts; }
			set { m_Texts = value; }
		}

		public uint GetId()
		{
			return GossipId;
		}

		public override string ToString()
		{
			return GetType().Name + " (Id: " + GossipId + ")";
		}
	}
	#endregion

	#region StaticGossipEntry
	/// <summary>
	/// Cacheable GossipEntry from DB
	/// </summary>
	[DataHolder]
	public class StaticGossipEntry : GossipEntry, IDataHolder
	{
		public StaticGossipEntry()
		{
			GossipTexts = new StaticGossipText[8];
			for (int i = 0; i < GossipTexts.Length; i++)
			{
				GossipTexts[i] = new StaticGossipText();
			}
		}

		public StaticGossipEntry(uint id, params string[] texts)
		{
			GossipId = id;
			GossipTexts = new StaticGossipText[texts.Length];
			var chance = 1f / texts.Length;
			for (var i = 0; i < texts.Length; i++)
			{
				GossipTexts[i] = new StaticGossipText(texts[i], chance);
			}
			FinalizeDataHolder();
		}

		public StaticGossipEntry(uint id, ChatLanguage lang, params string[] texts)
		{
			GossipId = id;
			GossipTexts = new StaticGossipText[texts.Length];
			var chance = 1f / texts.Length;
			for (var i = 0; i < texts.Length; i++)
			{
				var text = texts[i];
				GossipTexts[i] = new StaticGossipText(text, chance, lang);
			}
			FinalizeDataHolder();
		}

		public StaticGossipEntry(uint id, params StaticGossipText[] entries)
		{
			GossipId = id;
			GossipTexts = entries;
			FinalizeDataHolder();
		}

		/// <summary>
		/// GossipEntry's from DB are always cached
		/// </summary>
		public override bool IsDynamic { get { return false; } }

		public StaticGossipText GetText(int i)
		{
			return (StaticGossipText)m_Texts[i];
		}

		public void FinalizeDataHolder()
		{
			if (m_Texts == null)
			{
				ContentMgr.OnInvalidDBData("Entries is null in: " + this);
				return;
			}

			if (GossipId > 0)
			{
				m_Texts = m_Texts.Where(
					entry => !string.IsNullOrEmpty(((StaticGossipText)entry).TextFemale) || !string.IsNullOrEmpty(((StaticGossipText)entry).TextMale)
				).ToArray();

				GossipMgr.GossipEntries[GossipId] = this;

				foreach (StaticGossipText entry in m_Texts)
				{
					var isMaleTextEmpty = string.IsNullOrEmpty(entry.TextMale);
					var isFemaleTextEmpty = string.IsNullOrEmpty(entry.TextFemale);

					if (isMaleTextEmpty && isFemaleTextEmpty)
					{
						entry.TextMale = " ";
						entry.TextFemale = " ";
					}
					else if (isMaleTextEmpty)
					{
						entry.TextMale = entry.TextFemale;
					}
					else if (isFemaleTextEmpty)
					{
						entry.TextFemale = entry.TextMale;
					}
				}
			}
			else
			{
				ContentMgr.OnInvalidDBData("Invalid id: " + this);
			}
		}

		public DataHolderState DataHolderState
		{
			get;
			set;
		}

		public static IEnumerable<StaticGossipEntry> GetAllDataHolders()
		{
			var list = new List<StaticGossipEntry>(GossipMgr.GossipEntries.Count);
			list.AddRange(GossipMgr.GossipEntries.Values.Where(entry => entry is StaticGossipEntry).OfType<StaticGossipEntry>());
			return list;
		}
	}
	#endregion

	#region DynamicGossipEntry
	public class DynamicGossipEntry : GossipEntry
	{
		public DynamicGossipEntry(uint id, params GossipStringFactory[] texts)
		{
			GossipId = id;
			GossipTexts = new DynamicGossipText[texts.Length];
			var chance = 1f / texts.Length;
			for (var i = 0; i < texts.Length; i++)
			{
				GossipTexts[i] = new DynamicGossipText(texts[i], chance);
			}
		}

		public DynamicGossipEntry(uint id, ChatLanguage lang, params GossipStringFactory[] texts)
		{
			GossipId = id;
			GossipTexts = new DynamicGossipText[texts.Length];
			var chance = 1f / texts.Length;
			for (var i = 0; i < texts.Length; i++)
			{
				GossipTexts[i] = new DynamicGossipText(texts[i], chance, lang);
			}
		}

		public DynamicGossipEntry(uint id, params DynamicGossipText[] entries)
		{
			GossipId = id;
			GossipTexts = entries;
		}

		public override bool IsDynamic
		{
			get { return true; }
		}

		public DynamicGossipText GetText(int i)
		{
			return (DynamicGossipText)m_Texts[i];
		}
	}
	#endregion

	#region NPCGossipRelation
	[DataHolder]
	public class NPCGossipRelation : IDataHolder
	{
		public uint NPCSpawnId;

		public uint GossipId;

		public uint GetId()
		{
			return NPCSpawnId;
		}

		public DataHolderState DataHolderState
		{
			get;
			set;
		}

		public void FinalizeDataHolder()
		{
			var spawn = NPCMgr.GetSpawnEntry(NPCSpawnId);
			var gossipEntry = GossipMgr.GetEntry(GossipId);

			if (spawn == null)
			{
				ContentMgr.OnInvalidDBData("{0} refers to invalid spawn id: {1}", GetType(), this);
			}
			else if (gossipEntry == null)
			{
				ContentMgr.OnInvalidDBData("{0} has invalid GossipId: {1}", GetType(), this);
			}
			else
			{
				var entry = spawn.Entry;
				if (spawn.DefaultGossip == null)
				{
					spawn.DefaultGossip = new GossipMenu(gossipEntry);
				}
				else
				{
					spawn.DefaultGossip.GossipEntry = gossipEntry;
				}

				//entry.NPCFlags |= NPCFlags.Gossip;
			}
		}

		public override string ToString()
		{
			return string.Format("NPC: {0} <-> Gossip id: {1}", NPCSpawnId, GossipId);
		}
	}
	#endregion
}