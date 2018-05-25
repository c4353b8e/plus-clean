namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    using System.Linq;

    internal class AllAroundMeCommand : IChatCommand
    {
        public string PermissionRequired => "command_allaroundme";

        public string Parameters => "";

        public string Description => "Need some attention? Pull all of the users to you.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            var Users = Room.GetRoomUserManager().GetRoomUsers();
            foreach (var U in Users.ToList())
            {
                if (U == null || Session.GetHabbo().Id == U.UserId)
                {
                    continue;
                }

                U.MoveTo(User.X, User.Y, true);
            }
        }
    }
}
