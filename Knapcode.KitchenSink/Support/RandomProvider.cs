using System;
using System.Threading;

namespace Knapcode.KitchenSink.Support
{
    /// <summary>
    /// A thread-safe random provider.
    /// From http://csharpindepth.com/Articles/Chapter12/Random.aspx
    /// </summary>
    public static class RandomProvider
    {
        private static int _seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> RandomWrapper = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        public static Random GetThreadRandom()
        {
            return RandomWrapper.Value;
        }
    } 
}
