namespace Plus.Game.Rooms.Chat.Commands.Moderator
{
    using System.Linq;
    using Players;

    internal class RoomKickCommand : IChatCommand
    {
        public string PermissionRequired => "command_room_kick";

        public string Parameters => "%message%";

        public string Description => "Kick the room and provide a message to the users.";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please provide a reason to the users for this room kick.");
                return;
            }

            var Message = CommandManager.MergeParams(Params, 1);
            foreach (var RoomUser in Room.GetRoomUserManager().GetUserList().ToList())
            {
                if (RoomUser == null || RoomUser.IsBot || RoomUser.GetClient() == null || RoomUser.GetClient().GetHabbo() == null || RoomUser.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool") || RoomUser.GetClient().GetHabbo().Id == Session.GetHabbo().Id)
                {
                    continue;
                }

                RoomUser.GetClient().SendNotification("You have been kicked by a moderator: " + Message);

                Room.GetRoomUserManager().RemoveUserFromRoom(RoomUser.GetClient(), true, false);
            }

            Session.SendWhisper("Successfully kicked all users from the room.");
        }
    }
}
