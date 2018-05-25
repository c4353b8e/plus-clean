namespace Plus.Game.Rooms.Chat.Commands.User
{
    using System.Linq;
    using Players;

    internal class EjectAllCommand :IChatCommand
    {
        public string PermissionRequired => "command_ejectall";

        public string Parameters => "";

        public string Description => "Removes all of the items from the room.";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            if (Session.GetHabbo().Id == Room.OwnerId)
            {
                //Let us check anyway.
                if (!Room.CheckRights(Session, true))
                {
                    return;
                }

                foreach (var Item in Room.GetRoomItemHandler().GetWallAndFloor.ToList())
                {
                    if (Item == null || Item.UserID == Session.GetHabbo().Id)
                    {
                        continue;
                    }

                    var TargetClient = Program.GameContext.PlayerController.GetClientByUserId(Item.UserID);
                    if (TargetClient != null && TargetClient.GetHabbo() != null)
                    {
                        Room.GetRoomItemHandler().RemoveFurniture(TargetClient, Item.Id);
                        TargetClient.GetHabbo().GetInventoryComponent().AddNewItem(Item.Id, Item.BaseItem, Item.ExtraData, Item.GroupId, true, true, Item.LimitedNo, Item.LimitedTot);
                        TargetClient.GetHabbo().GetInventoryComponent().UpdateItems(false);
                    }
                    else
                    {
                        Room.GetRoomItemHandler().RemoveFurniture(null, Item.Id);
                        using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = '" + Item.Id + "' LIMIT 1");
                        }
                    }
                }
            }
            else
            {
                foreach (var Item in Room.GetRoomItemHandler().GetWallAndFloor.ToList())
                {
                    if (Item == null || Item.UserID != Session.GetHabbo().Id)
                    {
                        continue;
                    }

                    var TargetClient = Program.GameContext.PlayerController.GetClientByUserId(Item.UserID);
                    if (TargetClient != null && TargetClient.GetHabbo() != null)
                    {
                        Room.GetRoomItemHandler().RemoveFurniture(TargetClient, Item.Id);
                        TargetClient.GetHabbo().GetInventoryComponent().AddNewItem(Item.Id, Item.BaseItem, Item.ExtraData, Item.GroupId, true, true, Item.LimitedNo, Item.LimitedTot);
                        TargetClient.GetHabbo().GetInventoryComponent().UpdateItems(false);
                    }
                    else
                    {
                        Room.GetRoomItemHandler().RemoveFurniture(null, Item.Id);
                        using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = '" + Item.Id + "' LIMIT 1");
                        }
                    }
                }
            }
        }
    }
}
