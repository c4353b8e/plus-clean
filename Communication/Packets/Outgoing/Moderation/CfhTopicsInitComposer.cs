namespace Plus.Communication.Packets.Outgoing.Moderation
{
    using System.Collections.Generic;
    using System.Linq;
    using Game.Moderation;

    internal class CfhTopicsInitComposer : ServerPacket
    {
        public CfhTopicsInitComposer(Dictionary<string, List<ModerationPresetActions>> UserActionPresets)
            : base(ServerPacketHeader.CfhTopicsInitMessageComposer)
        {

            WriteInteger(UserActionPresets.Count);
            foreach (var Cat in UserActionPresets.ToList())
            {
                WriteString(Cat.Key);
                WriteInteger(Cat.Value.Count);
                foreach (var Preset in Cat.Value.ToList())
                {
                    WriteString(Preset.Caption);
                    WriteInteger(Preset.Id);
                    WriteString(Preset.Type);
                }
            }
        }
    }
}
