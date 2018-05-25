namespace Plus.Communication.Packets.Outgoing.Messenger
{
    using Game.Cache.Type;

    internal class NewBuddyRequestComposer : ServerPacket
    {
        public NewBuddyRequestComposer(UserCache Habbo)
            : base(ServerPacketHeader.NewBuddyRequestMessageComposer)
        {
            WriteInteger(Habbo.Id);
           WriteString(Habbo.Username);
           WriteString(Habbo.Look);
        }
    }
}
