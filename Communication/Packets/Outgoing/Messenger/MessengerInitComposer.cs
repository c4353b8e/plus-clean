namespace Plus.Communication.Packets.Outgoing.Messenger
{
    using System;

    internal class MessengerInitComposer : ServerPacket
    {
        public MessengerInitComposer()
            : base(ServerPacketHeader.MessengerInitMessageComposer)
        {
            WriteInteger(Convert.ToInt32(Program.SettingsManager.TryGetValue("messenger.buddy_limit")));//Friends max.
            WriteInteger(300);
            WriteInteger(800);
            WriteInteger(0); // category count
        }
    }
}
