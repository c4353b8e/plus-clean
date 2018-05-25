namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class SuperFastwalkCommand : IChatCommand
    {
        public string PermissionRequired => "command_super_fastwalk";

        public string Parameters => "";

        public string Description => "Gives you the ability to walk very very fast.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            User.SuperFastWalking = !User.SuperFastWalking;

            if (User.FastWalking)
            {
                User.FastWalking = false;
            }

            Session.SendWhisper("Walking mode updated.");
        }
    }
}
