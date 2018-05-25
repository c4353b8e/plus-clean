namespace Plus.Communication.Packets.Incoming.Handshake
{
    using Game.Players;
    using Outgoing.Handshake;

    public class InfoRetrieveEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new UserObjectComposer(session.GetHabbo()));
            session.SendPacket(new UserPerksComposer(session.GetHabbo()));
        }
    }
}