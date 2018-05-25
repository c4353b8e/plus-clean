namespace Plus.Game.Rooms.Chat.Commands.Moderator
{
    using System.Linq;
    using Players;

    internal class RoomBadgeCommand : IChatCommand
    {
        public string PermissionRequired => "command_room_badge";

        public string Parameters => "%badge%";

        public string Description => "Give a badge to the entire room!";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter the name of the badge you'd like to give to the room.");
                return;
            }

            foreach (var User in Room.GetRoomUserManager().GetUserList().ToList())
            {
                if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                {
                    continue;
                }

                if (!User.GetClient().GetHabbo().GetBadgeComponent().HasBadge(Params[1]))
                {
                    User.GetClient().GetHabbo().GetBadgeComponent().GiveBadge(Params[1], true, User.GetClient());
                    User.GetClient().SendNotification("You have just been given a badge!");
                }
                else
                {
                    User.GetClient().SendWhisper(Session.GetHabbo().Username + " tried to give you a badge, but you already have it!");
                }
            }

            Session.SendWhisper("You have successfully given every user in this room the " + Params[2] + " badge!");
        }
    }
}
