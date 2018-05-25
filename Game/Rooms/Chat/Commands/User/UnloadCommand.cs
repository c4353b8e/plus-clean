namespace Plus.Game.Rooms.Chat.Commands.User
{
    using Players;

    internal class UnloadCommand : IChatCommand
    {
        public string PermissionRequired => "command_unload";

        public string Parameters => "%id%";

        public string Description => "Unload the current room.";

        public void Execute(Player session, Room room, string[] Params)
        {
            if (room.CheckRights(session, true) || session.GetHabbo().GetPermissions().HasRight("room_unload_any"))
            {
                Program.GameContext.GetRoomManager().UnloadRoom(room.Id);
            }
        }
    }
}
