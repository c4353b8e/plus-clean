namespace Plus.Communication.Packets.Incoming.Inventory.Purse
{
    using Game.Players;

    internal class GetHabboClubWindowEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
           // Session.SendNotification("Habbo Club is free for all members, enjoy!");
        }
    }
}
