namespace Plus.Communication.Packets.Incoming.Help
{
    using HabboHotel.GameClients;

    internal class GetSanctionStatusEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            //Session.SendMessage(new SanctionStatusComposer());
        }
    }
}
