namespace Plus.Communication.Rcon.Commands.User
{
    using System;

    internal class ProgressUserAchievementCommand : IRconCommand
    {
        public string Description => "This command is used to progress a users achievement.";

        public string Parameters => "%userId% %achievement% %progess%";

        public bool TryExecute(string[] parameters)
        {
            if (!int.TryParse(parameters[0], out var userId))
            {
                return false;
            }

            var client = Program.GameContext.GetClientManager().GetClientByUserId(userId);
            if (client == null || client.GetHabbo() == null)
            {
                return false;
            }

            // Validate the achievement
            if (string.IsNullOrEmpty(Convert.ToString(parameters[1])))
            {
                return false;
            }

            var achievement = Convert.ToString(parameters[1]);

            // Validate the progress
            if (!int.TryParse(parameters[2], out var progress))
            {
                return false;
            }

            Program.GameContext.GetAchievementManager().ProgressAchievement(client, achievement, progress);
            return true;
        }
    }
}