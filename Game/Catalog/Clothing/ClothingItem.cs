namespace Plus.Game.Catalog.Clothing
{
    using System.Collections.Generic;

    public class ClothingItem
    {
        public int Id { get; }
        public string ClothingName { get; }
        public List<int> PartIds { get; }

        public ClothingItem(int id, string name, string partIds)
        {
            Id = id;
            ClothingName = name;

            PartIds = new List<int>();
            if (partIds.Contains(","))
            {
                foreach (var PartId in partIds.Split(','))
                {
                    PartIds.Add(int.Parse(PartId));
                }
            }
            else if (!string.IsNullOrEmpty(partIds) && int.Parse(partIds) > 0)
            {
                PartIds.Add(int.Parse(partIds));
            }
        }
    }
}
