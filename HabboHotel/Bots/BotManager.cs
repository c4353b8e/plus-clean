namespace Plus.HabboHotel.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Core.Logging;
    using Rooms.AI;
    using Rooms.AI.Responses;

    public class BotManager
    {
        private static readonly ILogger Logger = new Logger<BotManager>();

        private readonly List<BotResponse> _responses;

        public BotManager()
        {
            _responses = new List<BotResponse>();
            Init();
        }

        public void Init()
        {
            if (_responses.Count > 0)
            {
                _responses.Clear();
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `bot_ai`,`chat_keywords`,`response_text`,`response_mode`,`response_beverage` FROM `bots_responses`");
                var data = dbClient.GetTable();

                if (data != null)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        _responses.Add(new BotResponse(Convert.ToString(row["bot_ai"]), Convert.ToString(row["chat_keywords"]), Convert.ToString(row["response_text"]), row["response_mode"].ToString(), Convert.ToString(row["response_beverage"])));
                    }
                }
            }
        }

        public BotResponse GetResponse(BotAIType type, string message)
        {
            foreach (var response in _responses.Where(x => x.AiType == type).ToList())
            {
                if (response.KeywordMatched(message))
                {
                    return response;
                }
            }

            return null;
        }
    }
}
