namespace Plus.Game.Rooms.Chat.Commands.Moderator.Fun
{
    using Players;

    internal class ForceSitCommand : IChatCommand
    {
        public string PermissionRequired => "command_forcesit";

        public string Parameters => "%username%";

        public string Description => "Force another to user sit.";

        public void Execute(Player session, Room room, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Oops, you forgot to choose a target user!");
                return;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
            if (user == null)
            {
                return;
            }

            if (user.Statusses.ContainsKey("lie") || user.isLying || user.RidingHorse || user.IsWalking)
            {
                return;
            }

            if (!user.Statusses.ContainsKey("sit"))
            {
                if (user.RotBody % 2 == 0)
                {
                    if (user == null)
                    {
                        return;
                    }

                    try
                    {
                        user.Statusses.Add("sit", "1.0");
                        user.Z -= 0.35;
                        user.isSitting = true;
                        user.UpdateNeeded = true;
                    }
                    catch { }
                }
                else
                {
                    user.RotBody--;
                    user.Statusses.Add("sit", "1.0");
                    user.Z -= 0.35;
                    user.isSitting = true;
                    user.UpdateNeeded = true;
                }
            }
            else if (user.isSitting)
            {
                user.Z += 0.35;
                user.Statusses.Remove("sit");
                user.Statusses.Remove("1.0");
                user.isSitting = false;
                user.UpdateNeeded = true;
            }
        }
    }
}
