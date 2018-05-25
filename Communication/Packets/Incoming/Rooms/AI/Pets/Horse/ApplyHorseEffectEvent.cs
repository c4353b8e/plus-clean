namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse
{
    using Game.Items;
    using Game.Players;
    using Outgoing.Rooms.AI.Pets;
    using Outgoing.Rooms.Engine;

    internal class ApplyHorseEffectEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            var itemId = packet.PopInt();
            var item = room.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
            {
                return;
            }

            var petId = packet.PopInt();
            
            if (!room.GetRoomUserManager().TryGetPet(petId, out var petUser))
            {
                return;
            }

            if (petUser.PetData == null || petUser.PetData.OwnerId != session.GetHabbo().Id)
            {
                return;
            }

            if (item.Data.InteractionType == InteractionType.HORSE_SADDLE_1)
            {
                petUser.PetData.Saddle = 9;
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `bots_petdata` SET `have_saddle` = '9' WHERE `id` = '" + petUser.PetData.PetId + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + item.Id + "' LIMIT 1");
                }

                //We only want to use this if we're successful. 
                room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
            }
            else if (item.Data.InteractionType == InteractionType.HORSE_SADDLE_2)
            {
                petUser.PetData.Saddle = 10;
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `bots_petdata` SET `have_saddle` = '10' WHERE `id` = '" + petUser.PetData.PetId + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + item.Id + "' LIMIT 1");
                }

                //We only want to use this if we're successful. 
                room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
            }
            else if (item.Data.InteractionType == InteractionType.HORSE_HAIRSTYLE)
            {
                var parse = 100;
                var hairType = item.GetBaseItem().ItemName.Split('_')[2];

                parse = parse + int.Parse(hairType);

                petUser.PetData.PetHair = parse;
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `bots_petdata` SET `pethair` = '" + petUser.PetData.PetHair + "' WHERE `id` = '" + petUser.PetData.PetId + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + item.Id + "' LIMIT 1");
                }

                //We only want to use this if we're successful. 
                room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
            }
            else if (item.Data.InteractionType == InteractionType.HORSE_HAIR_DYE)
            {
                var hairDye = 48;
                var hairType = item.GetBaseItem().ItemName.Split('_')[2];
            
                hairDye = hairDye + int.Parse(hairType);
                petUser.PetData.HairDye = hairDye;

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `bots_petdata` SET `hairdye` = '" + petUser.PetData.HairDye + "' WHERE `id` = '" + petUser.PetData.PetId + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + item.Id + "' LIMIT 1");
                }

                //We only want to use this if we're successful. 
                room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
            }
            else if (item.Data.InteractionType == InteractionType.HORSE_BODY_DYE)
            {
                var race = item.GetBaseItem().ItemName.Split('_')[2];
                var parse = int.Parse(race);
                var raceLast = 2 + parse * 4 - 4;
                if (parse == 13)
                {
                    raceLast = 61;
                }
                else if (parse == 14)
                {
                    raceLast = 65;
                }
                else if (parse == 15)
                {
                    raceLast = 69;
                }
                else if (parse == 16)
                {
                    raceLast = 73;
                }

                petUser.PetData.Race = raceLast.ToString();

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `bots_petdata` SET `race` = '" + petUser.PetData.Race + "' WHERE `id` = '" + petUser.PetData.PetId + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + item.Id + "' LIMIT 1");
                }

                //We only want to use this if we're successful. 
                room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
            }

            //Update the Pet and the Pet figure information.
            room.SendPacket(new UsersComposer(petUser));
            room.SendPacket(new PetHorseFigureInformationComposer(petUser));
        }
    }
}