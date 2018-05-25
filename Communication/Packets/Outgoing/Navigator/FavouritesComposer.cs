namespace Plus.Communication.Packets.Outgoing.Navigator
{
    using System.Collections;

    internal class FavouritesComposer : ServerPacket
    {
        public FavouritesComposer(ArrayList favouriteIds)
            : base(ServerPacketHeader.FavouritesMessageComposer)
        {
            WriteInteger(50);
            WriteInteger(favouriteIds.Count);

            foreach (int id in favouriteIds.ToArray())
            {
                WriteInteger(id);
            }
        }
    }
}
