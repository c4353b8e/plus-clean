namespace Plus.Communication.Packets.Outgoing.Inventory.Pets
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.Rooms.AI;

    internal class PetInventoryComposer : ServerPacket
    {
        public PetInventoryComposer(ICollection<Pet> Pets)
            : base(ServerPacketHeader.PetInventoryMessageComposer)
        {
            WriteInteger(1);
            WriteInteger(1);
            WriteInteger(Pets.Count);
            foreach (var Pet in Pets.ToList())
            {
                WriteInteger(Pet.PetId);
               WriteString(Pet.Name);
                WriteInteger(Pet.Type);
                WriteInteger(int.Parse(Pet.Race));
               WriteString(Pet.Color);
                WriteInteger(0);
                WriteInteger(0);
                WriteInteger(0);
            }
        }
    }
}