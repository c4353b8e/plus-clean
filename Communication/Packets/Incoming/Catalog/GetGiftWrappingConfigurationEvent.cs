namespace Plus.Communication.Packets.Incoming.Catalog
{
    using Game.Players;
    using Outgoing.Catalog;

    public class GetGiftWrappingConfigurationEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new GiftWrappingConfigurationComposer());
        }
    }
}