namespace Plus.Communication.Packets.Incoming.GameCenter
{
    using Game.Games;
    using Game.Players;

    internal class Game2GetWeeklyLeaderboardEvent : IPacketEvent
    {
        public void Parse(Player Session, ClientPacket Packet)
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
