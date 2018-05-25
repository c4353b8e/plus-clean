namespace Plus.Communication.Packets.Outgoing.Messenger
{
    using HabboHotel.Users.Messenger;

    internal class FriendNotificationComposer : ServerPacket
    {
        public FriendNotificationComposer(int UserId, MessengerEventTypes type, string data)
            : base(ServerPacketHeader.FriendNotificationMessageComposer)
        {
            WriteString(UserId.ToString());
            WriteInteger(MessengerEventTypesUtility.GetEventTypePacketNum(type));
            WriteString(data);
        }
    }
}
