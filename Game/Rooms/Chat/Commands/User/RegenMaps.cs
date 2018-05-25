namespace Plus.Game.Rooms.Chat.Commands.User
{
    using Players;

    internal class RegenMaps : IChatCommand
    {
        public string PermissionRequired => "command_regen_maps";

        public string Parameters => "";

        public string Description => "Is the game map of your room broken? Fix it with this command!";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            if (!Room.CheckRights(Session, true))
            {
                Session.SendWhisper("Oops, only the owner of this room can run this command!");
                return;
            }

            Room.GetGameMap().GenerateMaps();
            Session.SendWhisper("Game map of this room successfully re-generated.");
        }
    }
}
