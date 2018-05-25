namespace Plus.Game.Rooms.Chat.Commands.Administrator
{
    using System;
    using Players;

    internal class CarryCommand : IChatCommand
    {
        public string PermissionRequired => "command_carry";

        public string Parameters => "%ItemId%";

        public string Description => "Allows you to carry a hand item";

        public void Execute(Player session, Room room, string[] Params)
        {
            if (!int.TryParse(Convert.ToString(Params[1]), out var itemId))
            {
                session.SendWhisper("Please enter a valid integer.");
                return;
            }

            room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).CarryItem(itemId);
        }
    }
}
