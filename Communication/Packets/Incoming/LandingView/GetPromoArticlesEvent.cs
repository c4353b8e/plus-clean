namespace Plus.Communication.Packets.Incoming.LandingView
{
    using Game.Players;
    using Outgoing.LandingView;

    internal class GetPromoArticlesEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var landingPromotions = Program.GameContext.GetLandingManager().GetPromotionItems();

            session.SendPacket(new PromoArticlesComposer(landingPromotions));
        }
    }
}
