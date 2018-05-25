namespace Plus.Communication.Packets.Incoming.Catalog
{
    using Game.Players;
    using Outgoing.Catalog;

    internal class GetClubGiftsEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new ClubGiftsComposer());
        }
    }
}
