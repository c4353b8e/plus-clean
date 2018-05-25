namespace Plus.Communication.Packets.Incoming.Misc
{
    using Game.Players;

    internal class ClientVariablesEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var gordanPath = packet.PopString();
            var externalVariables = packet.PopString();
        }
    }
}
