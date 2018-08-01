using System;
using System.Threading;
using JetBrains.Annotations;


namespace Dccelerator.UnRandomObjects
{

    /// <summary>
    /// An multithreaded static <see cref="Random"/>.
    /// </summary>
    /// <remarks>
    /// Originally posted <see href="http://stackoverflow.com/a/19271062">on stackoverflow</see> by <see href="https://stackoverflow.com/users/2655826/alessandro-dandria">Alessandro D'Andria</see>
    /// </remarks>
    public static class StaticRandom
    {
        static int _seed = Environment.TickCount;

        static readonly ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        public static int Next() {
            return _random.Value.Next();
        }

        public static int Next(int max) {
            return _random.Value.Next(max);
        }

        public static int Next(int min, int max) {
            return _random.Value.Next(min, max);
        }


        public static double NextDouble() {
            return _random.Value.NextDouble();
        }

        public static void NextBytes([NotNull] byte[] buffer) {
            _random.Value.NextBytes(buffer);
        }
    }
}