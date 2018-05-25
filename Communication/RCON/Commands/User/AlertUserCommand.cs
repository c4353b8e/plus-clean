namespace Plus.Communication.Rcon.Commands.User
{
    using System;
    using Packets.Outgoing.Moderation;

    internal class AlertUserCommand : IRconCommand
    {
        public string Description => "This command is used to alert a user.";

        public string Parameters => "%userId% %message%";

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

            // Validate the message
            if (string.IsNullOrEmpty(Convert.ToString(parameters[1])))
            {
                return false;
            }

            var message = Convert.ToString(parameters[1]);

            client.SendPacket(new BroadcastMessageAlertComposer(message));
            return true;
        }
    }
}