namespace Plus.Game.Rooms.Chat.Commands.Moderator.Fun
{
    using System.Linq;
    using Players;

    internal class MassEnableCommand : IChatCommand
    {
        public string PermissionRequired => "command_massenable";

        public string Parameters => "%EffectId%";

        public string Description => "Give every user in the room a specific enable ID.";

        public void Execute(Player session, Room room, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Please enter an effect ID.");
                return;
            }

            var enableId = 0;
            if (int.TryParse(Params[1], out enableId))
            {
                if (enableId == 102 || enableId == 178)
                {
                    session.Disconnect();
                    return;
                }

                if (!session.GetHabbo().GetPermissions().HasCommand("command_override_massenable") && room.OwnerId != session.GetHabbo().Id)
                {
                    session.SendWhisper("You can only use this command in your own room.");
                    return;
                }

                var users = room.GetRoomUserManager().GetRoomUsers();
                if (users.Count > 0)
                {
                    foreach (var u in users.ToList())
                    {
                        if (u == null || u.RidingHorse)
                        {
                            continue;
                        }

                        u.ApplyEffect(enableId);
                    }
                }
            }
            else
            {
                session.SendWhisper("Please enter an effect ID.");
            }
        }
    }
}
