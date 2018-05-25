namespace Plus.Utilities
{
    using System;

    public static class RandomNumber
    {
        private static readonly Random r = new Random();
        private static readonly object l = new object();

        private static readonly Random globalRandom = new Random();
        [ThreadStatic] private static Random localRandom;

        public static int GenerateNewRandom(int min, int max)
        {
            return new Random().Next(min, max + 1);
        }

        public static int GenerateLockedRandom(int min, int max)
        {
            lock (l) // only allow one entry at a time to prevent returning the same number to multiple entries.
            {
                return r.Next(min, max);
            }
        }

        public static int GenerateRandom(int min, int max)
        {
            var inst = localRandom;

            max++;

            if (inst == null)
            {
                int seed;
                lock (globalRandom)
                {
                    seed = globalRandom.Next();
                }
                localRandom = inst = new Random(seed);
            }

            return inst.Next(min, max);
        }
    }
}