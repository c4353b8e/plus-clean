namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    using System;

    internal class HeightMapComposer : ServerPacket
    {
        public HeightMapComposer(string Map)
            : base(ServerPacketHeader.HeightMapMessageComposer)
        {
            Map = Map.Replace("\n", "");
            var Split = Map.Split('\r');
            WriteInteger(Split[0].Length);
            WriteInteger((Split.Length - 1) * Split[0].Length);
            var x = 0;
            var y = 0;
            for (y = 0; y < Split.Length - 1; y++)
            {
                for (x = 0; x < Split[0].Length; x++)
                {
                    char pos;

                    try
                    {
                        pos = Split[y][x];
                    }
                    catch { pos = 'x'; }

                    if (pos == 'x')
                    {
                        WriteShort(-1);
                    }
                    else
                    {
                        var Height = 0;
                        if (int.TryParse(pos.ToString(), out Height))
                        {
                            Height = Height * 256;
                        }
                        else
                        {
                            Height = (Convert.ToInt32(pos) - 87) * 256;
                        }
                        WriteShort(Height);
                    }
                }
            }
        }
    }
}
