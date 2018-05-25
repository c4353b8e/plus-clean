namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class DisableMimicCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_mimic";

        public string Parameters => "";

        public string Description => "Allows you to disable the ability to be mimiced or to enable the ability to be mimiced.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            Session.GetHabbo().AllowMimic = !Session.GetHabbo().AllowMimic;
            Session.SendWhisper("You're " + (Session.GetHabbo().AllowMimic ? "now" : "no longer") + " able to be mimiced.");

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `allow_mimic` = @AllowMimic WHERE `id` = '" + Session.GetHabbo().Id + "'");
                dbClient.AddParameter("AllowMimic", Session.GetHabbo().AllowMimic ? "1" : "0");
                dbClient.RunQuery();
            }
        }
    }
}