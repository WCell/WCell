using System.Runtime.InteropServices;

namespace Cell.Core
{
    class NativeMethods
    {
        [DllImport("kernel32", ExactSpelling = true)]
        private static extern int SwitchToThread();

        public static void OsSwitchToThread()
        {
#if LINUX
            Thread.SpinWait(1);
#else
            SwitchToThread();
#endif
        }
    }
}
