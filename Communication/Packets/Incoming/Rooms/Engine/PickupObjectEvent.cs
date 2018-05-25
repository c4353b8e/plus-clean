﻿namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using Game.Items;
    using Game.Players;
    using Game.Quests;

    internal class PickupObjectEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {

            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            packet.PopInt(); //unknown
            var itemId = packet.PopInt();

            var item = room.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
            {
                return;
            }

            if (item.GetBaseItem().InteractionType == InteractionType.POSTIT)
            {
                return;
            }

            var itemRights = false;
            if (item.UserID == session.GetHabbo().Id || room.CheckRights(session, false))
            {
                itemRights = true;
            }
            else if (room.Group != null && room.CheckRights(session, false, true))//Room has a group, this user has group rights.
            {
                itemRights = true;
            }
            else if (session.GetHabbo().GetPermissions().HasRight("room_item_take"))
            {
                itemRights = true;
            }

            if (itemRights)
            {
                if (item.GetBaseItem().InteractionType == InteractionType.TENT || item.GetBaseItem().InteractionType == InteractionType.TENT_SMALL)
                {
                    room.RemoveTent(item.Id);
                }

                if (item.GetBaseItem().InteractionType == InteractionType.MOODLIGHT)
                {
                    using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                    {
                        dbClient.RunQuery("DELETE FROM `room_items_moodlight` WHERE `item_id` = '" + item.Id + "' LIMIT 1");
                    }
                }
                else if (item.GetBaseItem().InteractionType == InteractionType.TONER)
                {
                    using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                    {
                        dbClient.RunQuery("DELETE FROM `room_items_toner` WHERE `id` = '" + item.Id + "' LIMIT 1");
                    }
                }


                if (item.UserID == session.GetHabbo().Id)
                {
                    room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
                    session.GetHabbo().GetInventoryComponent().AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo, item.LimitedTot);
                    session.GetHabbo().GetInventoryComponent().UpdateItems(false);
                }
                else if (session.GetHabbo().GetPermissions().HasRight("room_item_take"))//Staff are taking this item
                {
                    room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
                    session.GetHabbo().GetInventoryComponent().AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo, item.LimitedTot);
                    session.GetHabbo().GetInventoryComponent().UpdateItems(false);

                }
                else//Item is being ejected.
                {
                    var targetClient = Program.GameContext.PlayerController.GetClientByUserId(item.UserID);
                    if (targetClient != null && targetClient.GetHabbo() != null)//Again, do we have an active client?
                    {
                        room.GetRoomItemHandler().RemoveFurniture(targetClient, item.Id);
                        targetClient.GetHabbo().GetInventoryComponent().AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo, item.LimitedTot);
                        targetClient.GetHabbo().GetInventoryComponent().UpdateItems(false);
                    }
                    else//No, query time.
                    {
                        room.GetRoomItemHandler().RemoveFurniture(null, item.Id);
                        using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = '" + item.Id + "' LIMIT 1");
                        }
                    }
                }

                Program.GameContext.GetQuestManager().ProgressUserQuest(session, QuestType.FurniPick);
            }
        }
    }
}