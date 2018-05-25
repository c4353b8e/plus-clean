namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class DisableDiagonalCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_diagonal";

        public string Parameters => "";

        public string Description => "Want to disable diagonal walking in your room? Type this command!";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            if (!Room.CheckRights(Session, true))
            {
                Session.SendWhisper("Oops, only the owner of this room can run this command!");
                return;
            }

            Room.GetGameMap().DiagonalEnabled = !Room.GetGameMap().DiagonalEnabled;
            Session.SendWhisper("Successfully updated the diagonal boolean value for this room.");
        }
    }
}
