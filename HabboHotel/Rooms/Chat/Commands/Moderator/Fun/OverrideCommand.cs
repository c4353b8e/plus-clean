namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class OverrideCommand : IChatCommand
    {
        public string PermissionRequired => "command_override";

        public string Parameters => "";

        public string Description => "Gives you the ability to walk over anything.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            User.AllowOverride = !User.AllowOverride;
            Session.SendWhisper("Override mode updated.");
        }
    }
}
