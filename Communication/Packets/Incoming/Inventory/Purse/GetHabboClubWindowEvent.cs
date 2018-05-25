namespace Plus.Communication.Packets.Incoming.Inventory.Purse
{
    using HabboHotel.GameClients;

    internal class GetHabboClubWindowEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
           // Session.SendNotification("Habbo Club is free for all members, enjoy!");
        }
    }
}
