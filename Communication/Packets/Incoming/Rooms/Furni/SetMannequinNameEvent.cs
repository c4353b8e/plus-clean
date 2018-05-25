namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using System;
    using System.Linq;
    using HabboHotel.GameClients;

    internal class SetMannequinNameEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var room = session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(session, true))
            {
                return;
            }

            var itemId = packet.PopInt();
            var name = packet.PopString();

            var item = session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
            {
                return;
            }

            if (item.ExtraData.Contains(Convert.ToChar(5)))
            {
                var flags = item.ExtraData.Split(Convert.ToChar(5));
                item.ExtraData = flags[0] + Convert.ToChar(5) + flags[1] + Convert.ToChar(5) + name;
            }
            else
            {
                item.ExtraData = "m" + Convert.ToChar(5) + ".ch-210-1321.lg-285-92" + Convert.ToChar(5) + "Default Mannequin";
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `items` SET `extra_data` = @Ed WHERE `id` = @itemId LIMIT 1");
                dbClient.AddParameter("itemId", item.Id);
                dbClient.AddParameter("Ed", item.ExtraData);
                dbClient.RunQuery();
            }

            item.UpdateState(true, true);
        }
    }
}
