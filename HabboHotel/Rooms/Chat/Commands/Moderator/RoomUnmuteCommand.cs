namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class RoomUnmuteCommand : IChatCommand
    {
        public string PermissionRequired => "command_unroommute";

        public string Parameters => "";

        public string Description => "Unmute the room.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            if (!Room.RoomMuted)
            {
                Session.SendWhisper("This room isn't muted.");
                return;
            }

            Room.RoomMuted = false;

            var RoomUsers = Room.GetRoomUserManager().GetRoomUsers();
            if (RoomUsers.Count > 0)
            {
                foreach (var User in RoomUsers)
                {
                    if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().Username == Session.GetHabbo().Username)
                    {
                        continue;
                    }

                    User.GetClient().SendWhisper("This room has been un-muted .");
                }
            }
        }
    }
}