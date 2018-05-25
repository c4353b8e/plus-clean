namespace Plus.Utilities
{
    using System;

    public static class EncryptionUtilities
    {
        public static Random GetRandom { get; } = new Random();
        
        public static byte NextByte(int min, int max)
        {
            max = Math.Min(max, 255);
            return (byte)GetRandom.Next(Math.Min(min, max), max);
        }

        public static void NextBytes(byte[] toparse)
        {
            GetRandom.NextBytes(toparse);
        }

        public static int DecodeInt32(byte[] v)
        {
            if ((v[0] | v[1] | v[2] | v[3]) < 0)
            {
                return -1;
            }
            return (v[0] << 0x18) + (v[1] << 0x10) + (v[2] << 8) + v[3];
        }

        public static int DecodeInt16(byte[] v)
        {
            if ((v[0] | v[1]) < 0)
            {
                return -1;
            }
            return (v[0] << 8) + v[1];
        }

        public static string BytesToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        public static byte[] HexStringToBytes(string characters)
        {
            var length = characters.Length;
            var bytes = new byte[length / 2];
            for (var i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(characters.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}
