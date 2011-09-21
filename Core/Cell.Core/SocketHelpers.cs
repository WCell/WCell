using System.Net.Sockets;

namespace Cell.Core
{
    public static class SocketHelpers
    {
        //static int s_AcquiredArgs;
        //static int s_ReleasedArgs;
        //static int s_OutstandingArgs;

        static SocketHelpers()
        {
            if (!zzObjectPoolMgr.ContainsType<SocketAsyncEventArgs>())
            {
                zzObjectPoolMgr.RegisterType<SocketAsyncEventArgs>(CreateSocketArg);

                // TODO: have the minimum value set in config or find an appropriate minimum.
                zzObjectPoolMgr.SetMinimumSize<SocketAsyncEventArgs>(100);
            }
        }


        private static SocketAsyncEventArgs CreateSocketArg()
        {
            SocketAsyncEventArgs arg = new SocketAsyncEventArgs();

            // TODO: Check what settings to apply on creation

            return arg;
        }

        private static void CleanSocketArg(SocketAsyncEventArgs arg)
        {
            // TODO: Check what cleanup needs to be done with the arg
        }

        public static SocketAsyncEventArgs AcquireSocketArg()
        {
            //Interlocked.Increment(ref s_OutstandingArgs);
            //Interlocked.Increment(ref s_AcquiredArgs);
            //Console.WriteLine("Acquiring SocketAsyncEventArg {0}:{1}", s_OutstandingArgs, s_AcquiredArgs);
			SocketAsyncEventArgs args = zzObjectPoolMgr.ObtainObject<SocketAsyncEventArgs>();

			CleanSocketArg(args);

			return args;
        }

        public static void ReleaseSocketArg(SocketAsyncEventArgs arg)
        {
            //Interlocked.Increment(ref s_ReleasedArgs);
            //Interlocked.Decrement(ref s_OutstandingArgs);
            //Console.WriteLine("Releasing SocketAsyncEventArg {0}:{1}", s_OutstandingArgs, s_ReleasedArgs);

            zzObjectPoolMgr.ReleaseObject<SocketAsyncEventArgs>(arg);
        }


        public static void SetListenSocketOptions(Socket socket)
        {
            socket.NoDelay = true;
        }
    }
}