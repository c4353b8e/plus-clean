namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using System.Collections.Generic;
    using System.Linq;
    using Game.Players;
    using Outgoing.Rooms.Engine;
    using Outgoing.Rooms.Permissions;
    using Outgoing.Rooms.Settings;

    internal class RemoveAllRightsEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var instance))
            {
                return;
            }

            if (!instance.CheckRights(session, true))
            {
                return;
            }

            foreach (var userId in new List<int>(instance.UsersWithRights))
            {
                var user = instance.GetRoomUserManager().GetRoomUserByHabbo(userId);
                if (user != null && !user.IsBot)
                {
                    user.RemoveStatus("flatctrl 1");
                    user.UpdateNeeded = true;

                    user.GetClient().SendPacket(new YouAreControllerComposer(0));
                }

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `room_rights` WHERE `user_id` = @uid AND `room_id` = @rid LIMIT 1");
                    dbClient.AddParameter("uid", userId);
                    dbClient.AddParameter("rid", instance.Id);
                    dbClient.RunQuery();
                }

                session.SendPacket(new FlatControllerRemovedComposer(instance, userId));
                session.SendPacket(new RoomRightsListComposer(instance));
                session.SendPacket(new UserUpdateComposer(instance.GetRoomUserManager().GetUserList().ToList()));
            }

            if (instance.UsersWithRights.Count > 0)
            {
                instance.UsersWithRights.Clear();
            }
        }
    }
}
