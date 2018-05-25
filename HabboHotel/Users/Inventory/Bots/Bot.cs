namespace Plus.HabboHotel.Users.Inventory.Bots
{
    public class Bot
    {
        public Bot(int Id, int OwnerId, string Name, string Motto, string Figure, string Gender)
        {
            this.Id = Id;
            this.OwnerId = OwnerId;
            this.Name = Name;
            this.Motto = Motto;
            this.Figure = Figure;
            this.Gender = Gender;
        }

        public int Id { get; set; }

        public int OwnerId { get; set; }

        public string Name { get; set; }

        public string Motto { get; set; }

        public string Figure { get; set; }

        public string Gender { get; set; }
    }
}
