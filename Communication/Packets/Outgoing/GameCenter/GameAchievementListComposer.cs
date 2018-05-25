namespace Plus.Communication.Packets.Outgoing.GameCenter
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.Achievements;
    using HabboHotel.GameClients;

    internal class GameAchievementListComposer : ServerPacket
    {
        public GameAchievementListComposer(GameClient Session, ICollection<Achievement> Achievements, int GameId)
            : base(ServerPacketHeader.GameAchievementListMessageComposer)
        {
            WriteInteger(GameId);
            WriteInteger(Achievements.Count);
            foreach (var Ach in Achievements.ToList())
            {
                var UserData = Session.GetHabbo().GetAchievementData(Ach.GroupName);
                var TargetLevel = UserData != null ? UserData.Level + 1 : 1;
           
                var TargetLevelData = Ach.Levels[TargetLevel];

                WriteInteger(Ach.Id); // ach id
                WriteInteger(TargetLevel); // target level
               WriteString(Ach.GroupName + TargetLevel); // badge
                WriteInteger(TargetLevelData.Requirement); // requirement
                WriteInteger(TargetLevelData.Requirement); // requirement
                WriteInteger(TargetLevelData.RewardPixels); // pixels
                WriteInteger(0); // ach score
                WriteInteger(UserData != null ? UserData.Progress : 0); // Current progress
                WriteBoolean(UserData != null ? UserData.Level >= Ach.Levels.Count : false); // Set 100% completed(??)
               WriteString(Ach.Category);
               WriteString("basejump");
                WriteInteger(0); // total levels
                WriteInteger(0);
            }
           WriteString("");
        }
    }
}
