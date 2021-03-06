﻿namespace Plus.Game.Rooms.Chat.Commands.User
{
    using Players;

    internal class DNDCommand : IChatCommand
    {
        public string PermissionRequired => "command_dnd";

        public string Parameters => "";

        public string Description => "Allows you to chose the option to enable or disable console messages.";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            Session.GetHabbo().AllowConsoleMessages = !Session.GetHabbo().AllowConsoleMessages;
            Session.SendWhisper("You're " + (Session.GetHabbo().AllowConsoleMessages ? "now" : "no longer") + " accepting console messages.");
        }
    }
}