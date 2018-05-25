namespace Plus.Game.Rooms.AI.Responses
{
    using System.Collections.Generic;
    using Catalog.Utilities;

    public class BotResponse
    {
        public BotAIType AiType { get; set; }
        public List<string> Keywords { get; set; }
        public string ResponseText { get; set; }
        public string ResponseType { get; set; }
        public List<int> BeverageIds { get; }

        public BotResponse(string BotAi, string Keywords, string ResponseText, string ResponseMode, string ResponseBeverages)
        {
            AiType = BotUtility.GetAIFromString(BotAi);
           
            this.Keywords = new List<string>();
            foreach (var Keyword in Keywords.Split(','))
            {
                this.Keywords.Add(Keyword.ToLower());
            }

            this.ResponseText = ResponseText;
            ResponseType = ResponseMode;

            BeverageIds = new List<int>(); 
            if (ResponseBeverages.Contains(","))
            {
                foreach (var VendingId in ResponseBeverages.Split(','))
                {
                    try
                    {
                        BeverageIds.Add(int.Parse(VendingId));
                    }
                    catch
                    {
                    }
                }
            }
            else if (!string.IsNullOrEmpty(ResponseBeverages) && int.Parse(ResponseBeverages) > 0)
            {
                BeverageIds.Add(int.Parse(ResponseBeverages));
            }
        }

        public bool KeywordMatched(string Message)
        { 
            foreach (var Keyword in Keywords)
            {
                if (Message.ToLower().Contains(Keyword.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}