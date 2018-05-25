namespace Plus.Game.Rooms.Chat.Styles
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Core.Logging;

    public sealed class ChatStyleManager
    {
        private static readonly ILogger Logger = new Logger<ChatStyleManager>();

        private readonly Dictionary<int, ChatStyle> _styles;

        public ChatStyleManager()
        {
            _styles = new Dictionary<int, ChatStyle>();
        }

        public void Init()
        {
            if (_styles.Count > 0)
            {
                _styles.Clear();
            }

            DataTable Table = null;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `room_chat_styles`;");
                Table = dbClient.GetTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        try
                        {
                            if (!_styles.ContainsKey(Convert.ToInt32(Row["id"])))
                            {
                                _styles.Add(Convert.ToInt32(Row["id"]), new ChatStyle(Convert.ToInt32(Row["id"]), Convert.ToString(Row["name"]), Convert.ToString(Row["required_right"])));
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Unable to load ChatBubble for ID [" + Convert.ToInt32(Row["id"]) + "]", ex);
                        }
                    }
                }
            }
        }

        public bool TryGetStyle(int Id, out ChatStyle Style)
        {
            return _styles.TryGetValue(Id, out Style);
        }
    }
}
