﻿namespace Plus.Game.Catalog.Marketplace
{
    public class MarketOffer
    {
        public int OfferID { get; }
        public int ItemType { get; }
        public int SpriteId { get; }
        public int TotalPrice { get; }
        public int LimitedNumber { get; }
        public int LimitedStack { get; }

        public MarketOffer(int offerId, int spriteId, int totalPrice, int itemType, int limitedNumber, int limitedStack)
        {
            OfferID = offerId;
            SpriteId = spriteId;
            ItemType = itemType;
            TotalPrice = totalPrice;
            LimitedNumber = limitedNumber;
            LimitedStack = limitedStack;
        }
    }
}
