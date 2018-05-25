namespace Plus.Communication.Packets.Incoming.Help
{
    using Game.Players;

    internal class OnBullyClickEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            //I am a very boring packet.
        }
    }
}
