﻿namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets
{
    using Game.Players;
    using Outgoing.Rooms.AI.Pets;

    internal class GetPetInformationEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var petId = packet.PopInt();

            if (!session.GetHabbo().CurrentRoom.GetRoomUserManager().TryGetPet(petId, out var pet))
            {
                //Okay so, we've established we have no pets in this room by this virtual Id, let us check out users, maybe they're creeping as a pet?!
                var user = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(petId);
                if (user == null)
                {
                    return;
                }

                //Check some values first, please!
                if (user.GetClient() == null || user.GetClient().GetHabbo() == null)
                {
                    return;
                }

                //And boom! Let us send the information composer 8-).
                session.SendPacket(new PetInformationComposer(user.GetClient().GetHabbo()));
                return;
            }

            //Continue as a regular pet..
            if (pet.RoomId != session.GetHabbo().CurrentRoomId || pet.PetData == null)
            {
                return;
            }

            session.SendPacket(new PetInformationComposer(pet.PetData));
        }
    }
}
