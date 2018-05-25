namespace Plus.Game.Rooms.Chat
{
    using Commands;
    using Core.Logging;
    using Emotions;
    using Filter;
    using Logs;
    using Pets.Commands;
    using Pets.Locale;
    using Styles;

    public sealed class ChatManager
    {
        private static readonly ILogger Logger = new Logger<ChatManager>();

        private readonly ChatEmotionsManager _emotions;

        private readonly ChatlogManager _logs;

        private readonly WordFilterManager _filter;

        private readonly CommandManager _commands;

        private readonly PetCommandManager _petCommands;

        private readonly PetLocale _petLocale;

        private readonly ChatStyleManager _chatStyles;

        public ChatManager()
        {
            _emotions = new ChatEmotionsManager();
            _logs = new ChatlogManager();
         
            _filter = new WordFilterManager();
            _filter.Init();

            _commands = new CommandManager(":");
            _petCommands = new PetCommandManager();
            _petLocale = new PetLocale();
      
            _chatStyles = new ChatStyleManager();
            _chatStyles.Init();

            Logger.Trace("Chat Manager -> LOADED");
        }

        public ChatEmotionsManager GetEmotions()
        {
            return _emotions;
        }

        public ChatlogManager GetLogs()
        {
            return _logs;
        }

        public WordFilterManager GetFilter()
        {
            return _filter;
        }

        public CommandManager GetCommands()
        {
            return _commands;
        }

        public PetCommandManager GetPetCommands()
        {
            return _petCommands;
        }

        public PetLocale GetPetLocale()
        {
            return _petLocale;
        }

        public ChatStyleManager GetChatStyles()
        {
            return _chatStyles;
        }
    }
}
