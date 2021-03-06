﻿namespace Plus.Communication.Packets.Incoming.Rooms.Chat
{
    using System;
    using Game.Moderation;
    using Game.Players;
    using Game.Quests;
    using Game.Rooms.Chat.Logs;
    using Outgoing.Moderation;
    using Outgoing.Rooms.Chat;
    using Utilities;

    public class ShoutEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;

            if (room == null)
            {
                return;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (user == null)
            {
                return;
            }

            var message = StringUtilities.Escape(packet.PopString());

            if (message.Length > 100)
            {
                message = message.Substring(0, 100);
            }

            var colour = packet.PopInt();

            if (!Program.GameContext.GetChatManager().GetChatStyles().TryGetStyle(colour, out var style) || style.RequiredRight.Length > 0 && !session.GetHabbo().GetPermissions().HasRight(style.RequiredRight))
            {
                colour = 0;
            }

            user.LastBubble = session.GetHabbo().CustomBubbleId == 0 ? colour : session.GetHabbo().CustomBubbleId;

            if (UnixUtilities.GetNow() < session.GetHabbo().FloodTime && session.GetHabbo().FloodTime != 0)
            {
                return;
            }

            if (session.GetHabbo().TimeMuted > 0)
            {
                session.SendPacket(new MutedComposer(session.GetHabbo().TimeMuted));
                return;
            }

            if (!session.GetHabbo().GetPermissions().HasRight("room_ignore_mute") && room.CheckMute(session))
            {
                session.SendWhisper("Oops, you're currently muted.");
                return;
            }

            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                if (user.IncrementAndCheckFlood(out var muteTime))
                {
                    session.SendPacket(new FloodControlComposer(muteTime));
                    return;
                }
            }

            Program.GameContext.GetChatManager().GetLogs().StoreChatlog(new ChatlogEntry(session.GetHabbo().Id, room.Id, message, UnixUtilities.GetNow(), session.GetHabbo(), room));

            if (message.StartsWith(":", StringComparison.CurrentCulture) && Program.GameContext.GetChatManager().GetCommands().Parse(session, message))
            {
                return;
            }

            if (Program.GameContext.GetChatManager().GetFilter().CheckBannedWords(message))
            {
                session.GetHabbo().BannedPhraseCount++;
                if (session.GetHabbo().BannedPhraseCount >= Convert.ToInt32(Program.SettingsManager.TryGetValue("room.chat.filter.banned_phrases.chances")))
                {
                    Program.GameContext.GetModerationManager().BanUser("System", ModerationBanType.Username, session.GetHabbo().Username, "Spamming banned phrases (" + message + ")", UnixUtilities.GetNow() + 78892200);
                    session.Disconnect();
                    return;
                }
                session.SendPacket(new ShoutComposer(user.VirtualId, message, 0, colour));
                return;
            }

            if (!session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
            {
                message = Program.GameContext.GetChatManager().GetFilter().CheckMessage(message);
            }

            Program.GameContext.GetQuestManager().ProgressUserQuest(session, QuestType.SocialChat);

            user.UnIdle();
            user.OnChat(user.LastBubble, message, true);
        }
    }
}