namespace Plus.Communication.Packets.Outgoing.Inventory.Bots
{
    using System.Collections.Generic;
    using System.Linq;
    using Game.Users.Inventory.Bots;

    internal class BotInventoryComposer : ServerPacket
    {
        public BotInventoryComposer(ICollection<Bot> Bots)
            : base(ServerPacketHeader.BotInventoryMessageComposer)
        {
            WriteInteger(Bots.Count);
            foreach (var Bot in Bots.ToList())
            {
                WriteInteger(Bot.Id);
               WriteString(Bot.Name);
               WriteString(Bot.Motto);
               WriteString(Bot.Gender);
               WriteString(Bot.Figure);
            }
        }
    }
}