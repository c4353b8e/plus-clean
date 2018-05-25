namespace Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.Users.Clothing.Parts;

    internal class FigureSetIdsComposer : ServerPacket
    {
        public FigureSetIdsComposer(ICollection<ClothingParts> ClothingParts)
            : base(ServerPacketHeader.FigureSetIdsMessageComposer)
        {
            WriteInteger(ClothingParts.Count);
            foreach (var Part in ClothingParts.ToList())
            {
                WriteInteger(Part.PartId);
            }

            WriteInteger(ClothingParts.Count);
            foreach (var Part in ClothingParts.ToList())
            {
               WriteString(Part.Part);
            }
        }
    }
}
