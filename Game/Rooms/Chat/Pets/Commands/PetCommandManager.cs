namespace Plus.Game.Rooms.Chat.Pets.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    public class PetCommandManager
    {
        private readonly Dictionary<int, string> _commandRegister;
        private readonly Dictionary<string, string> _commandDatabase;
        private readonly Dictionary<string, PetCommand> _petCommands;

        public PetCommandManager()
        {
            _petCommands = new Dictionary<string, PetCommand>();
            _commandRegister = new Dictionary<int, string>();
            _commandDatabase = new Dictionary<string, string>();

            Init();
        }

        public void Init()
        {
            _petCommands.Clear();
            _commandRegister.Clear();
            _commandDatabase.Clear();

            DataTable Table = null;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `bots_pet_commands`");
                Table = dbClient.GetTable();

                if (Table != null)
                {
                    foreach (DataRow row in Table.Rows)
                    {
                        _commandRegister.Add(Convert.ToInt32(row[0]), row[1].ToString());
                        _commandDatabase.Add(row[1] + ".input", row[2].ToString());
                    }
                }
            }

            foreach (var pair in _commandRegister)
            {
                var commandID = pair.Key;
                var commandStringedID = pair.Value;
                var commandInput = _commandDatabase[commandStringedID + ".input"].Split(',');

                foreach (var command in commandInput)
                {
                    _petCommands.Add(command, new PetCommand(commandID, command));
                }
            }
        }

        public int TryInvoke(string Input)
        {
            PetCommand Command = null;
            if (_petCommands.TryGetValue(Input.ToLower(), out Command))
            {
                return Command.Id;
            }

            return 0;
        }
    }
}