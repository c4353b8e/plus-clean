namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    using System.Linq;
    using PathFinding;

    internal class AllEyesOnMeCommand : IChatCommand
    {
        public string PermissionRequired => "command_alleyesonme";

        public string Parameters => "";

        public string Description => "Want some attention? Make everyone face you!";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            var ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
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

                U.SetRot(Rotation.Calculate(U.X, U.Y, ThisUser.X, ThisUser.Y), false);
            }
        }
    }
}
