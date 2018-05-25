namespace Plus.Utilities
{
    using System;

    public static class Randomizer
    {
        public static Random GetRandom { get; } = new Random();
        
        public static int Next(int min, int max)
        {
            return GetRandom.Next(min, max);
        }
        
        public static byte NextByte(int min, int max)
        {
            max = Math.Min(max, 255);
            return (byte)Next(Math.Min(min, max), max);
        }

        public static void NextBytes(byte[] toparse)
        {
            GetRandom.NextBytes(toparse);
        }
    }
}
