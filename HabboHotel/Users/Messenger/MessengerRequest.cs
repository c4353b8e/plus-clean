namespace Plus.HabboHotel.Users.Messenger
{
    public class MessengerRequest
    {
        public MessengerRequest(int ToUser, int FromUser, string Username)
        {
            To = ToUser;
            From = FromUser;
            this.Username = Username;
        }

        public string Username { get; }

        public int To { get; }

        public int From { get; }
    }
}