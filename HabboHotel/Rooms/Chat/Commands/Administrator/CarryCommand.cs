namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator
{
    using System;

    internal class CarryCommand : IChatCommand
    {
        public string PermissionRequired => "command_carry";

        public string Parameters => "%ItemId%";

        public string Description => "Allows you to carry a hand item";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            var ItemId = 0;
            if (!int.TryParse(Convert.ToString(Params[1]), out ItemId))
            {
                Session.SendWhisper("Please enter a valid integer.");
                return;
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            User.CarryItem(ItemId);
        }
    }
}
