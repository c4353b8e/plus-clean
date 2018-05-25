namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    using System;
    using Game.Items;
    using Utilities;

    internal class ObjectAddComposer : ServerPacket
    {
        public ObjectAddComposer(Item Item)
            : base(ServerPacketHeader.ObjectAddMessageComposer)
        {
            WriteInteger(Item.Id);
            WriteInteger(Item.GetBaseItem().SpriteId);
            WriteInteger(Item.GetX);
            WriteInteger(Item.GetY);
            WriteInteger(Item.Rotation);
            WriteString(string.Format("{0:0.00}", StringUtilities.GetString(Item.GetZ)));
            WriteString(string.Empty);

            if (Item.LimitedNo > 0)
            {
                WriteInteger(1);
                WriteInteger(256);
                WriteString(Item.ExtraData);
                WriteInteger(Item.LimitedNo);
                WriteInteger(Item.LimitedTot);
            }
            else
            {
                ItemBehaviourUtility.GenerateExtradata(Item, this);
            }

            WriteInteger(-1); // to-do: check
            WriteInteger(Item.GetBaseItem().Modes > 1 ? 1 : 0);
            WriteInteger(Item.UserID);
            WriteString(Item.Username);
        }
    }
}