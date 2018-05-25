namespace Plus.Communication.Packets.Incoming.Handshake
{
    using Encryption;
    using HabboHotel.GameClients;
    using Outgoing.Handshake;

    public class InitCryptoEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new InitCryptoComposer(HabboEncryptionV2.GetRsaDiffieHellmanPrimeKey(), HabboEncryptionV2.GetRsaDiffieHellmanGeneratorKey()));
        }
    }
}