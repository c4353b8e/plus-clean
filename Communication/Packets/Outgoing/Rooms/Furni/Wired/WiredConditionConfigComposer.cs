namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.Wired
{
    using System;
    using System.Linq;
    using Game.Items.Wired;

    internal class WiredConditionConfigComposer : ServerPacket
    {
        public WiredConditionConfigComposer(IWiredItem Box)
            : base(ServerPacketHeader.WiredConditionConfigMessageComposer)
        {
            WriteBoolean(false);
            WriteInteger(5);

            WriteInteger(Box.SetItems.Count);
            foreach (var Item in Box.SetItems.Values.ToList())
            {
                WriteInteger(Item.Id);
            }

            WriteInteger(Box.Item.GetBaseItem().SpriteId);
            WriteInteger(Box.Item.Id);
           WriteString(Box.StringData);

            if (Box.Type == WiredBoxType.ConditionMatchStateAndPosition || Box.Type == WiredBoxType.ConditionDontMatchStateAndPosition)
            {
                if (string.IsNullOrEmpty(Box.StringData))
                {
                    Box.StringData = "0;0;0";
                }

                WriteInteger(3);//Loop
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[0]) : 0);
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[1]) : 0);
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[2]) : 0);
            }
            else if (Box.Type == WiredBoxType.ConditionUserCountInRoom || Box.Type == WiredBoxType.ConditionUserCountDoesntInRoom)
            {
                if (string.IsNullOrEmpty(Box.StringData))
                {
                    Box.StringData = "0;0";
                }

                WriteInteger(2);//Loop
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[0]) : 1);
                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[1]) : 50);
            }

            if (Box.Type == WiredBoxType.ConditionFurniHasNoFurni)
            {
                WriteInteger(1);
            }

            if (Box.Type != WiredBoxType.ConditionUserCountInRoom && Box.Type != WiredBoxType.ConditionUserCountDoesntInRoom && Box.Type != WiredBoxType.ConditionFurniHasNoFurni)
            {
                WriteInteger(0);
            }
            else if (Box.Type == WiredBoxType.ConditionFurniHasNoFurni)
            {
                if (string.IsNullOrEmpty(Box.StringData))
                {
                    Box.StringData = "0";
                }

                WriteInteger(Box.StringData != null ? int.Parse(Box.StringData.Split(';')[0]) : 50);
            }
            WriteInteger(0);
            WriteInteger(WiredBoxTypeUtility.GetWiredId(Box.Type));
        }
    }
}