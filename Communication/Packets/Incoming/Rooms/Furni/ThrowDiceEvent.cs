namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using HabboHotel.GameClients;

    internal class ThrowDiceEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            var item = room.GetRoomItemHandler().GetItem(packet.PopInt());
            if (item == null)
            {
                return;
            }

            var hasRights = room.CheckRights(session, false, true);

            var request = packet.PopInt();

            item.Interactor.OnTrigger(session, item, request, hasRights);
        }
    }
}
