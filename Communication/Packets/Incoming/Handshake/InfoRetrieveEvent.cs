namespace Plus.Communication.Packets.Incoming.Handshake
{
    using HabboHotel.GameClients;
    using Outgoing.Handshake;

    public class InfoRetrieveEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new UserObjectComposer(session.GetHabbo()));
            session.SendPacket(new UserPerksComposer(session.GetHabbo()));
        }
    }
}