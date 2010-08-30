using System.Collections.Concurrent;
using System.Threading;

namespace WCell.Util.Threading.ActorModel
{
    public abstract class Channel<TActor, TMessage> : IChannel
        where TActor : Actor
    {
        protected Channel(TActor actor)
        {
            Actor = actor;

            actor.AddChannel(this);
        }

        public TActor Actor { get; private set; }

        private readonly ConcurrentQueue<TMessage> _queue = new ConcurrentQueue<TMessage>();

        void IChannel.Wait()
        {
            while (!_queue.IsEmpty)
                Thread.SpinWait(1);
        }

        public void Send(TMessage msg)
        {
            if (Actor.Exited)
                return;

            _queue.Enqueue(msg);

            Execute();
        }

        public T Receive<T>(TMessage msg)
        {
            if (Actor.Exited)
                throw new ActorException("Actor has exited.");

            lock (Actor.Lock)
                return (T)OnTwoWayMessage(msg);
        }

        private void Execute()
        {
            if (Actor.Exited)
                return;

            var status = (ActorStatus)Interlocked.CompareExchange(ref Actor.Status, (int)ActorStatus.Executing,
                (int)ActorStatus.Waiting);

            if (status == ActorStatus.Waiting)
                ThreadPool.QueueUserWorkItem(PoolCallback, Actor);
        }

        private void PoolCallback(object state)
        {
            lock (Actor.Lock)
            {
                TMessage msg;
                while (_queue.TryDequeue(out msg))
                    OnOneWayMessage(msg);
            }

            if (!Actor.Exited)
            {
                Interlocked.Exchange(ref Actor.Status, (int)ActorStatus.Waiting);

                if (!_queue.IsEmpty)
                    Execute();
            }
            else
                Interlocked.Exchange(ref Actor.Status, (int)ActorStatus.Exited);
        }

        protected virtual void OnOneWayMessage(TMessage msg)
        {
            throw new ActorException("This actor does not support one-way messages.");
        }

        protected virtual object OnTwoWayMessage(TMessage msg)
        {
            throw new ActorException("This actor does not support two-way messages.");
        }
    }
}
