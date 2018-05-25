namespace Plus.Game.Rooms.Chat.Commands.Moderator.Fun
{
    using System.Linq;
    using PathFinding;
    using Players;

    internal class AllEyesOnMeCommand : IChatCommand
    {
        public string PermissionRequired => "command_alleyesonme";

        public string Parameters => "";

        public string Description => "Want some attention? Make everyone face you!";

        public void Execute(Player session, Room room, string[] Params)
        {
            var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
            {
                return;
            }

            var users = room.GetRoomUserManager().GetRoomUsers();
            foreach (var u in users.ToList())
            {
                if (u == null || session.GetHabbo().Id == u.UserId)
                {
                    continue;
                }

                u.SetRot(Rotation.Calculate(u.X, u.Y, thisUser.X, thisUser.Y), false);
            }
        }
    }
}
