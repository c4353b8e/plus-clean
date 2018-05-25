namespace Plus.Communication.Rcon.Commands.User
{
    internal class ReloadUserVIPRankCommand : IRconCommand
    {
        public string Description => "This command is used to reload a users VIP rank and permissions.";

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
                dbClient.SetQuery("SELECT `rank_vip` FROM `users` WHERE `id` = @userId LIMIT 1");
                dbClient.AddParameter("userId", userId);
                client.GetHabbo().VIPRank = dbClient.GetInteger();
            }

            client.GetHabbo().GetPermissions().Init(client.GetHabbo());
            return true;
        }
    }
}