using System.Diagnostics;

namespace WCell.PerformanceCounterInstaller
{
    class Program
    {
        private const string CategoryName = "WCell";
        private const string CategoryHelp = "Performance counters for the WCell server platform.";

        static void Main(string[] args)
        {
            SetupCounters();
        }

        public static void SetupCounters()
        {
            var counterList = new CounterCreationDataCollection();

            var sentPacketCounter = new CounterCreationData
                                        {
                                            CounterName = "Packets Sent/sec",
                                            CounterType = PerformanceCounterType.RateOfCountsPerSecond64,
                                            CounterHelp = "Number of packets sent per second."
                                        };

            var recvPacketCounter = new CounterCreationData
                                        {
                                            CounterName = "Packets Received/sec",
                                            CounterType = PerformanceCounterType.RateOfCountsPerSecond64,
                                            CounterHelp = "Number of packets received per second."
                                        };

            var bytesSentCounter = new CounterCreationData
                                       {
                                           CounterName = "Bytes Sent",
                                           CounterType = PerformanceCounterType.NumberOfItems64,
                                           CounterHelp = "Total number of bytes sent."
                                       };

            var bytesRecvCounter = new CounterCreationData
                                       {
                                           CounterName = "Bytes Received",
                                           CounterType = PerformanceCounterType.NumberOfItems64,
                                           CounterHelp = "Total number of bytes received."
                                       };

            var authQueueCounter = new CounterCreationData
                                       {
                                           CounterName = "Auth Queue Size",
                                           CounterType = PerformanceCounterType.CountPerTimeInterval32,
                                           CounterHelp = "Number of clients waiting in the auth queue."
                                       };

            counterList.Add(sentPacketCounter);
            counterList.Add(recvPacketCounter);
            counterList.Add(bytesSentCounter);
            counterList.Add(bytesRecvCounter);
            counterList.Add(authQueueCounter);

            if (!PerformanceCounterCategory.Exists(CategoryName))
            {
                PerformanceCounterCategory.Create(CategoryName, CategoryHelp,
                                                  PerformanceCounterCategoryType.SingleInstance,
                                                  counterList);
            }
        }
    }
}
