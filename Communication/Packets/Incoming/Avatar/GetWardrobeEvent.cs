namespace Plus.Communication.Packets.Incoming.Avatar
{
    using HabboHotel.GameClients;
    using Outgoing.Avatar;

    internal class GetWardrobeEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new WardrobeComposer(session.GetHabbo().Id));
        }
    }
}
