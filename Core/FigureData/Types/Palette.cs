namespace Plus.Core.FigureData.Types
{
    using System.Collections.Generic;

    public class Palette
    {
        public int Id { get; set; }
        public Dictionary<int, Color> Colors { get; set; }

        public Palette(int id)
        {
            Id = id;
            Colors = new Dictionary<int, Color>();
        }

    }
}
