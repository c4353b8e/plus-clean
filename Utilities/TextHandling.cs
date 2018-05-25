namespace Plus.Utilities
{
    using System.Globalization;

    public static class TextHandling
    {
        public static string GetString(double k)
        {
            return k.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
        }
    }
}