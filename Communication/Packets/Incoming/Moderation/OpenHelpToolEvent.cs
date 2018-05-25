namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using Outgoing.Moderation;

    internal class OpenHelpToolEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new OpenHelpToolComposer());
        }
    }
}
