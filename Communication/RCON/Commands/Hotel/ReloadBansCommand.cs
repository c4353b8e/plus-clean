namespace Plus.Communication.Rcon.Commands.Hotel
{
    internal class ReloadBansCommand : IRconCommand
    {
        public string Description => "This command is used to re-cache the bans.";

        public string Parameters => "";

        public bool TryExecute(string[] parameters)
        {
            Program.GameContext.GetModerationManager().ReCacheBans();

            return true;
        }
    }
}