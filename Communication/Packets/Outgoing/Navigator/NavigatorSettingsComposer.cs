namespace Plus.Communication.Packets.Outgoing.Navigator
{
    internal class NavigatorSettingsComposer : ServerPacket
    {
        public NavigatorSettingsComposer(int homeroom)
            : base(ServerPacketHeader.NavigatorSettingsMessageComposer)
        {
            WriteInteger(homeroom);
            WriteInteger(homeroom);
        }
    }
}
