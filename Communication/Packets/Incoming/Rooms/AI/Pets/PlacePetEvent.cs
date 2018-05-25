namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets
{
    using System;
    using System.Collections.Generic;
    using Core.Logging;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms.AI;
    using HabboHotel.Rooms.AI.Speech;
    using Outgoing.Inventory.Pets;
    using Outgoing.Rooms.Notifications;

    internal class PlacePetEvent : IPacketEvent
    {
        private static readonly ILogger Logger = new Logger<PlacePetEvent>();

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

            if (room.AllowPets == 0 && !room.CheckRights(session, true) || !room.CheckRights(session, true))
            {
                session.SendPacket(new RoomErrorNotifComposer(1));
                return;
            }

            if (room.GetRoomUserManager().PetCount > Convert.ToInt32(Program.SettingsManager.TryGetValue("room.pets.placement_limit")))
            {
                session.SendPacket(new RoomErrorNotifComposer(2));//5 = I have too many.
                return;
            }

            if (!session.GetHabbo().GetInventoryComponent().TryGetPet(packet.PopInt(), out var pet))
            {
                return;
            }

            if (pet == null)
            {
                return;
            }

            if (pet.PlacedInRoom)
            {
                session.SendNotification("This pet is already in the room?");
                return;
            }

            var x = packet.PopInt();
            var y = packet.PopInt();

            if (!room.GetGameMap().CanWalk(x, y, false))
            {
                session.SendPacket(new RoomErrorNotifComposer(4));
                return;
            }

            if (room.GetRoomUserManager().TryGetPet(pet.PetId, out var oldPet))
            {
                room.GetRoomUserManager().RemoveBot(oldPet.VirtualId, false);
            }

            pet.X = x;
            pet.Y = y;

            pet.PlacedInRoom = true;
            pet.RoomId = room.RoomId;

            var rndSpeechList = new List<RandomSpeech>();
            var roomBot = new RoomBot(pet.PetId, pet.RoomId, "pet", "freeroam", pet.Name, "", pet.Look, x, y, 0, 0, 0, 0, 0, 0, ref rndSpeechList, "", 0, pet.OwnerId, false, 0, false, 0);

            room.GetRoomUserManager().DeployBot(roomBot, pet);

            pet.DBState = PetDatabaseUpdateState.NeedsUpdate;
            room.GetRoomUserManager().UpdatePets();

            if (!session.GetHabbo().GetInventoryComponent().TryRemovePet(pet.PetId, out var toRemove))
            {
                Logger.Error("Error whilst removing pet: " + toRemove.PetId);
                return;
            }

            session.SendPacket(new PetInventoryComposer(session.GetHabbo().GetInventoryComponent().GetPets()));
        }
    }
}
