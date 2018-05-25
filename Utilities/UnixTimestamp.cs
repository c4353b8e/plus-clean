namespace Plus.Utilities
{
    using System;

    internal static class UnixTimestamp
    {
        public static double GetNow()
        {
            var ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0);
            return ts.TotalSeconds;
        }

        public static long GetNowMilliseconds()  // TODO: Get rid
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            var unixTime = ts.TotalMilliseconds;
            return (long)unixTime;
        }

        public static DateTime FromUnixTimestamp(double timestamp)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dt = dt.AddSeconds(timestamp);
            return dt;
        }
    }
}
