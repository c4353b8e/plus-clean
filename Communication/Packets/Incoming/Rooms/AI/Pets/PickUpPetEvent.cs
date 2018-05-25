namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets
{
    using System.Drawing;
    using HabboHotel.GameClients;
    using Outgoing.Inventory.Pets;
    using Outgoing.Rooms.Engine;

    internal class PickUpPetEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            if (session.GetHabbo() == null || session.GetHabbo().GetInventoryComponent() == null)
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            var petId = packet.PopInt();

            if (!room.GetRoomUserManager().TryGetPet(petId, out var pet))
            {
                //Check kick rights, just because it seems most appropriate.
                if (!room.CheckRights(session) && room.WhoCanKick != 2 && room.Group == null || room.Group != null && !room.CheckRights(session, false, true))
                {
                    return;
                }

                //Okay so, we've established we have no pets in this room by this virtual Id, let us check out users, maybe they're creeping as a pet?!
                var targetUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(petId);
                if (targetUser == null)
                {
                    return;
                }

                //Check some values first, please!
                if (targetUser.GetClient() == null || targetUser.GetClient().GetHabbo() == null)
                {
                    return;
                }

                //Update the targets PetId.
                targetUser.GetClient().GetHabbo().PetId = 0;

                //Quickly remove the old user instance.
                room.SendPacket(new UserRemoveComposer(targetUser.VirtualId));

                //Add the new one, they won't even notice a thing!!11 8-)
                room.SendPacket(new UsersComposer(targetUser));
                return;
            }

            if (session.GetHabbo().Id != pet.PetData.OwnerId && !room.CheckRights(session, true))
            {
                session.SendWhisper("You can only pickup your own pets, to kick a pet you must have room rights.");
                return;
            }

            if (pet.RidingHorse)
            {
                var userRiding = room.GetRoomUserManager().GetRoomUserByVirtualId(pet.HorseID);
                if (userRiding != null)
                {
                    userRiding.RidingHorse = false;
                    userRiding.ApplyEffect(-1);
                    userRiding.MoveTo(new Point(userRiding.X + 1, userRiding.Y + 1));
                }
                else
                {
                    pet.RidingHorse = false;
                }
            }

            pet.PetData.RoomId = 0;
            pet.PetData.PlacedInRoom = false;

            var data = pet.PetData;
            if (data != null)
            {
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `bots` SET `room_id` = '0', `x` = '0', `Y` = '0', `Z` = '0' WHERE `id` = '" + data.PetId + "' LIMIT 1");
                    dbClient.RunQuery("UPDATE `bots_petdata` SET `experience` = '" + data.experience + "', `energy` = '" + data.Energy + "', `nutrition` = '" + data.Nutrition + "', `respect` = '" + data.Respect + "' WHERE `id` = '" + data.PetId + "' LIMIT 1");
                }
            }

            if (data.OwnerId != session.GetHabbo().Id)
            {
                var target = Program.GameContext.GetClientManager().GetClientByUserId(data.OwnerId);
                if (target != null)
                {
                    target.GetHabbo().GetInventoryComponent().TryAddPet(pet.PetData);
                    room.GetRoomUserManager().RemoveBot(pet.VirtualId, false);

                    target.SendPacket(new PetInventoryComposer(target.GetHabbo().GetInventoryComponent().GetPets()));
                    return;
                }
            }
            
            room.GetRoomUserManager().RemoveBot(pet.VirtualId, false);
        }
    }
}
