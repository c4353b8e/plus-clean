namespace Plus.HabboHotel.Catalog
{
    using Items;

    public class CatalogItem
    {
        public int Id { get; }
        public int ItemId { get; }
        public ItemData Data { get; }
        public int Amount { get; }
        public int CostCredits { get; }
        public string ExtraData { get; }
        public bool HaveOffer { get; }
        public bool IsLimited { get; }
        public string Name { get; }
        public int PageID { get; }
        public int CostPixels { get; }
        public int LimitedEditionStack { get; }
        public int LimitedEditionSells { get; set; }
        public int CostDiamonds { get; }
        public string Badge { get; }
        public int OfferId { get; }

        public CatalogItem(int id, int itemId, ItemData data, string catalogName, int pageId, int costCredits, int costPixels,
            int costDiamonds, int amount, int limitedEditionSells, int limitedEditionStack, bool hasOffer, string extraData, string badge, int offerId)
        {
            Id = id;
            Name = catalogName;
            ItemId = itemId;
            Data = data;
            PageID = pageId;
            CostCredits = costCredits;
            CostPixels = costPixels;
            CostDiamonds = costDiamonds;
            Amount = amount;
            LimitedEditionSells = limitedEditionSells;
            LimitedEditionStack = limitedEditionStack;
            IsLimited = limitedEditionStack > 0;
            HaveOffer = hasOffer;
            ExtraData = extraData;
            Badge = badge;
            OfferId = offerId;
        }
    }
}