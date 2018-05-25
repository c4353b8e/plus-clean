namespace Plus.Communication.Packets.Incoming.GameCenter
{
    using Outgoing.GameCenter;

    internal class GetGameListingEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            var Games = Program.GameContext.GetGameDataManager().GameData;

            Session.SendPacket(new GameListComposer(Games));
        }
    }
}
