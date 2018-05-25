namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using Game.Players;
    using Outgoing.Rooms.Settings;

    internal class GetRoomRightsEvent : IPacketEvent
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


            session.SendPacket(new RoomRightsListComposer(instance));
        }
    }
}
