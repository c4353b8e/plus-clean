namespace Plus.Game.Rooms.Chat.Commands.User
{
    using System;
    using System.Data;
    using Communication.Packets.Outgoing.Inventory.Purse;
    using Items;
    using Players;

    internal class ConvertCreditsCommand : IChatCommand
    {
        public string PermissionRequired => "command_convert_credits";

        public string Parameters => "";

        public string Description => "Convert your exchangeable furniture into actual credits.";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            var TotalValue = 0;

            try 
            {
                DataTable Table = null;           
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id` FROM `items` WHERE `user_id` = '" + Session.GetHabbo().Id + "' AND (`room_id`=  '0' OR `room_id` = '')");
                    Table = dbClient.GetTable();
                }

                if (Table == null)
                {
                    Session.SendWhisper("You currently have no items in your inventory!");
                    return;
                }
                if (Table.Rows.Count > 0)
                {
                    using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                    {
                        foreach (DataRow Row in Table.Rows)
                        {
                            var Item = Session.GetHabbo().GetInventoryComponent().GetItem(Convert.ToInt32(Row[0]));
                            if (Item == null || Item.RoomId > 0 || Item.Data.InteractionType != InteractionType.EXCHANGE)
                            {
                                continue;
                            }

                            var Value = Item.Data.BehaviourData;

                            dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + Item.Id + "' LIMIT 1");

                            Session.GetHabbo().GetInventoryComponent().RemoveItem(Item.Id);

                            TotalValue += Value;

                            if (Value > 0)
                            {
                                Session.GetHabbo().Credits += Value;
                                Session.SendPacket(new CreditBalanceComposer(Session.GetHabbo().Credits));
                            }
                        }
                    }
                }

                if (TotalValue > 0)
                {
                    Session.SendNotification("All credits have successfully been converted!\r\r(Total value: " + TotalValue + " credits!");
                }
                else
                {
                    Session.SendNotification("It appears you don't have any exchangeable items!");
                }
            }
            catch
            {
                Session.SendNotification("Oops, an error occoured whilst converting your credits!");
            }
        }
    }
}
