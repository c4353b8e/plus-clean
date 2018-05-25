namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    using Communication.Packets.Outgoing.Rooms.Chat;
    using GameClients;

    internal class SuperPullCommand : IChatCommand
    {
        public string PermissionRequired => "command_super_pull";

        public string Parameters => "%username%";

        public string Description => "Pull another user to you, with no limits!";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter the username of the user you wish to super pull.");
                return;
            }

            if (!Room.SuperPullEnabled && !Room.CheckRights(Session, true) && !Session.GetHabbo().GetPermissions().HasRight("room_override_custom_config"))
            {
                Session.SendWhisper("Oops, it appears that the room owner has disabled the ability to use the spull command in here.");
                return;
            }

            var TargetClient = Program.GameContext.GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            var TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (TargetUser == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user, maybe they're not online or in this room.");
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("Come on, surely you don't want to push yourself!");
                return;
            }

            if (TargetUser.TeleportEnabled)
            {
                Session.SendWhisper("Oops, you cannot push a user whilst they have their teleport mode enabled.");
                return;
            }

            var ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
            {
                return;
            }

            if (ThisUser.SetX - 1 == Room.GetGameMap().Model.DoorX)
            {
                Session.SendWhisper("Please don't pull that user out of the room :(!");
                return;
            }

            if (ThisUser.RotBody % 2 != 0)
            {
                ThisUser.RotBody--;
            }

            if (ThisUser.RotBody == 0)
            {
                TargetUser.MoveTo(ThisUser.X, ThisUser.Y - 1);
            }
            else if (ThisUser.RotBody == 2)
            {
                TargetUser.MoveTo(ThisUser.X + 1, ThisUser.Y);
            }
            else if (ThisUser.RotBody == 4)
            {
                TargetUser.MoveTo(ThisUser.X, ThisUser.Y + 1);
            }
            else if (ThisUser.RotBody == 6)
            {
                TargetUser.MoveTo(ThisUser.X - 1, ThisUser.Y);
            }

            Room.SendPacket(new ChatComposer(ThisUser.VirtualId, "*super pulls " + Params[1] + " to them*", 0, ThisUser.LastBubble));
        }
    }
}