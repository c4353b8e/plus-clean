namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse
{
    using HabboHotel.Catalog.Utilities;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using Outgoing.Catalog;
    using Outgoing.Inventory.Furni;
    using Outgoing.Rooms.AI.Pets;
    using Outgoing.Rooms.Engine;

    internal class RemoveSaddleFromHorseEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            if (!room.GetRoomUserManager().TryGetPet(packet.PopInt(), out var petUser))
            {
                return;
            }

            if (petUser.PetData == null || petUser.PetData.OwnerId != session.GetHabbo().Id)
            {
                return;
            }

            //Fetch the furniture Id for the pets current saddle.
            var saddleId = ItemUtility.GetSaddleId(petUser.PetData.Saddle);

            //Remove the saddle from the pet.
            petUser.PetData.Saddle = 0;

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `bots_petdata` SET `have_saddle` = '0' WHERE `id` = '" + petUser.PetData.PetId + "' LIMIT 1");
            }

            //Give the saddle back to the user.
            if (!Program.GameContext.GetItemManager().GetItem(saddleId, out var itemData))
            {
                return;
            }

            var item = ItemFactory.CreateSingleItemNullable(itemData, session.GetHabbo(), "", "");
            if (item != null)
            {
                session.GetHabbo().GetInventoryComponent().TryAddItem(item);
                session.SendPacket(new FurniListNotificationComposer(item.Id, 1));
                session.SendPacket(new PurchaseOKComposer());
                session.SendPacket(new FurniListAddComposer(item));
                session.SendPacket(new FurniListUpdateComposer());
            }

            //Update the Pet and the Pet figure information.
            room.SendPacket(new UsersComposer(petUser));
            room.SendPacket(new PetHorseFigureInformationComposer(petUser));
        }
    }
}
