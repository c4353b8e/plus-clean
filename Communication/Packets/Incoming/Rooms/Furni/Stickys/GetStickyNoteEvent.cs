namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Stickys
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using Outgoing.Rooms.Furni.Stickys;

    internal class GetStickyNoteEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            var item = room.GetRoomItemHandler().GetItem(packet.PopInt());
            if (item == null || item.GetBaseItem().InteractionType != InteractionType.POSTIT)
            {
                return;
            }

            session.SendPacket(new StickyNoteComposer(item.Id.ToString(), item.ExtraData));
        }
    }
}