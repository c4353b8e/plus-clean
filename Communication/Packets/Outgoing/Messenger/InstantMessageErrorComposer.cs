namespace Plus.Communication.Packets.Outgoing.Messenger
{
    using Game.Users.Messenger;

    internal class InstantMessageErrorComposer : ServerPacket
    {
        public InstantMessageErrorComposer(MessengerMessageErrors Error, int Target)
            : base(ServerPacketHeader.InstantMessageErrorMessageComposer)
        {
            WriteInteger(MessengerMessageErrorsUtility.GetMessageErrorPacketNum(Error));
            WriteInteger(Target);
           WriteString("");
        }
    }
}
