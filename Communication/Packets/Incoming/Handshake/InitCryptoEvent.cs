namespace Plus.Communication.Packets.Incoming.Handshake
{
    using Encryption;
    using Game.Players;
    using Outgoing.Handshake;

    public class InitCryptoEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new InitCryptoComposer(HabboEncryptionV2.GetRsaDiffieHellmanPrimeKey(), HabboEncryptionV2.GetRsaDiffieHellmanGeneratorKey()));
        }
    }
}