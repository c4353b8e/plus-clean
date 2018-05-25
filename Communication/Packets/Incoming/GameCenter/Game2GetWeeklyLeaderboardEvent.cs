namespace Plus.Communication.Packets.Incoming.GameCenter
{
    using HabboHotel.Games;

    internal class Game2GetWeeklyLeaderboardEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            var GameId = Packet.PopInt();

            GameData GameData = null;
            if (Program.GameContext.GetGameDataManager().TryGetGame(GameId, out GameData))
            {
                //Code
            }
        }
    }
}
