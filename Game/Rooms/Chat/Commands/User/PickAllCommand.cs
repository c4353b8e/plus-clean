namespace Plus.Game.Rooms.Chat.Commands.User
{
    using System.Linq;
    using Communication.Packets.Outgoing.Inventory.Furni;
    using Players;

    internal class PickAllCommand : IChatCommand
    {
        public string PermissionRequired => "command_pickall";

        public string Parameters => "";

        public string Description => "Picks up all of the furniture from your room.";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            if (!Room.CheckRights(Session, true))
            {
                return;
            }

            Room.GetRoomItemHandler().RemoveItems(Session);
            Room.GetGameMap().GenerateMaps();

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `items` SET `room_id` = '0' WHERE `room_id` = @RoomId AND `user_id` = @UserId");
                dbClient.AddParameter("RoomId", Room.Id);
                dbClient.AddParameter("UserId", Session.GetHabbo().Id);
                dbClient.RunQuery();
            }

            var Items = Room.GetRoomItemHandler().GetWallAndFloor.ToList();
            if (Items.Count > 0)
            {
                Session.SendWhisper("There are still more items in this room, manually remove them or use :ejectall to eject them!");
            }

            Session.SendPacket(new FurniListUpdateComposer());
        }
    }
}