namespace Plus.Communication.Packets.Incoming.Handshake
{
    using HabboHotel.GameClients;

    public class GetClientVersionEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var build = packet.PopString();

            if (Program.GameContext.GameRevision != build)
            {
                Program.GameContext.GameRevision = build;
            }
        }
    }
}