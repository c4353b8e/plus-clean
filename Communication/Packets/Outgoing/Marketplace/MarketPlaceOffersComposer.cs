namespace Plus.Communication.Packets.Outgoing.Marketplace
{
    using System.Collections.Generic;
    using HabboHotel.Catalog.Marketplace;

    internal class MarketPlaceOffersComposer : ServerPacket
    {
        public MarketPlaceOffersComposer(Dictionary<int, MarketOffer> dictionary, Dictionary<int, int> dictionary2)
            : base(ServerPacketHeader.MarketPlaceOffersMessageComposer)
        {
            WriteInteger(dictionary.Count);
            if (dictionary.Count > 0)
            {
                foreach (var pair in dictionary)
                {
                    WriteInteger(pair.Value.OfferID);
                    WriteInteger(1);//State
                    WriteInteger(1);
                    WriteInteger(pair.Value.SpriteId);

                    WriteInteger(256);
                    WriteString("");
                    WriteInteger(pair.Value.LimitedNumber);
                    WriteInteger(pair.Value.LimitedStack);

                    WriteInteger(pair.Value.TotalPrice);
                    WriteInteger(0);
                    WriteInteger(Program.GameContext.GetCatalog().GetMarketplace().AvgPriceForSprite(pair.Value.SpriteId));
                    WriteInteger(dictionary2[pair.Value.SpriteId]);
                }
            }
            WriteInteger(dictionary.Count);//Item count to show how many were found.
        }
    }
}