namespace Plus.Game.Users.Effects
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Communication.Packets.Outgoing.Rooms.Avatar;

    public sealed class EffectsComponent
    {
        private Habbo _habbo;

        private readonly ConcurrentDictionary<int, AvatarEffect> _effects = new ConcurrentDictionary<int, AvatarEffect>();
        
        public bool Init(Habbo habbo)
        {
            if (_effects.Count > 0)
            {
                return false;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `user_effects` WHERE `user_id` = @id;");
                dbClient.AddParameter("id", habbo.Id);
                var getEffects = dbClient.GetTable();

                if (getEffects != null)
                {
                    foreach (DataRow Row in getEffects.Rows)
                    {
                        if (_effects.TryAdd(Convert.ToInt32(Row["id"]), new AvatarEffect(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["user_id"]), Convert.ToInt32(Row["effect_id"]), Convert.ToDouble(Row["total_duration"]), Row["is_activated"].ToString() == "1", Convert.ToDouble(Row["activated_stamp"]), Convert.ToInt32(Row["quantity"]))))
                        {
                            //umm?
                        }
                    }
                }
            }

            _habbo = habbo;
            CurrentEffect = 0;
            return true;
        }

        public bool TryAdd(AvatarEffect Effect)
        {
            return _effects.TryAdd(Effect.Id, Effect);
        }

        public bool HasEffect(int SpriteId, bool ActivatedOnly = false, bool UnactivatedOnly = false)
        {
            return GetEffectNullable(SpriteId, ActivatedOnly, UnactivatedOnly) != null;
        }

        public AvatarEffect GetEffectNullable(int SpriteId, bool ActivatedOnly = false, bool UnactivatedOnly = false)
        {
            foreach (var Effect in _effects.Values.ToList())
            {
                if (!Effect.HasExpired && Effect.SpriteId == SpriteId && (!ActivatedOnly || Effect.Activated) && (!UnactivatedOnly || !Effect.Activated))
                {
                    return Effect;
                }
            }
            return null;
        }

        public void CheckEffectExpiry(Habbo Habbo)
        {
            foreach (var Effect in _effects.Values.ToList())
            {
                if (Effect.HasExpired)
                {
                    Effect.HandleExpiration(Habbo);
                }
            }
        }

        public void ApplyEffect(int EffectId)
        {
            if (_habbo == null || _habbo.CurrentRoom == null)
            {
                return;
            }

            var User = _habbo.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(_habbo.Id);
            if (User == null)
            {
                return;
            }

            CurrentEffect = EffectId;

            if (User.IsDancing)
            {
                _habbo.CurrentRoom.SendPacket(new DanceComposer(User, 0));
            }

            _habbo.CurrentRoom.SendPacket(new AvatarEffectComposer(User.VirtualId, EffectId));
        }

        public ICollection<AvatarEffect> GetAllEffects => _effects.Values;

        public int CurrentEffect { get; set; }

        public void Dispose()
        {
            _effects.Clear();
        }
    }
}
