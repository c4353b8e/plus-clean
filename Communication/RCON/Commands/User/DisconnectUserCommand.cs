﻿namespace Plus.Communication.Rcon.Commands.User
{
    internal class DisconnectUserCommand : IRconCommand
    {
        public string Description => "This command is used to Disconnect a user.";

        public string Parameters => "%userId%";

        public bool TryExecute(string[] parameters)
        {
            if (!int.TryParse(parameters[0], out var userId))
            {
                return false;
            }

            var client = Program.GameContext.PlayerController.GetClientByUserId(userId);
            if (client == null || client.GetHabbo() == null)
            {
                return false;
            }

            client.Disconnect();
            return true;
        }
    }
}
