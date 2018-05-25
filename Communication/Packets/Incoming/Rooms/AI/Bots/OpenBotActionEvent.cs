namespace Plus.Communication.Packets.Incoming.Rooms.AI.Bots
{
    using System.Linq;
    using Game.Players;
    using Outgoing.Rooms.AI.Bots;

    internal class OpenBotActionEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var botId = packet.PopInt();
            var actionId = packet.PopInt();

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            if (!room.GetRoomUserManager().TryGetBot(botId, out var botUser))
            {
                return;
            }

            var botSpeech = "";
            foreach (var speech in botUser.BotData.RandomSpeech.ToList())
            {
                botSpeech += speech.Message + "\n";
            }

            botSpeech += ";#;";
            botSpeech += botUser.BotData.AutomaticChat;
            botSpeech += ";#;";
            botSpeech += botUser.BotData.SpeakingInterval;
            botSpeech += ";#;";
            botSpeech += botUser.BotData.MixSentences;

            if (actionId == 2 || actionId == 5)
            {
                session.SendPacket(new OpenBotActionComposer(botUser, actionId, botSpeech));
            }
        }
    }
}