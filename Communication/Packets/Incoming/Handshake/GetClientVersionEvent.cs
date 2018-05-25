namespace Plus.Communication.Packets.Incoming.Handshake
{
    using Game.Players;

    public class GetClientVersionEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var build = packet.PopString();

            if (Program.GameContext.GameRevision != build)
            {
                Program.GameContext.GameRevision = build;
            }
        }
    }
}