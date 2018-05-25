namespace Plus.Communication.Packets.Incoming.Inventory.Purse
{
    using Game.Players;
    using Outgoing.Inventory.Purse;

    internal class GetCreditsInfoEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
            session.SendPacket(new ActivityPointsComposer(session.GetHabbo().Duckets, session.GetHabbo().Diamonds, session.GetHabbo().GOTWPoints));
        }
    }
}
