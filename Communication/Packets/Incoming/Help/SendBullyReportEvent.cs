namespace Plus.Communication.Packets.Incoming.Help
{
    using HabboHotel.GameClients;
    using Outgoing.Help;

    internal class SendBullyReportEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new SendBullyReportComposer());
        }
    }
}
