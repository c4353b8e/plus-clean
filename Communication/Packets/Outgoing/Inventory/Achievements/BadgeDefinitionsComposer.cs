namespace Plus.Communication.Packets.Outgoing.Inventory.Achievements
{
    using System.Collections.Generic;
    using HabboHotel.Achievements;

    internal class BadgeDefinitionsComposer: ServerPacket
    {
        public BadgeDefinitionsComposer(Dictionary<string, Achievement> Achievements)
            : base(ServerPacketHeader.BadgeDefinitionsMessageComposer)
        {
            WriteInteger(Achievements.Count);

            foreach (var Achievement in Achievements.Values)
            {
               WriteString(Achievement.GroupName.Replace("ACH_", ""));
                WriteInteger(Achievement.Levels.Count);
                foreach (var Level in Achievement.Levels.Values)
                {
                    WriteInteger(Level.Level);
                    WriteInteger(Level.Requirement);
                }
            }
        }
    }
}
