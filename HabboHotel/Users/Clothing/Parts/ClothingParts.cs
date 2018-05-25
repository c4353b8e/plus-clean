namespace Plus.HabboHotel.Users.Clothing.Parts
{
    public sealed class ClothingParts
    {
        public ClothingParts(int Id, int PartId, string Part)
        {
            this.Id = Id;
            this.PartId = PartId;
            this.Part = Part;
        }

        public int Id { get; set; }

        public int PartId { get; set; }

        public string Part { get; set; }
    }
}