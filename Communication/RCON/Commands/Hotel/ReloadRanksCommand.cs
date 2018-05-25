namespace Plus.Communication.Rcon.Commands.Hotel
{
    using System.Linq;

    internal class ReloadRanksCommand : IRconCommand
    {
        public string Description => "This command is used to reload user permissions.";

        public string Parameters => "";

        public bool TryExecute(string[] parameters)
        {
            Program.GameContext.GetPermissionManager().Init();

            foreach (var client in Program.GameContext.PlayerController.GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null || client.GetHabbo().GetPermissions() == null)
                {
                    continue;
                }

                client.GetHabbo().GetPermissions().Init(client.GetHabbo());
            }
            
            return true;
        }
    }
}