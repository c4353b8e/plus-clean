namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    using System.Linq;

    internal class MassEnableCommand : IChatCommand
    {
        public string PermissionRequired => "command_massenable";

        public string Parameters => "%EffectId%";

        public string Description => "Give every user in the room a specific enable ID.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter an effect ID.");
                return;
            }

            var EnableId = 0;
            if (int.TryParse(Params[1], out EnableId))
            {
                if (EnableId == 102 || EnableId == 178)
                {
                    Session.Disconnect();
                    return;
                }

                if (!Session.GetHabbo().GetPermissions().HasCommand("command_override_massenable") && Room.OwnerId != Session.GetHabbo().Id)
                {
                    Session.SendWhisper("You can only use this command in your own room.");
                    return;
                }

                var Users = Room.GetRoomUserManager().GetRoomUsers();
                if (Users.Count > 0)
                {
                    foreach (var U in Users.ToList())
                    {
                        if (U == null || U.RidingHorse)
                        {
                            continue;
                        }

                        U.ApplyEffect(EnableId);
                    }
                }
            }
            else
            {
                Session.SendWhisper("Please enter an effect ID.");
            }
        }
    }
}
