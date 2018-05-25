namespace Plus.Communication.Packets.Incoming.Rooms.AI.Bots
{
    using System;
    using System.Data;
    using HabboHotel.Rooms.AI.Speech;
    using Outgoing;
    using Outgoing.Rooms.Avatar;
    using Outgoing.Rooms.Engine;

    internal class SaveBotActionEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            var botId = packet.PopInt();
            var actionId = packet.PopInt();
            var dataString = packet.PopString();

            if (actionId < 1 || actionId > 5)
            {
                return;
            }

            if (!room.GetRoomUserManager().TryGetBot(botId, out var bot))
            {
                return;
            }

            if (bot.BotData.OwnerId != session.GetHabbo().Id && !session.GetHabbo().GetPermissions().HasRight("bot_edit_any_override"))
            {
                return;
            }

            var roomBot = bot.BotData;
            if (roomBot == null)
            {
                return;
            }

            /* 1 = Copy looks
             * 2 = Setup Speech
             * 3 = Relax
             * 4 = Dance
             * 5 = Change Name
             */

            switch (actionId)
            {
                #region Copy Looks (1)
                case 1:
                    {
                        var userChangeComposer = new ServerPacket(ServerPacketHeader.UserChangeMessageComposer);
                        userChangeComposer.WriteInteger(bot.VirtualId);
                        userChangeComposer.WriteString(session.GetHabbo().Look);
                        userChangeComposer.WriteString(session.GetHabbo().Gender);
                        userChangeComposer.WriteString(bot.BotData.Motto);
                        userChangeComposer.WriteInteger(0);
                        room.SendPacket(userChangeComposer);

                        //Change the defaults
                        bot.BotData.Look = session.GetHabbo().Look;
                        bot.BotData.Gender = session.GetHabbo().Gender;

                        using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `bots` SET `look` = @look, `gender` = '" + session.GetHabbo().Gender + "' WHERE `id` = '" + bot.BotData.Id + "' LIMIT 1");
                            dbClient.AddParameter("look", session.GetHabbo().Look);
                            dbClient.RunQuery();
                        }

                        //Room.SendMessage(new UserChangeComposer(BotUser.GetClient(), true));
                        break;
                    }
                #endregion

                #region Setup Speech (2)
                case 2:
                    {

                        var configData = dataString.Split(new[]
                        {
                            ";#;"
                        }, StringSplitOptions.None);

                        var speechData = configData[0].Split(new[]
                        {
                            '\r',
                            '\n'
                        }, StringSplitOptions.RemoveEmptyEntries);

                        var automaticChat = Convert.ToString(configData[1]);
                        var speakingInterval = Convert.ToString(configData[2]);
                        var mixChat = Convert.ToString(configData[3]);

                        if (string.IsNullOrEmpty(speakingInterval) || Convert.ToInt32(speakingInterval) <= 0 || Convert.ToInt32(speakingInterval) < 7)
                        {
                            speakingInterval = "7";
                        }

                        roomBot.AutomaticChat = Convert.ToBoolean(automaticChat);
                        roomBot.SpeakingInterval = Convert.ToInt32(speakingInterval);
                        roomBot.MixSentences = Convert.ToBoolean(mixChat);

                        using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                        {
                            dbClient.RunQuery("DELETE FROM `bots_speech` WHERE `bot_id` = '" + bot.BotData.Id + "'");

                            #region Save Data - TODO: MAKE METHODS FOR THIS.  

                            for (var i = 0; i <= speechData.Length - 1; i++)
                            {
                                dbClient.SetQuery("INSERT INTO `bots_speech` (`bot_id`, `text`) VALUES (@id, @data)");
                                dbClient.AddParameter("id", botId);
                                dbClient.AddParameter("data", speechData[i]);
                                dbClient.RunQuery();

                                dbClient.SetQuery("UPDATE `bots` SET `automatic_chat` = @AutomaticChat, `speaking_interval` = @SpeakingInterval, `mix_sentences` = @MixChat WHERE `id` = @id LIMIT 1");
                                dbClient.AddParameter("id", botId);
                                dbClient.AddParameter("AutomaticChat", automaticChat.ToLower());
                                dbClient.AddParameter("SpeakingInterval", Convert.ToInt32(speakingInterval));
                                dbClient.AddParameter("MixChat", Convert.ToBoolean(mixChat) ? "1" : "0");
                                dbClient.RunQuery();
                            }
                            #endregion

                            #region Handle Speech
                            roomBot.RandomSpeech.Clear();

                            dbClient.SetQuery("SELECT `text` FROM `bots_speech` WHERE `bot_id` = @id");
                            dbClient.AddParameter("id", botId);

                            var botSpeech = dbClient.GetTable();

                            foreach (DataRow speech in botSpeech.Rows)
                            {
                                roomBot.RandomSpeech.Add(new RandomSpeech(Convert.ToString(speech["text"]), botId));
                            }

                            #endregion
                        }
                        break;
                    }
                #endregion

                #region Relax (3)
                case 3:
                    {
                        if (bot.BotData.WalkingMode == "stand")
                        {
                            bot.BotData.WalkingMode = "freeroam";
                        }
                        else
                        {
                            bot.BotData.WalkingMode = "stand";
                        }

                        using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `bots` SET `walk_mode` = '" + bot.BotData.WalkingMode + "' WHERE `id` = '" + bot.BotData.Id + "' LIMIT 1");
                        }
                        break;
                    }
                #endregion

                #region Dance (4)
                case 4:
                    {
                        if (bot.BotData.DanceId > 0)
                        {
                            bot.BotData.DanceId = 0;
                        }
                        else
                        {
                            var randomDance = new Random();
                            bot.BotData.DanceId = randomDance.Next(1, 4);
                        }

                        room.SendPacket(new DanceComposer(bot, bot.BotData.DanceId));
                        break;
                    }
                #endregion

                #region Change Name (5)
                case 5:
                    {
                        if (dataString.Length == 0)
                        {
                            session.SendWhisper("Come on, atleast give the bot a name!");
                            return;
                        }

                        if (dataString.Length >= 16)
                        {
                            session.SendWhisper("Come on, the bot doesn't need a name that long!");
                            return;
                        }

                        if (dataString.Contains("<img src") || dataString.Contains("<font ") || dataString.Contains("</font>") || dataString.Contains("</a>") || dataString.Contains("<i>"))
                        {
                            session.SendWhisper("No HTML, please :<");
                            return;
                        }

                        bot.BotData.Name = dataString;                      
                        using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `bots` SET `name` = @name WHERE `id` = '" + bot.BotData.Id + "' LIMIT 1");
                            dbClient.AddParameter("name", dataString);
                            dbClient.RunQuery();
                        }
                        room.SendPacket(new UsersComposer(bot));
                        break;
                    }
                #endregion
            }
        }
    }
}
