namespace Plus.Game.Rooms.Chat.Commands.Moderator.Fun
{
    using Players;

    internal class OverrideCommand : IChatCommand
    {
        public string PermissionRequired => "command_override";

        public string Parameters => "";

        public string Description => "Gives you the ability to walk over anything.";

        public void Execute(Player session, Room room, string[] Params)
        {
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            user.AllowOverride = !user.AllowOverride;
            session.SendWhisper("Override mode updated.");
        }
    }
}
