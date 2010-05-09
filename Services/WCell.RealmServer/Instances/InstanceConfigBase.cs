using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using NLog;
using WCell.Util;
using WCell.Util.NLog;

namespace WCell.RealmServer.Instances
{
	public interface IInstanceConfig
	{
		void Setup();
	}

	public abstract class InstanceConfigBase<T, E> : XmlConfig<T>, IInstanceConfig
		where T : XmlConfigBase, IInstanceConfig, new()
		where E : IComparable
	{
		private static string filename;

		public static string Filename
		{
			get { return filename; }
		}

		protected static T LoadSettings(string fileName)
		{
			filename = RealmServerConfiguration.GetContentPath(fileName);

			T settings;
			if (File.Exists(filename))
			{
				settings = Load(filename);
			}
			else
			{
				settings = new T();
			}

			settings.Setup();

			try
			{
				settings.SaveAs(filename);
			}
			catch (Exception e)
			{
				LogUtil.WarnException(e, "Unable to save Instance Config");
			}

			return settings;
		}

		private Dictionary<E, InstanceSetting<E>> m_Settings = new Dictionary<E, InstanceSetting<E>>();
		private InstanceSetting<E>[] m_entries;

		public InstanceConfigBase()
		{
		}

		[XmlElement("Setting")]
		public InstanceSetting<E>[] Entries
		{
			get { return m_entries; }
			set
			{
				m_entries = value;
				SortSettings();
			}
		}

		[XmlIgnore]
		public Dictionary<E, InstanceSetting<E>> Settings
		{
			get { return m_Settings; }
			set
			{
				m_Settings = value;
			}
		}

		[XmlIgnore]
		public abstract IEnumerable<E> SortedIds
		{
			get;
		}

		public InstanceSetting<E> GetSetting(E id)
		{
			InstanceSetting<E> setting;
			m_Settings.TryGetValue(id, out setting);
			return setting;
		}

		protected abstract void InitSetting(InstanceSetting<E> setting);

		public void Setup()
		{
			if (Entries == null)
			{
				SortSettings();
			}
			else
			{
				foreach (var setting in Settings.Values)
				{
					if (setting != null && setting.TypeName.Trim().Length > 0)
					{
						InitSetting(setting);
					}
				}
			}
		}

		private void SortSettings()
		{
			if (Entries != null)
			{
				foreach (var setting in Entries)
				{
					if (setting != null)
					{
						Settings[setting.Name] = setting;
					}
				}
			}

			CreateStubs();

			m_entries = m_Settings.Values.ToArray();
			Array.Sort(m_entries);
		}

		private void CreateStubs()
		{
			CreateStubs(SortedIds);
		}

		private void CreateStubs(IEnumerable<E> sortedIds)
		{
			foreach (var id in sortedIds)
			{
				var setting = GetSetting(id);
				if (setting == null)
				{
					Settings[id] = new InstanceSetting<E>(id, " ");
				}
			}
		}
	}

	public class InstanceSetting<E> : IComparable
		where E : IComparable
	{
		private string m_TypeName;

		public InstanceSetting()
		{
			TypeName = " ";
		}

		public InstanceSetting(E id)
			: this(id, " ")
		{
		}

		public InstanceSetting(E id, string typeName)
		{
			Name = id;
			TypeName = typeName;
		}

		[XmlElement("Name")]
		public E Name
		{
			get;
			set;
		}

		[XmlElement("Type")]
		public string TypeName
		{
			get { return m_TypeName; }
			set { m_TypeName = value; }
		}

		public int CompareTo(object obj)
		{
			var setting = obj as InstanceSetting<E>;
			if (setting != null)
			{
				return Name.CompareTo(setting.Name);
			}
			return -1;
		}
	}
}
