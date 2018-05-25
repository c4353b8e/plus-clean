namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class CoordsCommand : IChatCommand
    {
        public string PermissionRequired => "command_coords";

        public string Parameters => "";

        public string Description => "Used to get your current position within the room.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            var ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
            {
                return;
            }

            Session.SendNotification("X: " + ThisUser.X + "\n - Y: " + ThisUser.Y + "\n - Z: " + ThisUser.Z + "\n - Rot: " + ThisUser.RotBody + ", sqState: " + Room.GetGameMap().GameMap[ThisUser.X, ThisUser.Y] + "\n\n - RoomID: " + Session.GetHabbo().CurrentRoomId);                           
        }
    }
}
