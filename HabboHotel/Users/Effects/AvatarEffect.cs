namespace Plus.HabboHotel.Users.Effects
{
    using Communication.Packets.Outgoing.Inventory.AvatarEffects;
    using Utilities;

    public sealed class AvatarEffect
    {
        public AvatarEffect(int Id, int UserId, int SpriteId, double Duration, bool Activated, double TimestampActivated, int Quantity)
        {
            this.Id = Id;
            this.UserId = UserId;
            this.SpriteId = SpriteId;
            this.Duration = Duration;
            this.Activated = Activated;
            this.TimestampActivated = TimestampActivated;
            this.Quantity = Quantity;
        }

        public int Id { get; set; }

        public int UserId { get; set; }

        public int SpriteId { get; set; }

        public double Duration { get; set; }

        public bool Activated { get; set; }

        public double TimestampActivated { get; set; }

        public int Quantity { get; set; }

        public double TimeUsed => UnixTimestamp.GetNow() - TimestampActivated;

        public double TimeLeft
        {
            get
            {
                var tl = Activated ? Duration - TimeUsed : Duration;

                if (tl < 0)
                {
                    tl = 0;
                }

                return tl;
            }
        }

        public bool HasExpired => Activated && TimeLeft <= 0;

        public bool Activate()
        {
            var TsNow = UnixTimestamp.GetNow();

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `user_effects` SET `is_activated` = '1', `activated_stamp` = @ts WHERE `id` = @id");
                dbClient.AddParameter("ts", TsNow);
                dbClient.AddParameter("id", Id);
                dbClient.RunQuery();

                Activated = true;
                TimestampActivated = TsNow;
                return true;
            }
        }

        public void HandleExpiration(Habbo Habbo)
        {
            Quantity--;

            Activated = false;
            TimestampActivated = 0;

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                if (Quantity < 1)
                {
                    dbClient.SetQuery("DELETE FROM `user_effects` WHERE `id` = @id");
                    dbClient.AddParameter("id", Id);
                    dbClient.RunQuery();
                }
                else
                {
                    dbClient.SetQuery("UPDATE `user_effects` SET `quantity` = @qt, `is_activated` = '0', `activated_stamp` = 0 WHERE `id` = @id");
                    dbClient.AddParameter("qt", Quantity);
                    dbClient.AddParameter("id", Id);
                    dbClient.RunQuery();
                }
            }

            Habbo.GetClient().SendPacket(new AvatarEffectExpiredComposer(this));
            // reset fx if in room?
        }

        public void AddToQuantity()
        {
            Quantity++;

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `user_effects` SET `quantity` = @qt WHERE `id` = @id");
                dbClient.AddParameter("qt", Quantity);
                dbClient.AddParameter("id", Id);
                dbClient.RunQuery();
            }
        }
    }
}