namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.Moodlight
{
    using Game.Items.Data.Moodlight;

    internal class MoodlightConfigComposer : ServerPacket
    {
        public MoodlightConfigComposer(MoodlightData MoodlightData)
            : base(ServerPacketHeader.MoodlightConfigMessageComposer)
        {
            WriteInteger(MoodlightData.Presets.Count);
            WriteInteger(MoodlightData.CurrentPreset);

            var i = 1;
            foreach (var Preset in MoodlightData.Presets)
            {
                WriteInteger(i);
                WriteInteger(Preset.BackgroundOnly ? 2 : 1);
               WriteString(Preset.ColorCode);
                WriteInteger(Preset.ColorIntensity);
                i++;
            }
        }
    }
}