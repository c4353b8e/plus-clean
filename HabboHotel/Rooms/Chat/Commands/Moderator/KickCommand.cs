namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using GameClients;

    internal class KickCommand : IChatCommand
    {
        public string PermissionRequired => "command_kick";

        public string Parameters => "%username% %reason%";

        public string Description => "Kick a user from a room and send them a reason.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter the username of the user you wish to summon.");
                return;
            }

            var TargetClient = Program.GameContext.GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            if (TargetClient.GetHabbo() == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("Get a life.");
                return;
            }

            if (!TargetClient.GetHabbo().InRoom)
            {
                Session.SendWhisper("That user currently isn't in a room.");
                return;
            }

            Room TargetRoom;
            if (!Program.GameContext.GetRoomManager().TryGetRoom(TargetClient.GetHabbo().CurrentRoomId, out TargetRoom))
            {
                return;
            }

            if (Params.Length > 2)
            {
                TargetClient.SendNotification("A moderator has kicked you from the room for the following reason: " + CommandManager.MergeParams(Params, 2));
            }
            else
            {
                TargetClient.SendNotification("A moderator has kicked you from the room.");
            }

            TargetRoom.GetRoomUserManager().RemoveUserFromRoom(TargetClient, true, false);
        }
    }
}
