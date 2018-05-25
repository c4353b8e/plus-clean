namespace Plus.Communication.Packets.Incoming.Handshake
{
    using Encryption;
    using Encryption.Crypto.Prng;
    using HabboHotel.GameClients;
    using Outgoing.Handshake;

    public class GenerateSecretKeyEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Program.EncryptionEnabled = true;

            var cipherPublickey = packet.PopString();
           
            var sharedKey = HabboEncryptionV2.CalculateDiffieHellmanSharedKey(cipherPublickey);
            if (sharedKey != 0)
            {
                session.Rc4Client = new ARC4(sharedKey.getBytes());
                session.SendPacket(new SecretKeyComposer(HabboEncryptionV2.GetRsaDiffieHellmanPublicKey()));
            }
            else 
            {
                session.SendNotification("There was an error logging you in, please try again!");
            }
        }
    }
}