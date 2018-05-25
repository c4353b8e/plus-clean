namespace Plus.Communication.Packets.Incoming.Help
{
    using Game.Players;
    using Outgoing.Help;

    internal class SendBullyReportEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new SendBullyReportComposer());
        }
    }
}
