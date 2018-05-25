namespace Plus.Game.Rooms.Chat.Commands.User.Fun
{
    using Communication.Packets.Outgoing.Rooms.Avatar;
    using Players;

    internal class DanceCommand :IChatCommand
    {
        public string PermissionRequired => "command_dance";

        public string Parameters => "%DanceId%";

        public string Description => "Too lazy to dance the proper way? Do it like this!";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            var ThisUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
            {
                return;
            }

            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter an ID of a dance.");
                return;
            }

            int DanceId;
            if (int.TryParse(Params[1], out DanceId))
            {
                if (DanceId > 4 || DanceId < 0)
                {
                    Session.SendWhisper("The dance ID must be between 0 and 4!");
                    return;
                }

                Session.GetHabbo().CurrentRoom.SendPacket(new DanceComposer(ThisUser, DanceId));
            }
            else
            {
                Session.SendWhisper("Please enter a valid dance ID.");
            }
        }
    }
}
