namespace Plus.Communication.Packets.Incoming.Help
{
    using Game.Players;

    internal class GetSanctionStatusEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            //Session.SendMessage(new SanctionStatusComposer());
        }
    }
}
