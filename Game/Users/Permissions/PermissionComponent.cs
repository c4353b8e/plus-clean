namespace Plus.Game.Users.Permissions
{
    using System.Collections.Generic;

    public sealed class PermissionComponent
    {
        private readonly List<string> _permissions;

        private readonly List<string> _commands;

        public PermissionComponent()
        {
            _permissions = new List<string>();
            _commands = new List<string>();
        }

        public bool Init(Habbo habbo)
        {
            if (_permissions.Count > 0)
            {
                _permissions.Clear();
            }

            if (_commands.Count > 0)
            {
                _commands.Clear();
            }

            _permissions.AddRange(Program.GameContext.GetPermissionManager().GetPermissionsForPlayer(habbo));
            _commands.AddRange(Program.GameContext.GetPermissionManager().GetCommandsForPlayer(habbo));
            return true;
        }

        public bool HasRight(string Right)
        {
            return _permissions.Contains(Right);
        }

        public bool HasCommand(string Command)
        {
            return _commands.Contains(Command);
        }

        public void Dispose()
        {
            _permissions.Clear();
        }
    }
}
