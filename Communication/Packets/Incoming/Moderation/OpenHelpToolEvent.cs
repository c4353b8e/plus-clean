namespace Plus.Communication.Packets.Incoming.Moderation
{
    using Game.Players;
    using Outgoing.Moderation;

    internal class OpenHelpToolEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new OpenHelpToolComposer());
        }
    }
}
