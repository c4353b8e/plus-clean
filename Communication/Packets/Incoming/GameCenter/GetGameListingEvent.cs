namespace Plus.Communication.Packets.Incoming.GameCenter
{
    using Game.Players;
    using Outgoing.GameCenter;

    internal class GetGameListingEvent : IPacketEvent
    {
        public void Parse(Player Session, ClientPacket Packet)
        {
            var Games = Program.GameContext.GetGameDataManager().GameData;

            Session.SendPacket(new GameListComposer(Games));
        }
    }
}
