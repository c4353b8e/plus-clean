namespace Plus.HabboHotel.Catalog.Pets
{
    public class PetRace
    {
        public bool _hasPrimaryColour;
        public bool _hasSecondaryColour;

        public PetRace(int raceId, int primaryColour, int secondaryColour, bool hasPrimaryColour, bool hasSecondaryColour)
        {
            RaceId = raceId;
            PrimaryColour = primaryColour;
            SecondaryColour = secondaryColour;
            _hasPrimaryColour = hasPrimaryColour;
            _hasSecondaryColour = hasSecondaryColour;
        }

        public int RaceId { get; set; }

        public int PrimaryColour { get; set; }

        public int SecondaryColour { get; set; }

        public bool HasPrimaryColour
        {
            get => _hasPrimaryColour;
            set => _hasPrimaryColour = value;
        }

        public bool HasSecondaryColour
        {
            get => _hasSecondaryColour;
            set => _hasSecondaryColour = value;
        }
    }
}
