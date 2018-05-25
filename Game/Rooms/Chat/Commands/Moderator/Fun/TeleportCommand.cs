namespace Plus.Game.Rooms.Chat.Commands.Moderator.Fun
{
    using Players;

    internal class TeleportCommand : IChatCommand
    {
        public string PermissionRequired => "command_teleport";

        public string Parameters => "";

        public string Description => "The ability to teleport anywhere within the room.";

        public void Execute(Player session, Room room, string[] Params)
        {
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            user.TeleportEnabled = !user.TeleportEnabled;
            room.GetGameMap().GenerateMaps();
        }
    }
}
