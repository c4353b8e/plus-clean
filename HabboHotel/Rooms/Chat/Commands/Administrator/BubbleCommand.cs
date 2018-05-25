namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator
{
    using Styles;

    internal class BubbleCommand : IChatCommand
    {
        public string PermissionRequired => "command_bubble";

        public string Parameters => "%id%";

        public string Description => "Use a custom bubble to chat with.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            if (Params.Length == 1)
            {
                Session.SendWhisper("Oops, you forgot to enter a bubble ID!");
                return;
            }

            var Bubble = 0;
            if (!int.TryParse(Params[1], out Bubble))
            {
                Session.SendWhisper("Please enter a valid number.");
                return;
            }

            ChatStyle Style = null;
            if (!Program.GameContext.GetChatManager().GetChatStyles().TryGetStyle(Bubble, out Style) || Style.RequiredRight.Length > 0 && !Session.GetHabbo().GetPermissions().HasRight(Style.RequiredRight))
            {
                Session.SendWhisper("Oops, you cannot use this bubble due to a rank requirement, sorry!");
                return;
            }

            User.LastBubble = Bubble;
            Session.GetHabbo().CustomBubbleId = Bubble;
            Session.SendWhisper("Bubble set to: " + Bubble);
        }
    }
}