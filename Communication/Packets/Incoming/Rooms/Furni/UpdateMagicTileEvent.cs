namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using System;
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Engine;
    using Outgoing.Rooms.Furni;

    internal class UpdateMagicTileEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            if (!room.CheckRights(session, false, true) && !session.GetHabbo().GetPermissions().HasRight("room_item_use_any_stack_tile"))
            {
                return;
            }

            var itemId = packet.PopInt();
            var decimalHeight = packet.PopInt();

            var item = room.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
            {
                return;
            }

            item.GetZ = decimalHeight / 100.0;

            room.SendPacket(new ObjectUpdateComposer(item, Convert.ToInt32(session.GetHabbo().Id)));
            room.SendPacket(new UpdateMagicTileComposer(itemId, decimalHeight));
        }
    }
}
