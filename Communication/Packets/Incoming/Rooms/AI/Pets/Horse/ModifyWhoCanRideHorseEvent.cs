namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse
{
    using Game.Players;
    using Outgoing.Rooms.AI.Pets;

    internal class ModifyWhoCanRideHorseEvent : IPacketEvent
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

            var petId = packet.PopInt();
           
            if (!room.GetRoomUserManager().TryGetPet(petId, out var pet))
            {
                return;
            }

            pet.PetData.AnyoneCanRide = pet.PetData.AnyoneCanRide == 1 ? 0 : 1;

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `bots_petdata` SET `anyone_ride` = '" + pet.PetData.AnyoneCanRide + "' WHERE `id` = '" + petId + "' LIMIT 1");
            }

            room.SendPacket(new PetInformationComposer(pet.PetData));
        }
    }
}
