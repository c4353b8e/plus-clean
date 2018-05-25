namespace Plus.Utilities
{
    using System.Collections.Generic;

    internal class HabboUtilities
    {
        private static readonly List<char> Allowedchars = new List<char>(new[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l',
            'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
            'y', 'z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-', '.'
        });  // TODO: Get rid

        public static string FilterFigure(string figure)  // TODO: Get rid
        {
            foreach (var character in figure)
            {
                if (!IsValid(character))
                {
                    return "sh-3338-93.ea-1406-62.hr-831-49.ha-3331-92.hd-180-7.ch-3334-93-1408.lg-3337-92.ca-1813-62";
                }
            }

            return figure;
        }

        private static bool IsValid(char character) // TODO: Get rid
        {
            return Allowedchars.Contains(character);
        }

        public static bool IsValidAlphaNumeric(string inputStr) // TODO: Get rid
        {
            inputStr = inputStr.ToLower();
            if (string.IsNullOrEmpty(inputStr))
            {
                return false;
            }

            for (var i = 0; i < inputStr.Length; i++)
            {
                if (!IsValid(inputStr[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
