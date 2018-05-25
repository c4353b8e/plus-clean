namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class DisableForcedFXCommand : IChatCommand
    {
        public string PermissionRequired => "command_forced_effects";

        public string Parameters => "";

        public string Description => "Gives you the ability to ignore or allow forced effects.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            Session.GetHabbo().DisableForcedEffects = !Session.GetHabbo().DisableForcedEffects;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `disable_forced_effects` = @DisableForcedEffects WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("DisableForcedEffects", (Session.GetHabbo().DisableForcedEffects ? 1 : 0).ToString());
                dbClient.RunQuery();
            }

            Session.SendWhisper("Forced FX mode is now " + (Session.GetHabbo().DisableForcedEffects ? "disabled!" : "enabled!"));
        }
    }
}
