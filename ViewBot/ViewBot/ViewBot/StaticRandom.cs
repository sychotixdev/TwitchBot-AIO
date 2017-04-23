using System;
using System.Threading;

namespace TwitchBot.ViewBot
{
    public static class StaticRandom
    {
        private static int _seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> Random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        /// <summary>
        /// Generates a new random int
        /// </summary>
        /// <returns></returns>
        public static int Rand()
        {
            return Random.Value.Next();
        }
    }
}
