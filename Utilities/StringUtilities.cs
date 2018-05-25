﻿namespace Plus.Utilities
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    public static class StringUtilities
    {
        public static string GetString(double k)
        {
            return k.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
        }

        public static string Escape(string str, bool allowBreaks = false)
        {
            str = str.Trim();

            str = str.Replace(Convert.ToChar(1), ' ');
            str = str.Replace(Convert.ToChar(2), ' ');
            str = str.Replace(Convert.ToChar(3), ' ');
            str = str.Replace(Convert.ToChar(9), ' ');

            if (!allowBreaks)
            {
                str = str.Replace(Convert.ToChar(10), ' ');
                str = str.Replace(Convert.ToChar(13), ' ');
            }

            str = Regex.Replace(str, "<(.|\\n)*?>", string.Empty);

            return str;
        }
    }
}