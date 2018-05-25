namespace Plus.Game.Rooms.Chat.Commands.Moderator
{
    using System.Linq;
    using Players;

    internal class MassBadgeCommand : IChatCommand
    {
        public string PermissionRequired => "command_mass_badge";

        public string Parameters => "%badge%";

        public string Description => "Give a badge to the entire hotel.";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter the code of the badge you'd like to give to the entire hotel.");
                return;
            }

            foreach (var Client in Program.GameContext.PlayerController.GetClients.ToList())
            {
                if (Client == null || Client.GetHabbo() == null || Client.GetHabbo().Username == Session.GetHabbo().Username)
                {
                    continue;
                }

                if (!Client.GetHabbo().GetBadgeComponent().HasBadge(Params[1]))
                {
                    Client.GetHabbo().GetBadgeComponent().GiveBadge(Params[1], true, Client);
                    Client.SendNotification("You have just been given a badge!");
                }
                else
                {
                    Client.SendWhisper(Session.GetHabbo().Username + " tried to give you a badge, but you already have it!");
                }
            }

            Session.SendWhisper("You have successfully given every user in this hotel the " + Params[1] + " badge!");
        }
    }
}
