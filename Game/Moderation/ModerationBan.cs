namespace Plus.Game.Moderation
{
    using Utilities;

    public class ModerationBan
    {
        public string Value { get; set; }
        public double Expire { get; set; }
        public string Reason { get; set; }
        public ModerationBanType Type { get; set; }

        public ModerationBan(ModerationBanType type, string value, string reason, double expire)
        {
            Type = type;
            Value = value;
            Reason = reason;
            Expire = expire;
        }

        public bool Expired
        {
            get
            {
                if (UnixUtilities.GetNow() >= Expire)
                {
                    return true;
                }

                return false;
            }
        }
    }
}