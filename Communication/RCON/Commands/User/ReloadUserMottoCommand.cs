namespace Plus.Communication.Rcon.Commands.User
{
    using Packets.Outgoing.Rooms.Engine;

    internal class ReloadUserMottoCommand : IRconCommand
    {
        public string Description => "This command is used to reload the users motto from the database.";

        public string Parameters => "%userId%";

        public bool TryExecute(string[] parameters)
        {
            if (!int.TryParse(parameters[0], out var userId))
            {
                return false;
            }

            var client = Program.GameContext.GetClientManager().GetClientByUserId(userId);
            if (client == null || client.GetHabbo() == null)
            {
                return false;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `motto` FROM `users` WHERE `id` = @userID LIMIT 1");
                dbClient.AddParameter("userID", userId);
                client.GetHabbo().Motto = dbClient.GetString();
            }

            // If we're in a room, we cannot really send the packets, so flag this as completed successfully, since we already updated it.
            if (!client.GetHabbo().InRoom)
            {
                return true;
            }

            //We are in a room, let's try to run the packets.
            var room = client.GetHabbo().CurrentRoom;
            if (room != null)
            {
                var user = room.GetRoomUserManager().GetRoomUserByHabbo(client.GetHabbo().Id);
                if (user != null)
                {
                    room.SendPacket(new UserChangeComposer(user, false));
                    return true;
                }
            }

            return false;
        }
    }
}
