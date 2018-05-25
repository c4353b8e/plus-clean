namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class TeleportCommand : IChatCommand
    {
        public string PermissionRequired => "command_teleport";

        public string Parameters => "";

        public string Description => "The ability to teleport anywhere within the room.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            User.TeleportEnabled = !User.TeleportEnabled;
            Room.GetGameMap().GenerateMaps();
        }
    }
}
