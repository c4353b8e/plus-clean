namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    using Communication.Packets.Outgoing.Rooms.Session;
    using GameClients;

    internal class SummonCommand : IChatCommand
    {
        public string PermissionRequired => "command_summon";

        public string Parameters => "%username%";

        public string Description => "Bring another user to your current room.";

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

            TargetClient.SendNotification("You have been summoned to " + Session.GetHabbo().Username + "!");
            if (!TargetClient.GetHabbo().InRoom)
            {
                TargetClient.SendPacket(new RoomForwardComposer(Session.GetHabbo().CurrentRoomId));
            }
            else
            {
                TargetClient.GetHabbo().PrepareRoom(Session.GetHabbo().CurrentRoomId, "");
            }
        }
    }
}