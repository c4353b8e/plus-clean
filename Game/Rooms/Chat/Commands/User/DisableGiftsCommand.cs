namespace Plus.Game.Rooms.Chat.Commands.User
{
    using Players;

    internal class DisableGiftsCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_gifts";

        public string Parameters => "";

        public string Description => "Allows you to disable the ability to receive gifts or to enable the ability to receive gifts.               ";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            Session.GetHabbo().AllowGifts = !Session.GetHabbo().AllowGifts;
            Session.SendWhisper("You're " + (Session.GetHabbo().AllowGifts ? "now" : "no longer") + " accepting gifts.");

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `allow_gifts` = @AllowGifts WHERE `id` = '" + Session.GetHabbo().Id + "'");
                dbClient.AddParameter("AllowGifts", Session.GetHabbo().AllowGifts ? "1" : "0");
                dbClient.RunQuery();
            }
        }
    }
}