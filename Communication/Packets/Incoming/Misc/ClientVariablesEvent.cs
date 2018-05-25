namespace Plus.Communication.Packets.Incoming.Misc
{
    using HabboHotel.GameClients;

    internal class ClientVariablesEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var gordanPath = packet.PopString();
            var externalVariables = packet.PopString();
        }
    }
}
