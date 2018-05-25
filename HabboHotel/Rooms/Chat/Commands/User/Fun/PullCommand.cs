﻿namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    using System;
    using Communication.Packets.Outgoing.Rooms.Chat;
    using GameClients;

    internal class PullCommand : IChatCommand
    {
        public string PermissionRequired => "command_pull";

        public string Parameters => "%target%";

        public string Description => "Pull another user towards you.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter the username of the user you wish to pull.");
                return;
            }

            if (!Room.PullEnabled && !Session.GetHabbo().GetPermissions().HasRight("room_override_custom_config"))
            {
                Session.SendWhisper("Oops, it appears that the room owner has disabled the ability to use the pull command in here.");
                return;
            }

            var TargetClient = Program.GameContext.GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            var TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (TargetUser == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user, maybe they're not online or in this room.");
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("Come on, surely you don't want to push yourself!");
                return;
            }

            if (TargetUser.TeleportEnabled)
            {
                Session.SendWhisper("Oops, you cannot push a user whilst they have their teleport mode enabled.");
                return;
            }

            var ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
            {
                return;
            }

            if (ThisUser.SetX - 1 == Room.GetGameMap().Model.DoorX)
            {
                Session.SendWhisper("Please don't pull that user out of the room :(!");
                return;
            }


            var PushDirection = "down";
            if (TargetClient.GetHabbo().CurrentRoomId == Session.GetHabbo().CurrentRoomId && Math.Abs(ThisUser.X - TargetUser.X) < 3 && Math.Abs(ThisUser.Y - TargetUser.Y) < 3)
            {
                Room.SendPacket(new ChatComposer(ThisUser.VirtualId, "*pulls " + Params[1] + " to them*", 0, ThisUser.LastBubble));

                if (ThisUser.RotBody == 0)
                {
                    PushDirection = "up";
                }

                if (ThisUser.RotBody == 2)
                {
                    PushDirection = "right";
                }

                if (ThisUser.RotBody == 4)
                {
                    PushDirection = "down";
                }

                if (ThisUser.RotBody == 6)
                {
                    PushDirection = "left";
                }

                if (PushDirection == "up")
                {
                    TargetUser.MoveTo(ThisUser.X, ThisUser.Y - 1);
                }

                if (PushDirection == "right")
                {
                    TargetUser.MoveTo(ThisUser.X + 1, ThisUser.Y);
                }

                if (PushDirection == "down")
                {
                    TargetUser.MoveTo(ThisUser.X, ThisUser.Y + 1);
                }

                if (PushDirection == "left")
                {
                    TargetUser.MoveTo(ThisUser.X - 1, ThisUser.Y);
                }
            }
            else
            {
                Session.SendWhisper("That user is not close enough to you to be pulled, try getting closer!");
            }
        }
    }
}