namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Wired
{
    using HabboHotel.GameClients;
    using HabboHotel.Items.Wired;
    using Outgoing.Rooms.Furni.Wired;

    internal class SaveWiredConfigEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
            {
                return;
            }

            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(session, false, true))
            {
                return;
            }

            var itemId = packet.PopInt();

            session.SendPacket(new HideWiredConfigComposer());

            var selectedItem = room.GetRoomItemHandler().GetItem(itemId);
            if (selectedItem == null)
            {
                return;
            }

            if (!session.GetHabbo().CurrentRoom.GetWired().TryGet(itemId, out var box))
            {
                return;
            }

            if (box.Type == WiredBoxType.EffectGiveUserBadge && !session.GetHabbo().GetPermissions().HasRight("room_item_wired_rewards"))
            {
                session.SendNotification("You don't have the correct permissions to do this.");
                return;
            }

            box.HandleSave(packet);
            session.GetHabbo().CurrentRoom.GetWired().SaveBox(box);
        }
    }
}
