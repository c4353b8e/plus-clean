namespace Plus.HabboHotel.Items.Televisions
{
    public class TelevisionItem
    {
        public TelevisionItem(int Id, string YouTubeId, string Title, string Description, bool Enabled)
        {
            this.Id = Id;
            this.YouTubeId = YouTubeId;
            this.Title = Title;
            this.Description = Description;
            this.Enabled = Enabled;
        }

        public int Id { get; }

        public string YouTubeId { get; }


        public string Title { get; }

        public string Description { get; }

        public bool Enabled { get; }
    }
}
