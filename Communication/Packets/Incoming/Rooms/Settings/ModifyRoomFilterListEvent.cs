namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using Game.Players;

    internal class ModifyRoomFilterListEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var instance = session.GetHabbo().CurrentRoom;
            if (instance == null)
            {
                return;
            }

            if (!instance.CheckRights(session))
            {
                return;
            }

            packet.PopInt(); //roomId
            var added = packet.PopBoolean();
            var word = packet.PopString();

            if (added)
            {
                instance.GetFilter().AddFilter(word);
            }
            else
            {
                instance.GetFilter().RemoveFilter(word);
            }
        }
    }
}
