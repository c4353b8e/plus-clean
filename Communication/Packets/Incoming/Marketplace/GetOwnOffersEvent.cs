namespace Plus.Communication.Packets.Incoming.Marketplace
{
    using HabboHotel.GameClients;
    using Outgoing.Marketplace;

    internal class GetOwnOffersEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new MarketPlaceOwnOffersComposer(session.GetHabbo().Id));
        }
    }
}
