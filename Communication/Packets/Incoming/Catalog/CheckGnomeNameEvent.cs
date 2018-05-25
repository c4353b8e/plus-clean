namespace Plus.Communication.Packets.Incoming.Catalog
{
    using System;
    using System.Collections.Generic;
    using HabboHotel.Catalog.Utilities;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Rooms.AI;
    using HabboHotel.Rooms.AI.Speech;
    using Outgoing.Catalog;
    using Outgoing.Inventory.Furni;
    using Utilities;

    internal class CheckGnomeNameEvent : IPacketEvent
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

            var itemId = packet.PopInt();
            var item = room.GetRoomItemHandler().GetItem(itemId);

            if (item == null || item.Data == null || item.UserID != session.GetHabbo().Id || item.Data.InteractionType != InteractionType.GNOME_BOX)
            {
                return;
            }

            var petName = packet.PopString();
            if (string.IsNullOrEmpty(petName))
            {
                session.SendPacket(new CheckGnomeNameComposer(petName, 1));
                return;
            }

            if (!HabboUtilities.IsValidAlphaNumeric(petName))
            {
                session.SendPacket(new CheckGnomeNameComposer(petName, 1));
                return;
            }

            var x = item.GetX;
            var y = item.GetY;

            //Quickly delete it from the database.
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `items` WHERE `id` = @ItemId LIMIT 1");
                dbClient.AddParameter("ItemId", item.Id);
                dbClient.RunQuery();
            }

            //Remove the item.
            room.GetRoomItemHandler().RemoveFurniture(session, item.Id);

            //Apparently we need this for success.
            session.SendPacket(new CheckGnomeNameComposer(petName, 0));

            //Create the pet here.
            var pet = PetUtility.CreatePet(session.GetHabbo().Id, petName, 26, "30", "ffffff");
            if (pet == null)
            {
                session.SendNotification("Oops, an error occoured. Please report this!");
                return;
            }

            var rndSpeechList = new List<RandomSpeech>();

            pet.RoomId = session.GetHabbo().CurrentRoomId;
            pet.GnomeClothing = RandomClothing();

            //Update the pets gnome clothing.
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `bots_petdata` SET `gnome_clothing` = @GnomeClothing WHERE `id` = @PetId LIMIT 1");
                dbClient.AddParameter("GnomeClothing", pet.GnomeClothing);
                dbClient.AddParameter("PetId", pet.PetId);
                dbClient.RunQuery();
            }

            //Make a RoomUser of the pet.
            room.GetRoomUserManager().DeployBot(new RoomBot(pet.PetId, pet.RoomId, "pet", "freeroam", pet.Name, "", pet.Look, x, y, 0, 0, 0, 0, 0, 0, ref rndSpeechList, "", 0, pet.OwnerId, false, 0, false, 0), pet);

            //Give the food.
            if (Program.GameContext.GetItemManager().GetItem(320, out var petFood))
            {
                var food = ItemFactory.CreateSingleItemNullable(petFood, session.GetHabbo(), "", "");
                if (food != null)
                {
                    session.GetHabbo().GetInventoryComponent().TryAddItem(food);
                    session.SendPacket(new FurniListNotificationComposer(food.Id, 1));
                }
            }
        }

        private static string RandomClothing()
        {
            var random = new Random();

            var randomNumber = random.Next(1, 6);
            switch (randomNumber)
            {
                default:
                    return "5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                case 2:
                    return "5 0 -1 0 1 102 13 3 301 4 4 401 5 2 201 3";
                case 3:
                    return "5 1 102 8 2 201 16 4 401 9 3 303 4 0 -1 6";
                case 4:
                    return "5 0 -1 0 3 303 4 4 401 5 1 101 2 2 201 3";
                case 5:
                    return "5 3 302 4 2 201 11 1 102 12 0 -1 28 4 401 24";
                case 6:
                    return "5 4 402 5 3 302 21 0 -1 7 1 101 12 2 201 17";
            }
        }
    }
}