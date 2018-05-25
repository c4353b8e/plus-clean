namespace Plus.Communication.Packets.Incoming.Rooms.Furni.YouTubeTelevisions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Game.Items.Televisions;
    using Game.Players;
    using Outgoing.Rooms.Furni.YouTubeTelevisions;

    internal class YouTubeGetNextVideo : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var videos = Program.GameContext.GetItemManager().GetTelevisionManager().TelevisionList;

            if (videos.Count == 0)
            {
                session.SendNotification("Oh, it looks like the hotel manager haven't added any videos for you to watch! :(");
                return;
            }

            var itemId = packet.PopInt();
            packet.PopInt(); //next

            TelevisionItem item = null;
            var dict = Program.GameContext.GetItemManager().GetTelevisionManager()._televisions;
            foreach (var value in RandomValues(dict).Take(1))
            {
                item = value;
            }

            if(item == null)
            {
                session.SendNotification("Oh, it looks like their was a problem getting the video.");
                return;
            }

            session.SendPacket(new GetYouTubeVideoComposer(itemId, item.YouTubeId));
        }

        private static IEnumerable<TValue> RandomValues<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            var rand = new Random();
            var values = dict.Values.ToList();
            var size = dict.Count;
            while (true)
            {
                yield return values[rand.Next(size)];
            }
        }
    }
}