namespace Plus.HabboHotel.Users.UserData
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Achievements;
    using Badges;
    using Messenger;
    using Relationships;

    public class UserData
    {
        public int UserId { get; }
        public Habbo user;

        public Dictionary<int, Relationship> Relations;
        public ConcurrentDictionary<string, UserAchievement> achievements;
        public List<Badge> badges;
        public List<int> favouritedRooms;
        public Dictionary<int, MessengerRequest> requests;
        public Dictionary<int, MessengerBuddy> friends;
        public Dictionary<int, int> quests;

        public UserData(int userID, ConcurrentDictionary<string, UserAchievement> achievements, List<int> favouritedRooms,
            List<Badge> badges, Dictionary<int, MessengerBuddy> friends, Dictionary<int, MessengerRequest> requests, Dictionary<int, int> quests, Habbo user, 
            Dictionary<int, Relationship> Relations)
        {
            UserId = userID;
            this.achievements = achievements;
            this.favouritedRooms = favouritedRooms;
            this.badges = badges;
            this.friends = friends;
            this.requests = requests;
            this.quests = quests;
            this.user = user;
            this.Relations = Relations;
        }
    }
}