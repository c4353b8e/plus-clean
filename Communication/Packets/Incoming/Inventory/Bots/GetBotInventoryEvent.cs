namespace Plus.Communication.Packets.Incoming.Inventory.Bots
{
    using Game.Players;
    using Outgoing.Inventory.Bots;

    internal class GetBotInventoryEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session.GetHabbo().GetInventoryComponent() == null)
            {
                return;
            }

            var bots = session.GetHabbo().GetInventoryComponent().GetBots();
            session.SendPacket(new BotInventoryComposer(bots));
        }
    }
}
