namespace Plus.Game.Rooms.Chat.Commands.Moderator.Fun
{
    using System.Linq;
    using Players;

    internal class AllAroundMeCommand : IChatCommand
    {
        public string PermissionRequired => "command_allaroundme";

        public string Parameters => "";

        public string Description => "Need some attention? Pull all of the users to you.";

        public void Execute(Player session, Room room, string[] Params)
        {
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
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

                u.MoveTo(user.X, user.Y, true);
            }
        }
    }
}
