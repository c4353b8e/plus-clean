namespace Plus.Game.Games
{
    public class GameData
    {
        public int Id { get; }
        public string Name { get; }
        public string ColourOne { get; }
        public string ColourTwo { get; }
        public string ResourcePath { get; }
        public string StringThree { get; }
        public string SWF { get; }
        public string Assets { get; }
        public string ServerHost { get; }
        public string ServerPort { get; }
        public string SocketPolicyPort { get; }
        public bool Enabled { get; }

        public GameData(int gameId, string name, string colourOne, string colourTwo, string resourcePath, string stringThree, string gameSWF, string gameAssets, string gameServerHost, string gameServerPort, string socketPolicyPort, bool enabled)
        {
            Id = gameId;
            Name = name;
            ColourOne = colourOne;
            ColourTwo = colourTwo;
            ResourcePath = resourcePath;
            StringThree = stringThree;
            SWF = gameSWF;
            Assets = gameAssets;
            ServerHost = gameServerHost;
            ServerPort = gameServerPort;
            SocketPolicyPort = socketPolicyPort;
            Enabled = enabled;
        }
    }
}
