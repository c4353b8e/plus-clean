namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class MuteBotsCommand : IChatCommand
    {
        public string PermissionRequired => "command_mute_bots";

        public string Parameters => "";

        public string Description => "Ignore bot chat or enable it again.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            Session.GetHabbo().AllowBotSpeech = !Session.GetHabbo().AllowBotSpeech;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `users` SET `bots_muted` = '" + (Session.GetHabbo().AllowBotSpeech ? 1 : 0) + "' WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
            }

            if (Session.GetHabbo().AllowBotSpeech)
            {
                Session.SendWhisper("Change successful, you can no longer see speech from bots.");
            }
            else
            {
                Session.SendWhisper("Change successful, you can now see speech from bots.");
            }
        }
    }
}
