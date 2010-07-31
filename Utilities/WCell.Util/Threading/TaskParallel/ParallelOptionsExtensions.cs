using System.Threading.Tasks;

namespace WCell.Util.Threading.TaskParallel
{
    /// <summary>Extension methods for ParallelOptions.</summary>
    public static class ParallelOptionsExtensions
    {
        /// <summary>Copies a ParallelOptions instance to a shallow clone.</summary>
        /// <param name="options">The options to be cloned.</param>
        /// <returns>The shallow clone.</returns>
        public static ParallelOptions ShallowClone(this ParallelOptions options)
        {
            return new ParallelOptions()
            {
                CancellationToken = options.CancellationToken,
                MaxDegreeOfParallelism = options.MaxDegreeOfParallelism,
                TaskScheduler = options.TaskScheduler
            };
        }
    }
}