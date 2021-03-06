﻿namespace Plus.Communication.Packets.Incoming.GameCenter
{
    using Game.Players;
    using Outgoing.GameCenter;

    internal class GetPlayableGamesEvent : IPacketEvent
    {
        public void Parse(Player Session, ClientPacket Packet)
        {
            var GameId = Packet.PopInt();

            Session.SendPacket(new GameAccountStatusComposer(GameId));
            Session.SendPacket(new PlayableGamesComposer(GameId));
            Session.SendPacket(new GameAchievementListComposer(Session, Program.GameContext.GetAchievementManager().GetGameAchievements(GameId), GameId));
        }
    }
}