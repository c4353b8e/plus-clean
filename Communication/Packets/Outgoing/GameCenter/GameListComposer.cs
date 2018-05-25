namespace Plus.Communication.Packets.Outgoing.GameCenter
{
    using System.Collections.Generic;
    using Game.Games;

    internal class GameListComposer : ServerPacket
    {
        public GameListComposer(ICollection<GameData> Games)
            : base(ServerPacketHeader.GameListMessageComposer)
        {
            WriteInteger(Program.GameContext.GetGameDataManager().GetCount());//Game count
            foreach (var Game in Games)
            {
                WriteInteger(Game.Id);
               WriteString(Game.Name);
               WriteString(Game.ColourOne);
               WriteString(Game.ColourTwo);
               WriteString(Game.ResourcePath);
               WriteString(Game.StringThree);
            }
        }
    }
}
