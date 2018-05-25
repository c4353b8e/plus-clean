namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class FastwalkCommand : IChatCommand
    {
        public string PermissionRequired => "command_fastwalk";

        public string Parameters => "";

        public string Description => "Gives you the ability to walk very fast.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            User.FastWalking = !User.FastWalking;

            if (User.SuperFastWalking)
            {
                User.SuperFastWalking = false;
            }

            Session.SendWhisper("Walking mode updated.");
        }
    }
}
