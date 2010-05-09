using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;
using WCell.Util.Threading;

namespace WCell.Core.Paths
{
	public class PathQuery
	{
		private Vector3 from, to;
		private readonly IContextHandler m_ContextHandler;
		private Action<PathQuery> callback;

		public PathQuery(Vector3 from, ref Vector3 to, IContextHandler contextHandler, Action<PathQuery> callback)
		{
			this.from = from;
			this.to = to;
			m_ContextHandler = contextHandler;
			this.callback = callback;
		}

		public PathQuery(Vector3 from, Vector3 to, IContextHandler contextHandler, Action<PathQuery> callback)
		{
			this.from = from;
			this.to = to;
			m_ContextHandler = contextHandler;
			this.callback = callback;
		}

		public Vector3 From
		{
			get { return from; }
		}

		public Vector3 To
		{
			get { return to; }
		}

		public IContextHandler ContextHandler
		{
			get { return m_ContextHandler; }
		}

		public Action<PathQuery> Callback
		{
			get { return callback; }
		}

		public Path Path
		{
			get;
			private set;
		}

		public void Reply(Path path)
		{
		    Path = path;

			if (m_ContextHandler != null)
			{
				m_ContextHandler.ExecuteInContext(() => callback(this));
			}
			else
			{
				callback(this);
			}
		}
	}
}
