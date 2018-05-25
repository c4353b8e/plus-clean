namespace Plus.Game.Users.Effects
{
    using System;

    internal static class AvatarEffectFactory
    {
        public static AvatarEffect CreateNullable(Habbo Habbo, int SpriteId, double Duration)
        {
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `user_effects` (`user_id`,`effect_id`,`total_duration`,`is_activated`,`activated_stamp`,`quantity`) VALUES(@uid,@sid,@dur,'0',0,1)");
                dbClient.AddParameter("uid", Habbo.Id);
                dbClient.AddParameter("sid", SpriteId);
                dbClient.AddParameter("dur", Duration);

                return new AvatarEffect(Convert.ToInt32(dbClient.InsertQuery()), Habbo.Id, SpriteId, Duration, false, 0, 1);
            }
        }
    }
}