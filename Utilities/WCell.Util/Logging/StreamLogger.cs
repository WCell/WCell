using System;
using System.IO;

namespace WCell.Util.Logging
{
	public class StreamTarget : TargetWithLayout
	{
		private TextWriter _stream;
		private Layout _streamNameLayout;

		/// <summary>
		/// The network address. Can be tcp://host:port, udp://host:port, http://host:port or https://host:port
		/// </summary>
		[RequiredParameter]
		public TextWriter Stream
		{
			get { return _stream; }
			set
			{
				_stream = value;
			}
		}

		[RequiredParameter]
		public string StreamName
		{
			get { return _streamNameLayout.Text; }
			set
			{
				_streamNameLayout = new Layout(value);
				Name = StreamName;
			}
		}

		/// <summary>
		/// Flushes any buffers.
		/// </summary>
		/// <param name="timeout">Flush timeout.</param>
		public override void Flush(TimeSpan timeout)
		{
			lock (this)
			{
				if (_stream != null)
					_stream.Flush();
			}
		}

		/// <summary>
		/// Adds all layouts used by this target to the specified collection.
		/// </summary>
		/// <param name="layouts">The collection to add layouts to.</param>
		public override void PopulateLayouts(LayoutCollection layouts)
		{
			base.PopulateLayouts(layouts);
			_streamNameLayout.PopulateLayouts(layouts);
		}

		public override void Close()
		{
			try
			{
				base.Close();
			}
			finally
			{
				_stream.Close();
			}
		}

		/// <summary>
		/// Sends the 
		/// rendered logging event over the network optionally concatenating it with a newline character.
		/// </summary>
		/// <param name="logEvent">The logging event.</param>
		public override void Write(LogEventInfo logEvent)
		{
			lock (this)
			{
				var msg = CompiledLayout.GetFormattedMessage(logEvent);
				_stream.WriteLine(msg);
			}
		}
	}
}