namespace Plus.Game.Rooms.Chat.Commands.Moderator
{
    using Players;

    internal class RoomMuteCommand : IChatCommand
    {
        public string PermissionRequired => "command_roommute";

        public string Parameters => "%message%";

        public string Description => "Mute the room with a reason.";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please provide a reason for muting the room to show to the users.");
                return;
            }

            if (!Room.RoomMuted)
            {
                Room.RoomMuted = true;
            }

            var Msg = CommandManager.MergeParams(Params, 1);

            var RoomUsers = Room.GetRoomUserManager().GetRoomUsers();
            if (RoomUsers.Count > 0)
            {
                foreach (var User in RoomUsers)
                {
                    if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().Username == Session.GetHabbo().Username)
                    {
                        continue;
                    }

                    User.GetClient().SendWhisper("This room has been muted because: " + Msg);
                }
            }
        }
    }
}
