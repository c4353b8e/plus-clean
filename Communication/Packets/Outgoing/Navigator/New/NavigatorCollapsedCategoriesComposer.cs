namespace Plus.Communication.Packets.Outgoing.Navigator.New
{
    internal class NavigatorCollapsedCategoriesComposer : ServerPacket
    {
        public NavigatorCollapsedCategoriesComposer()
            : base(ServerPacketHeader.NavigatorCollapsedCategoriesMessageComposer)
        {
            WriteInteger(0);
        }
    }
}
