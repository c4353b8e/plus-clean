﻿namespace Plus.Game.Rooms.Chat.Commands.Moderator.Fun
{
    using Players;

    internal class FastwalkCommand : IChatCommand
    {
        public string PermissionRequired => "command_fastwalk";

        public string Parameters => "";

        public string Description => "Gives you the ability to walk very fast.";

        public void Execute(Player session, Room room, string[] Params)
        {
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            user.FastWalking = !user.FastWalking;

            if (user.SuperFastWalking)
            {
                user.SuperFastWalking = false;
            }

            session.SendWhisper("Walking mode updated.");
        }
    }
}
