namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using System;
    using System.Data;
    using System.Threading;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Engine;
    using Outgoing.Rooms.Furni;

    internal class OpenGiftEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            var presentId = packet.PopInt();
            var present = room.GetRoomItemHandler().GetItem(presentId);
            if (present == null)
            {
                return;
            }

            if (present.UserID != session.GetHabbo().Id)
            {
                return;
            }

            DataRow data;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `base_id`,`extra_data` FROM `user_presents` WHERE `item_id` = @presentId LIMIT 1");
                dbClient.AddParameter("presentId", present.Id);
                data = dbClient.GetRow();
            }

            if (data == null)
            {
                session.SendNotification("Oops! Appears there was a bug with this gift.\nWe'll just get rid of it for you.");
                room.GetRoomItemHandler().RemoveFurniture(null, present.Id);

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + present.Id + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `user_presents` WHERE `item_id` = '" + present.Id + "' LIMIT 1");
                }

                session.GetHabbo().GetInventoryComponent().RemoveItem(present.Id);
                return;
            }
            
            if (!int.TryParse(present.ExtraData.Split(Convert.ToChar(5))[2], out var purchaserId))
            {
                session.SendNotification("Oops! Appears there was a bug with this gift.\nWe'll just get rid of it for you.");
                room.GetRoomItemHandler().RemoveFurniture(null, present.Id);

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + present.Id + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `user_presents` WHERE `item_id` = '" + present.Id + "' LIMIT 1");
                }
                session.GetHabbo().GetInventoryComponent().RemoveItem(present.Id);

                return;
            }

            var purchaser = Program.GameContext.GetCacheManager().GenerateUser(purchaserId);
            if (purchaser == null)
            {
                session.SendNotification("Oops! Appears there was a bug with this gift.\nWe'll just get rid of it for you.");
                room.GetRoomItemHandler().RemoveFurniture(null, present.Id);

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + present.Id + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `user_presents` WHERE `item_id` = '" + present.Id + "' LIMIT 1");
                }

                session.GetHabbo().GetInventoryComponent().RemoveItem(present.Id);
                return;
            }
            
            if (!Program.GameContext.GetItemManager().GetItem(Convert.ToInt32(data["base_id"]), out var baseItem))
            {
                session.SendNotification("Oops, it appears that the item within the gift is no longer in the hotel!");
                room.GetRoomItemHandler().RemoveFurniture(null, present.Id);

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + present.Id + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `user_presents` WHERE `item_id` = '" + present.Id + "' LIMIT 1");
                }

                session.GetHabbo().GetInventoryComponent().RemoveItem(present.Id);
                return;
            }


            present.MagicRemove = true;
            room.SendPacket(new ObjectUpdateComposer(present, Convert.ToInt32(session.GetHabbo().Id)));

            var thread = new Thread(() => FinishOpenGift(session, baseItem, present, room, data));
            thread.Start();


        }

        private void FinishOpenGift(GameClient session, ItemData baseItem, Item present, Room room, DataRow row)
        {
            try
            {
                if (baseItem == null || present == null || room == null || row == null)
                {
                    return;
                }


                Thread.Sleep(1500);

                var itemIsInRoom = true;

                room.GetRoomItemHandler().RemoveFurniture(session, present.Id);

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE `items` SET `base_item` = @BaseItem, `extra_data` = @edata WHERE `id` = @itemId LIMIT 1");
                    dbClient.AddParameter("itemId", present.Id);
                    dbClient.AddParameter("BaseItem", row["base_id"]);
                    dbClient.AddParameter("edata", row["extra_data"]);
                    dbClient.RunQuery();

                    dbClient.RunQuery("DELETE FROM `user_presents` WHERE `item_id` = " + present.Id + " LIMIT 1");
                }

                present.BaseItem = Convert.ToInt32(row["base_id"]);
                present.ResetBaseItem();
                present.ExtraData = !string.IsNullOrEmpty(Convert.ToString(row["extra_data"])) ? Convert.ToString(row["extra_data"]) : "";

                if (present.Data.Type == 's')
                {
                    if (!room.GetRoomItemHandler().SetFloorItem(session, present, present.GetX, present.GetY, present.Rotation, true, false, true))
                    {
                        using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = @itemId LIMIT 1");
                            dbClient.AddParameter("itemId", present.Id);
                            dbClient.RunQuery();
                        }

                        itemIsInRoom = false;
                    }
                }
                else
                {
                    using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = @itemId LIMIT 1");
                        dbClient.AddParameter("itemId", present.Id);
                        dbClient.RunQuery();
                    }

                    itemIsInRoom = false;
                }

                session.SendPacket(new OpenGiftComposer(present.Data, present.ExtraData, present, itemIsInRoom));

                session.GetHabbo().GetInventoryComponent().UpdateItems(true);
            }
            catch
            {
                //ignored
            }
        }
    }
}