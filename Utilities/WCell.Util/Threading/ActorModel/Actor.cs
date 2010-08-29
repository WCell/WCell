using System;
using System.Collections.Concurrent;

namespace WCell.Util.Threading.ActorModel
{
    public abstract class Actor : IDisposable
    {
        private readonly ConcurrentBag<IChannel> _channels = new ConcurrentBag<IChannel>();

        protected Actor()
        {
            Lock = new object();
        }

        internal int Status;

        internal object Lock { get; private set; }

        public bool Exited { get; private set; }

        internal void AddChannel(IChannel channel)
        {
            _channels.Add(channel);
        }

        public void Dispose()
        {
            if (Exited)
                throw new ObjectDisposedException("Actor has already been disposed.");

            foreach (var channel in _channels)
                channel.Wait();

            Exited = true;
        }
    }
}
