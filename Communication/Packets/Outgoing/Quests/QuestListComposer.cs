namespace Plus.Communication.Packets.Outgoing.Quests
{
    using System.Collections.Generic;
    using Game.Players;
    using Game.Quests;

    public class QuestListComposer : ServerPacket
    {
        public QuestListComposer(Player Session, bool Send, Dictionary<string, Quest> UserQuests)
            : base(ServerPacketHeader.QuestListMessageComposer)
        {
            WriteInteger(UserQuests.Count);

            // Active ones first
            foreach (var UserQuest in UserQuests)
            {
                if (UserQuest.Value == null)
                {
                    continue;
                }

                SerializeQuest(this, Session, UserQuest.Value, UserQuest.Key);
            }

            // Dead ones last
            foreach (var UserQuest in UserQuests)
            {
                if (UserQuest.Value != null)
                {
                    continue;
                }

                SerializeQuest(this, Session, UserQuest.Value, UserQuest.Key);
            }

            WriteBoolean(Send);
        }

        private void SerializeQuest(ServerPacket Message, Player Session, Quest Quest, string Category)
        {
            if (Message == null || Session == null)
            {
                return;
            }

            var AmountInCat = Program.GameContext.GetQuestManager().GetAmountOfQuestsInCategory(Category);
            var Number = Quest == null ? AmountInCat : Quest.Number - 1;
            var UserProgress = Quest == null ? 0 : Session.GetHabbo().GetQuestProgress(Quest.Id);

            if (Quest != null && Quest.IsCompleted(UserProgress))
            {
                Number++;
            }

            Message.WriteString(Category);
            Message.WriteInteger(Quest == null ? 0 : (Quest.Category.Contains("xmas2012") ? 0 : Number));  // Quest progress in this cat
            Message.WriteInteger(Quest == null ? 0 : Quest.Category.Contains("xmas2012") ? 0 : AmountInCat); // Total quests in this cat
            Message.WriteInteger(Quest == null ? 3 : Quest.RewardType);// Reward type (1 = Snowflakes, 2 = Love hearts, 3 = Pixels, 4 = Seashells, everything else is pixels
            Message.WriteInteger(Quest == null ? 0 : Quest.Id); // Quest id
            Message.WriteBoolean(Quest == null ? false : Session.GetHabbo().GetStats().QuestId == Quest.Id);  // Quest started
            Message.WriteString(Quest == null ? string.Empty : Quest.ActionName);
            Message.WriteString(Quest == null ? string.Empty : Quest.DataBit);
            Message.WriteInteger(Quest == null ? 0 : Quest.Reward);
            Message.WriteString(Quest == null ? string.Empty : Quest.Name);
            Message.WriteInteger(UserProgress); // Current progress
            Message.WriteInteger(Quest == null ? 0 : Quest.GoalData); // Target progress
            Message.WriteInteger(Quest == null ? 0 : Quest.TimeUnlock); // "Next quest available countdown" in seconds
            Message.WriteString("");
            Message.WriteString("");
            Message.WriteBoolean(true);
        }
    }
}