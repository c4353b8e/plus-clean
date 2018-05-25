namespace Plus.Communication.Rcon.Commands.User
{
    using System;
    using Packets.Outgoing.Inventory.Purse;

    internal class ReloadUserCurrencyCommand : IRconCommand
    {
        public string Description => "This command is used to update the users currency from the database.";

        public string Parameters => "%userId% %currency%";

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

            // Validate the currency type
            if (string.IsNullOrEmpty(Convert.ToString(parameters[1])))
            {
                return false;
            }

            var currency = Convert.ToString(parameters[1]);

            switch (currency)
            {
                default:
                    return false;

                case "coins":
                case "credits":
                    {
                        int credits;
                        using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `credits` FROM `users` WHERE `id` = @id LIMIT 1");
                            dbClient.AddParameter("id", userId);
                            credits = dbClient.GetInteger();
                        }

                        client.GetHabbo().Credits = credits;
                        client.SendPacket(new CreditBalanceComposer(client.GetHabbo().Credits));
                        break;
                    }

                case "pixels":
                case "duckets":
                    {
                        int duckets;
                        using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `activity_points` FROM `users` WHERE `id` = @id LIMIT 1");
                            dbClient.AddParameter("id", userId);
                            duckets = dbClient.GetInteger();
                        }

                        client.GetHabbo().Duckets = duckets;
                        client.SendPacket(new HabboActivityPointNotificationComposer(client.GetHabbo().Duckets, duckets));
                        break;
                    }

                case "diamonds":
                    {
                        int diamonds;
                        using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `vip_points` FROM `users` WHERE `id` = @id LIMIT 1");
                            dbClient.AddParameter("id", userId);
                            diamonds = dbClient.GetInteger();
                        }

                        client.GetHabbo().Diamonds = diamonds;
                        client.SendPacket(new HabboActivityPointNotificationComposer(diamonds, 0, 5));
                        break;
                    }

                case "gotw":
                    {
                        int gotw;
                        using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `gotw_points` FROM `users` WHERE `id` = @id LIMIT 1");
                            dbClient.AddParameter("id", userId);
                            gotw = dbClient.GetInteger();
                        }

                        client.GetHabbo().GOTWPoints = gotw;
                        client.SendPacket(new HabboActivityPointNotificationComposer(gotw, 0, 103));
                        break;
                    }
            }
            return true;
        }
    }
}