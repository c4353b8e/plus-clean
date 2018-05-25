﻿namespace Plus.Communication.Packets.Outgoing.GameCenter
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.Games;
    using HabboHotel.Users;

    public class Game3WeeklyLeaderboardComposer : ServerPacket
    {
        public Game3WeeklyLeaderboardComposer(GameData GameData, ICollection<Habbo> Habbos)
            : base(ServerPacketHeader.Game3WeeklyLeaderboardMessageComposer)
        {
            WriteInteger(2014);
            WriteInteger(41);
            WriteInteger(0);
            WriteInteger(1);
            WriteInteger(1581);

            //Used to generate the ranking numbers.
            var num = 0;

            WriteInteger(Habbos.Count);//Count
            foreach (var Habbo in Habbos.ToList())
            {
                num++;
                WriteInteger(Habbo.Id);//Id
                WriteInteger(Habbo.FastfoodScore);//Score
                WriteInteger(num);//Rank
               WriteString(Habbo.Username);//Username
               WriteString(Habbo.Look);//Figure
               WriteString(Habbo.Gender.ToLower());//Gender .ToLower()
            }

            WriteInteger(0);//
            WriteInteger(GameData.Id);//Game Id?
        }
    }
}
