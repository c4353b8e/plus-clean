namespace Plus.Game.Rooms.Chat.Commands.User.Fun
{
    using Players;

    internal class MoonwalkCommand : IChatCommand
    {
        public string PermissionRequired => "command_moonwalk";

        public string Parameters => "";

        public string Description => "Wear the shoes of Michael Jackson.";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            User.moonwalkEnabled = !User.moonwalkEnabled;

            Session.SendWhisper(User.moonwalkEnabled ? "Moonwalk enabled!" : "Moonwalk disabled!");
        }
    }
}
