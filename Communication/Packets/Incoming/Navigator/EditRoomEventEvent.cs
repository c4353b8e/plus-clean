namespace Plus.Communication.Packets.Incoming.Navigator
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Engine;

    internal class EditRoomEventEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var roomId = packet.PopInt();
            var name = Program.GameContext.GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var desc = Program.GameContext.GetChatManager().GetFilter().CheckMessage(packet.PopString());

            if (!RoomFactory.TryGetData(roomId, out var data))
            {
                return;
            }

            if (data.OwnerId != session.GetHabbo().Id)
            {
                return;
            }

            if (data.Promotion == null)
            {
                session.SendNotification("Oops, it looks like there isn't a room promotion in this room?");
                return;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `room_promotions` SET `title` = @title, `description` = @desc WHERE `room_id` = " + roomId + " LIMIT 1");
                dbClient.AddParameter("title", name);
                dbClient.AddParameter("desc", desc);
                dbClient.RunQuery();
            }

            Room room;
            if (!Program.GameContext.GetRoomManager().TryGetRoom(Convert.ToInt32(roomId), out room))
            {
                return;
            }

            data.Promotion.Name = name;
            data.Promotion.Description = desc;
            room.SendPacket(new RoomEventComposer(data, data.Promotion));
        }
    }
}
