namespace Plus.Communication.Encryption
{
    using System.Text;
    using Crypto.RSA;
    using KeyExchange;
    using Keys;
    using Utilities;

    public static class HabboEncryptionV2
    {
        private static RSAKey Rsa;
        private static DiffieHellman DiffieHellman;

        public static void Initialize(RSAKeys keys)
        {
            Rsa = RSAKey.ParsePrivateKey(keys.N, keys.E, keys.D);
            DiffieHellman = new DiffieHellman();
        }

        private static string GetRsaStringEncrypted(string message)
        {
            try
            {
                var m = Encoding.Default.GetBytes(message);
                var c = Rsa.Sign(m);

                return EncryptionUtilities.BytesToHexString(c);
            }
            catch
            {
                return "0";
            }
        }

        public static string GetRsaDiffieHellmanPrimeKey()
        {
            var key = DiffieHellman.Prime.ToString(10);
            return GetRsaStringEncrypted(key);
        }

        public static string GetRsaDiffieHellmanGeneratorKey()
        {
            var key = DiffieHellman.Generator.ToString(10);
            return GetRsaStringEncrypted(key);
        }

        public static string GetRsaDiffieHellmanPublicKey()
        {
            var key = DiffieHellman.PublicKey.ToString(10);
            return GetRsaStringEncrypted(key);
        }

        public static BigInteger CalculateDiffieHellmanSharedKey(string publicKey)
        {
            try
            {
                var cbytes = EncryptionUtilities.HexStringToBytes(publicKey);
                var publicKeyBytes = Rsa.Verify(cbytes);
                var publicKeyString = Encoding.Default.GetString(publicKeyBytes);
                return DiffieHellman.CalculateSharedKey(new BigInteger(publicKeyString, 10));
            }
            catch
            {
                return 0;
            }
        }
    }
}
