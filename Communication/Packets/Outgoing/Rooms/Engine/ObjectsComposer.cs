namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    using System;
    using Game.Items;
    using Game.Rooms;
    using Utilities;

    internal class ObjectsComposer : ServerPacket
    {
        public ObjectsComposer(Item[] Objects, Room Room)
            : base(ServerPacketHeader.ObjectsMessageComposer)
        {
            WriteInteger(1);

            WriteInteger(Room.OwnerId);
           WriteString(Room.OwnerName);

            WriteInteger(Objects.Length);
            foreach (var Item in Objects)
            {
                WriteFloorItem(Item, Convert.ToInt32(Item.UserID));
            }
        }

        private void WriteFloorItem(Item Item, int UserID)
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
            WriteInteger(UserID);
        }
    }
}