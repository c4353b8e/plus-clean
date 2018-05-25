namespace Plus.HabboHotel.Rooms.Instance
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Items;
    using Items.Wired;
    using Items.Wired.Boxes;
    using Items.Wired.Boxes.Conditions;
    using Items.Wired.Boxes.Effects;
    using Items.Wired.Boxes.Triggers;

    public class WiredComponent
    {
        private readonly Room _room;
        private readonly ConcurrentDictionary<int, IWiredItem> _wiredItems;

        public WiredComponent(Room Instance)//, RoomItem Items)
        {
            _room = Instance;
            _wiredItems = new ConcurrentDictionary<int, IWiredItem>();
        }

        public void OnCycle()
        {
            var Start = DateTime.Now;
            foreach (var Item in _wiredItems.ToList())
            {
                var SelectedItem = _room.GetRoomItemHandler().GetItem(Item.Value.Item.Id);

                if (SelectedItem == null)
                {
                    TryRemove(Item.Key);
                }

                if (Item.Value is IWiredCycle)
                {
                    var Cycle = (IWiredCycle)Item.Value;

                    if (Cycle.TickCount <= 0)
                    {
                        Cycle.OnCycle();
                    }
                    else
                    {
                        Cycle.TickCount--;
                    }
                }
            }
            var Span = DateTime.Now - Start;
            if (Span.Milliseconds > 400)
            {
                //Logger.Warn("<Room " + _room.Id + "> Wired took " + Span.TotalMilliseconds + "ms to execute - Rooms lagging behind");
            }
        }

        public IWiredItem LoadWiredBox(Item Item)
        {
            var NewBox = GenerateNewBox(Item);

            DataRow Row = null;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM wired_items WHERE id=@id LIMIT 1");
                dbClient.AddParameter("id", Item.Id);
                Row = dbClient.GetRow();

                if (Row != null)
                {
                    if (string.IsNullOrEmpty(Convert.ToString(Row["string"])))
                    {
                        if (NewBox.Type == WiredBoxType.ConditionMatchStateAndPosition || NewBox.Type == WiredBoxType.ConditionDontMatchStateAndPosition)
                        {
                            NewBox.StringData = "0;0;0";
                        }
                        else if (NewBox.Type == WiredBoxType.ConditionUserCountInRoom || NewBox.Type == WiredBoxType.ConditionUserCountDoesntInRoom)
                        {
                            NewBox.StringData = "0;0";
                        }
                        else if (NewBox.Type == WiredBoxType.ConditionFurniHasNoFurni)
                        {
                            NewBox.StringData = "0";
                        }
                        else if (NewBox.Type == WiredBoxType.EffectMatchPosition)
                        {
                            NewBox.StringData = "0;0;0";
                        }
                        else if (NewBox.Type == WiredBoxType.EffectMoveAndRotate)
                        {
                            NewBox.StringData = "0;0";
                        }
                    }

                    NewBox.StringData = Convert.ToString(Row["string"]);
                    NewBox.BoolData = Convert.ToInt32(Row["bool"]) == 1;
                    NewBox.ItemsData = Convert.ToString(Row["items"]);

                    if (NewBox is IWiredCycle)
                    {
                        var Box = (IWiredCycle)NewBox;
                        Box.Delay = Convert.ToInt32(Row["delay"]);
                    }

                    foreach (var str in Convert.ToString(Row["items"]).Split(';'))
                    {
                        var Id = 0;
                        var sId = "0";

                        if (str.Contains(':'))
                        {
                            sId = str.Split(':')[0];
                        }

                        if (int.TryParse(str, out Id) || int.TryParse(sId, out Id))
                        {
                            var SelectedItem = _room.GetRoomItemHandler().GetItem(Convert.ToInt32(Id));
                            if (SelectedItem == null)
                            {
                                continue;
                            }

                            NewBox.SetItems.TryAdd(SelectedItem.Id, SelectedItem);
                        }
                    }
                }
                else
                {
                    NewBox.ItemsData = "";
                    NewBox.StringData = "";
                    NewBox.BoolData = false;
                    SaveBox(NewBox);
                }
            }

            if (!AddBox(NewBox))
            {
                // ummm
            }
            return NewBox;
        }

        public IWiredItem GenerateNewBox(Item Item)
        {
            switch (Item.GetBaseItem().WiredType)
            {
                case WiredBoxType.TriggerRoomEnter:
                    return new RoomEnterBox(_room, Item);
                case WiredBoxType.TriggerRepeat:
                    return new RepeaterBox(_room, Item);
                case WiredBoxType.TriggerStateChanges:
                    return new StateChangesBox(_room, Item);
                case WiredBoxType.TriggerUserSays:
                    return new UserSaysBox(_room, Item);
                case WiredBoxType.TriggerWalkOffFurni:
                    return new UserWalksOffBox(_room, Item);
                case WiredBoxType.TriggerWalkOnFurni:
                    return new UserWalksOnBox(_room, Item);
                case WiredBoxType.TriggerGameStarts:
                    return new GameStartsBox(_room, Item);
                case WiredBoxType.TriggerGameEnds:
                    return new GameEndsBox(_room, Item);
                case WiredBoxType.TriggerUserFurniCollision:
                    return new UserFurniCollision(_room, Item);
                case WiredBoxType.TriggerUserSaysCommand:
                    return new UserSaysCommandBox(_room, Item);

                case WiredBoxType.EffectShowMessage:
                    return new ShowMessageBox(_room, Item);
                case WiredBoxType.EffectTeleportToFurni:
                    return new TeleportUserBox(_room, Item);
                case WiredBoxType.EffectToggleFurniState:
                    return new ToggleFurniBox(_room, Item);
                case WiredBoxType.EffectMoveAndRotate:
                    return new MoveAndRotateBox(_room, Item);
                case WiredBoxType.EffectKickUser:
                    return new KickUserBox(_room, Item);
                case WiredBoxType.EffectMuteTriggerer:
                    return new MuteTriggererBox(_room, Item);

                case WiredBoxType.EffectGiveReward:
                    return new GiveRewardBox(_room, Item);


                case WiredBoxType.EffectMatchPosition:
                    return new MatchPositionBox(_room, Item);
                case WiredBoxType.EffectAddActorToTeam:
                    return new AddActorToTeamBox(_room, Item);
                case WiredBoxType.EffectRemoveActorFromTeam:
                    return new RemoveActorFromTeamBox(_room, Item);
                /*
                
                case WiredBoxType.EffectMoveFurniToNearestUser:
                    return new MoveFurniToNearestUserBox(_room, Item);
                case WiredBoxType.EffectMoveFurniFromNearestUser:
                    return new MoveFurniFromNearestUserBox(_room, Item);

                   */
                case WiredBoxType.ConditionFurniHasUsers:
                    return new FurniHasUsersBox(_room, Item);
                case WiredBoxType.ConditionTriggererOnFurni:
                    return new TriggererOnFurniBox(_room, Item);
                case WiredBoxType.ConditionTriggererNotOnFurni:
                    return new TriggererNotOnFurniBox(_room, Item);
                case WiredBoxType.ConditionFurniHasNoUsers:
                    return new FurniHasNoUsersBox(_room, Item);
                case WiredBoxType.ConditionFurniHasFurni:
                    return new FurniHasFurniBox(_room, Item);
                case WiredBoxType.ConditionIsGroupMember:
                    return new IsGroupMemberBox(_room, Item);
                case WiredBoxType.ConditionIsNotGroupMember:
                    return new IsNotGroupMemberBox(_room, Item);
                case WiredBoxType.ConditionUserCountInRoom:
                    return new UserCountInRoomBox(_room, Item);
                case WiredBoxType.ConditionUserCountDoesntInRoom:
                    return new UserCountDoesntInRoomBox(_room, Item);
                case WiredBoxType.ConditionIsWearingFX:
                    return new IsWearingFXBox(_room, Item);
                case WiredBoxType.ConditionIsNotWearingFX:
                    return new IsNotWearingFXBox(_room, Item);
                case WiredBoxType.ConditionIsWearingBadge:
                    return new IsWearingBadgeBox(_room, Item);
                case WiredBoxType.ConditionIsNotWearingBadge:
                    return new IsNotWearingBadgeBox(_room, Item);
                case WiredBoxType.ConditionMatchStateAndPosition:
                    return new FurniMatchStateAndPositionBox(_room, Item);
                case WiredBoxType.ConditionDontMatchStateAndPosition:
                    return new FurniDoesntMatchStateAndPositionBox(_room, Item);
                case WiredBoxType.ConditionFurniHasNoFurni:
                    return new FurniHasNoFurniBox(_room, Item);
                case WiredBoxType.ConditionActorHasHandItemBox:
                    return new ActorHasHandItemBox(_room, Item);
                case WiredBoxType.ConditionActorIsInTeamBox:
                    return new ActorIsInTeamBox(_room, Item);


                /*
                case WiredBoxType.ConditionMatchStateAndPosition:
                    return new FurniMatchStateAndPositionBox(_room, Item);

                case WiredBoxType.ConditionFurniTypeMatches:
                    return new FurniTypeMatchesBox(_room, Item);
                case WiredBoxType.ConditionFurniTypeDoesntMatch:
                    return new FurniTypeDoesntMatchBox(_room, Item);
                case WiredBoxType.ConditionFurniHasNoFurni:
                    return new FurniHasNoFurniBox(_room, Item);*/

                case WiredBoxType.AddonRandomEffect:
                    return new AddonRandomEffectBox(_room, Item);
                case WiredBoxType.EffectMoveFurniToNearestUser:
                    return new MoveFurniToUserBox(_room, Item);
                case WiredBoxType.EffectExecuteWiredStacks:
                    return new ExecuteWiredStacksBox(_room, Item);

                case WiredBoxType.EffectTeleportBotToFurniBox:
                    return new TeleportBotToFurniBox(_room, Item);
                case WiredBoxType.EffectBotChangesClothesBox:
                    return new BotChangesClothesBox(_room, Item);
                case WiredBoxType.EffectBotMovesToFurniBox:
                    return new BotMovesToFurniBox(_room, Item);
                case WiredBoxType.EffectBotCommunicatesToAllBox:
                    return new BotCommunicatesToAllBox(_room, Item);

                case WiredBoxType.EffectBotGivesHanditemBox:
                    return new BotGivesHandItemBox(_room, Item);
                case WiredBoxType.EffectBotFollowsUserBox:
                    return new BotFollowsUserBox(_room, Item);
                case WiredBoxType.EffectSetRollerSpeed:
                    return new SetRollerSpeedBox(_room, Item);
                case WiredBoxType.EffectRegenerateMaps:
                    return new RegenerateMapsBox(_room, Item);
                case WiredBoxType.EffectGiveUserBadge:
                    return new GiveUserBadgeBox(_room, Item);
            }
            return null;
        }

        public bool IsTrigger(Item Item)
        {
            return Item.GetBaseItem().InteractionType == InteractionType.WIRED_TRIGGER;
        }

        public bool IsEffect(Item Item)
        {
            return Item.GetBaseItem().InteractionType == InteractionType.WIRED_EFFECT;
        }

        public bool IsCondition(Item Item)
        {
            return Item.GetBaseItem().InteractionType == InteractionType.WIRED_CONDITION;
        }

        public bool OtherBoxHasItem(IWiredItem Box, int ItemId)
        {
            if (Box == null)
            {
                return false;
            }

            ICollection<IWiredItem> Items = GetEffects(Box).Where(x => x.Item.Id != Box.Item.Id).ToList();

            if (Items != null && Items.Count > 0)
            {
                foreach (var Item in Items)
                {
                    if (Item.Type != WiredBoxType.EffectMoveAndRotate && Item.Type != WiredBoxType.EffectMoveFurniFromNearestUser && Item.Type != WiredBoxType.EffectMoveFurniToNearestUser)
                    {
                        continue;
                    }

                    if (Item.SetItems == null || Item.SetItems.Count == 0)
                    {
                        continue;
                    }

                    if (Item.SetItems.ContainsKey(ItemId))
                    {
                        return true;
                    }
                }
            }


            return false;
        }

        public bool TriggerEvent(WiredBoxType Type, params object[] Params)
        {
            var Finished = false;
            try
            {
                if (Type == WiredBoxType.TriggerUserSays)
                {
                    var RanBoxes = new List<IWiredItem>();
                    foreach (var Box in _wiredItems.Values.ToList())
                    {
                        if (Box == null)
                        {
                            continue;
                        }

                        if (Box.Type == WiredBoxType.TriggerUserSays)
                        {
                            if (!RanBoxes.Contains(Box))
                            {
                                RanBoxes.Add(Box);
                            }
                        }
                    }

                    var Message = Convert.ToString(Params[1]);
                    foreach (var Box in RanBoxes.ToList())
                    {
                        if (Box == null)
                        {
                            continue;
                        }

                        if (Message.Contains(" " + Box.StringData) || Message.Contains(Box.StringData + " ") || Message == Box.StringData)
                        {
                            Finished = Box.Execute(Params);
                        }
                    }
                    return Finished;
                }

                foreach (var Box in _wiredItems.Values.ToList())
                {
                    if (Box == null)
                    {
                        continue;
                    }

                    if (Box.Type == Type && IsTrigger(Box.Item))
                    {
                        Finished = Box.Execute(Params);
                    }
                }
            }
            catch
            {
                //Logger.Error("Error when triggering Wired Event: " + e);
                return false;
            }

            return Finished;
        }

        public ICollection<IWiredItem> GetTriggers(IWiredItem Item)
        {
            var Items = new List<IWiredItem>();
            foreach (var I in _wiredItems.Values)
            {
                if (IsTrigger(I.Item) && I.Item.GetX == Item.Item.GetX && I.Item.GetY == Item.Item.GetY)
                {
                    Items.Add(I);
                }
            }

            return Items;
        }

        public ICollection<IWiredItem> GetEffects(IWiredItem Item)
        {
            var Items = new List<IWiredItem>();

            foreach (var I in _wiredItems.Values)
            {
                if (IsEffect(I.Item) && I.Item.GetX == Item.Item.GetX && I.Item.GetY == Item.Item.GetY)
                {
                    Items.Add(I);
                }
            }

            return Items.OrderBy(x => x.Item.GetZ).ToList();
        }

        public IWiredItem GetRandomEffect(ICollection<IWiredItem> Effects)
        {
            return Effects.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
        }

        public bool OnUserFurniCollision(Room room, Item item)
        {
            if (room == null || item == null)
            {
                return false;
            }

            foreach (var Point in item.GetSides())
            {
                if (room.GetGameMap().SquareHasUsers(Point.X, Point.Y))
                {
                    var Users = room.GetGameMap().GetRoomUsers(Point);
                    if (Users != null && Users.Count > 0)
                    {
                        foreach (var User in Users.ToList())
                        {
                            if (User == null)
                            {
                                continue;
                            }

                            item.UserFurniCollision(User);
                        }
                    }
                }
            }

            return true;
        }

        public ICollection<IWiredItem> GetConditions(IWiredItem Item)
        {
            var Items = new List<IWiredItem>();

            foreach (var I in _wiredItems.Values)
            {
                if (IsCondition(I.Item) && I.Item.GetX == Item.Item.GetX && I.Item.GetY == Item.Item.GetY)
                {
                    Items.Add(I);
                }
            }

            return Items;
        }

        public void OnEvent(Item Item)
        {
            if (Item.ExtraData == "1")
            {
                return;
            }

            Item.ExtraData = "1";
            Item.UpdateState(false, true);
            Item.RequestUpdate(2, true);
        }

        public void SaveBox(IWiredItem Item)
        {
            var Items = "";
            IWiredCycle Cycle = null;
            if (Item is IWiredCycle)
            {
                Cycle = (IWiredCycle)Item;
            }

            foreach (var I in Item.SetItems.Values)
            {
                var SelectedItem = _room.GetRoomItemHandler().GetItem(Convert.ToInt32(I.Id));
                if (SelectedItem == null)
                {
                    continue;
                }

                if (Item.Type == WiredBoxType.EffectMatchPosition || Item.Type == WiredBoxType.ConditionMatchStateAndPosition || Item.Type == WiredBoxType.ConditionDontMatchStateAndPosition)
                {
                    Items += I.Id + ":" + I.GetX + "," + I.GetY + "," + I.GetZ + "," + I.Rotation + "," + I.ExtraData + ";";
                }
                else
                {
                    Items += I.Id + ";";
                }
            }

            if (Item.Type == WiredBoxType.EffectMatchPosition || Item.Type == WiredBoxType.ConditionMatchStateAndPosition || Item.Type == WiredBoxType.ConditionDontMatchStateAndPosition)
            {
                Item.ItemsData = Items;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("REPLACE INTO `wired_items` VALUES (@id, @items, @delay, @string, @bool)");
                dbClient.AddParameter("id", Item.Item.Id);
                dbClient.AddParameter("items", Items);
                dbClient.AddParameter("delay", Item is IWiredCycle ? Cycle.Delay : 0);
                dbClient.AddParameter("string", Item.StringData);
                dbClient.AddParameter("bool", Item.BoolData ? "1" : "0");
                dbClient.RunQuery();
            }
        }

        public bool AddBox(IWiredItem Item)
        {
            return _wiredItems.TryAdd(Item.Item.Id, Item);
        }

        public bool TryRemove(int ItemId)
        {
            IWiredItem Item = null;
            return _wiredItems.TryRemove(ItemId, out Item);
        }

        public bool TryGet(int id, out IWiredItem Item)
        {
            return _wiredItems.TryGetValue(id, out Item);
        }

        public void Cleanup()
        {
            _wiredItems.Clear();
        }
    }
}
