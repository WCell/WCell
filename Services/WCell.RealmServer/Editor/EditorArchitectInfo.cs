using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Editor.Figurines;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Editor
{
	/// <summary>
	/// Represents a Character who works with an editor
	/// </summary>
	public class EditorArchitectInfo
	{
		public EditorArchitectInfo(Character chr)
		{
			Character = chr;
		}

		public Character Character
		{
			get;
			private set;
		}

		public MapEditor Editor
		{
			get;
			internal set;
		}

		public EditorFigurine CurrentTarget
		{
			get;
			internal set;
		}
	}
}
