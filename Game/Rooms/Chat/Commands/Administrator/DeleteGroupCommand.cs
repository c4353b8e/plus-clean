namespace Plus.Game.Rooms.Chat.Commands.Administrator
{
    using Players;

    internal class DeleteGroupCommand : IChatCommand
    {
        public string PermissionRequired => "command_delete_group";

        public string Parameters => "";

        public string Description => "Delete a group from the database and cache.";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            if (Room.Group == null)
            {
                Session.SendWhisper("Oops, there is no group here?");
                return;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `groups` WHERE `id` = '" + Room.Group.Id + "'");
                dbClient.RunQuery("DELETE FROM `group_memberships` WHERE `group_id` = '" + Room.Group.Id + "'");
                dbClient.RunQuery("DELETE FROM `group_requests` WHERE `group_id` = '" + Room.Group.Id + "'");
                dbClient.RunQuery("UPDATE `rooms` SET `group_id` = '0' WHERE `group_id` = '" + Room.Group.Id + "' LIMIT 1");
                dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `groupid` = '" + Room.Group.Id + "' LIMIT 1");
                dbClient.RunQuery("DELETE FROM `items_groups` WHERE `group_id` = '" + Room.Group.Id + "'");
            }

            Program.GameContext.GetGroupManager().DeleteGroup(Room.Group.Id);

            Room.Group = null;

            Program.GameContext.GetRoomManager().UnloadRoom(Room.Id);

            Session.SendNotification("Success, group deleted.");
        }
    }
}
