namespace Plus.Communication.Packets.Outgoing.Users
{
    internal class UserTagsComposer : ServerPacket
    {
        public UserTagsComposer(int userId)
            : base(ServerPacketHeader.UserTagsMessageComposer)
        {
            WriteInteger(userId);
            WriteInteger(0);//Count of the tags.
            {
                //Append a string.
            }
        }
    }
}
