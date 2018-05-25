namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class GOTOCommand : IChatCommand
    {
        public string PermissionRequired => "command_goto";

        public string Parameters => "%room_id%";

        public string Description => "";

        public void Execute(GameClients.GameClient session, Room room, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("You must specify a room id!");
                return;
            }


            if (!int.TryParse(Params[1], out var roomId))
            {
                session.SendWhisper("You must enter a valid room ID");
            }
            else
            {
                RoomData Data = null;
                if (!RoomFactory.TryGetData(roomId, out Data))
                {
                    session.SendWhisper("This room does not exist!");
                }
                else
                {
                    session.GetHabbo().PrepareRoom(roomId, "");
                }
            }
        }
    }
}