namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class RoomAlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_room_alert";

        public string Parameters => "%message%";

        public string Description => "Send a message to the users in this room.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter a message you'd like to send to the room.");
                return;
            }

            if(!Session.GetHabbo().GetPermissions().HasRight("mod_alert") && Room.OwnerId != Session.GetHabbo().Id)
            {
                Session.SendWhisper("You can only Room Alert in your own room!");
                return;
            }

            var Message = CommandManager.MergeParams(Params, 1);
            foreach (var RoomUser in Room.GetRoomUserManager().GetRoomUsers())
            {
                if (RoomUser == null || RoomUser.GetClient() == null || Session.GetHabbo().Id == RoomUser.UserId)
                {
                    continue;
                }

                RoomUser.GetClient().SendNotification(Session.GetHabbo().Username + " alerted the room with the following message:\n\n" + Message);
            }
            Session.SendWhisper("Message successfully sent to the room.");
        }
    }
}
