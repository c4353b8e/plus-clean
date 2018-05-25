namespace Plus.Game.Rooms.Chat.Commands.User
{
    using Players;

    internal class StandCommand :IChatCommand
    {
        public string PermissionRequired => "command_stand";

        public string Parameters => "";

        public string Description => "Allows you to stand up if not stood already.";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);
            if (User == null)
            {
                return;
            }

            if (User.isSitting)
            {
                User.Statusses.Remove("sit");
                User.Z += 0.35;
                User.isSitting = false;
                User.UpdateNeeded = true;
            }
            else if (User.isLying)
            {
                User.Statusses.Remove("lay");
                User.Z += 0.35;
                User.isLying = false;
                User.UpdateNeeded = true;
            }
        }
    }
}
