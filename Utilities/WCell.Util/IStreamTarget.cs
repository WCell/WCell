using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WCell.Util
{
	public interface IStreamTarget
	{
		string Name { get; set; }

		IndentTextWriter Writer { get; }

		void Open();

		void Close();

		void Flush();
	}

	public class FileStreamTarget : IStreamTarget
	{
		private string m_path;
		private IndentTextWriter m_Writer;

		public FileStreamTarget(string name)
		{
			m_path = Path.GetFullPath(name);
		}

		public string Name
		{
			get { return m_path; }
			set
			{
				if (m_Writer != null)
				{
					m_Writer.Close();
				}
				m_path = value;
				m_Writer = new IndentTextWriter(m_path);
			}
		}

		public IndentTextWriter Writer
		{
			get { return m_Writer; }
		}

		public void Flush()
		{
			m_Writer.Flush();
		}

		/// <summary>
		/// Opens a new StreamWriter to the given Path if not already opened.
		/// </summary>
		public void Open()
		{
			if (m_Writer != null)
			{
				Close();
			}
			m_Writer = new IndentTextWriter(m_path);
		}

		public void Close()
		{
			m_Writer.Close();
		}
	}
}
