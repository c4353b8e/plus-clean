namespace Plus.Communication.Packets.Outgoing.Catalog
{
    using System.Collections.Generic;
    using System.Linq;
    using Game.Catalog.Pets;

    public class SellablePetBreedsComposer : ServerPacket
    {
        public SellablePetBreedsComposer(string PetType, int PetId, ICollection<PetRace> Races)
            : base(ServerPacketHeader.SellablePetBreedsMessageComposer)
        {
           WriteString(PetType);

            WriteInteger(Races.Count);
            foreach (var Race in Races.ToList())
            {
                WriteInteger(PetId);
                WriteInteger(Race.PrimaryColour);
                WriteInteger(Race.SecondaryColour);
                WriteBoolean(Race.HasPrimaryColour);
                WriteBoolean(Race.HasSecondaryColour);
            }


        }
    }
}