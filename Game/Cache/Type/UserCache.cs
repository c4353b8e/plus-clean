namespace Plus.Game.Cache.Type
{
    using System;

    public class UserCache
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Motto { get; set; }
        public string Look { get; set; }
        public DateTime AddedTime { get; set; }
        public UserCache(int id, string username, string motto, string look)
        {
            Id = id;
            Username = username;
            Motto = motto;
            Look = look;
            AddedTime = DateTime.Now;
        }
        public bool IsExpired()
        {
            var cacheTime = DateTime.Now - AddedTime;
            return cacheTime.TotalMinutes >= 30;
        }
    }
}
