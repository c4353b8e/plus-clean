﻿namespace Plus.HabboHotel.Rooms.Games
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using Items;

    public class GameItemHandler
    {
        private Room _room;
        private Random rnd;
        private ConcurrentDictionary<int, Item> _banzaiPyramids;
        private ConcurrentDictionary<int, Item> _banzaiTeleports;

        public GameItemHandler(Room room)
        {
            _room = room;
            rnd = new Random();

            _banzaiPyramids = new ConcurrentDictionary<int, Item>();
            _banzaiTeleports = new ConcurrentDictionary<int, Item>();
        }

        public void OnCycle()
        {
            CyclePyramids();
        }

        private void CyclePyramids()
        {
            var rnd = new Random();

            foreach (var item in _banzaiPyramids.Values.ToList())
            {
                if (item == null)
                {
                    continue;
                }

                if (item.interactionCountHelper == 0 && item.ExtraData == "1")
                {
                    _room.GetGameMap().RemoveFromMap(item, false);
                    item.interactionCountHelper = 1;
                }

                if (string.IsNullOrEmpty(item.ExtraData))
                {
                    item.ExtraData = "0";
                }

                var randomNumber = rnd.Next(0, 30);
                if (randomNumber == 15)
                {
                    if (item.ExtraData == "0")
                    {
                        item.ExtraData = "1";
                        item.UpdateState();
                        _room.GetGameMap().RemoveFromMap(item, false);
                    }
                    else
                    {
                        if (_room.GetGameMap().ItemCanBePlaced(item.GetX, item.GetY))
                        {
                            item.ExtraData = "0";
                            item.UpdateState();
                            _room.GetGameMap().AddItemToMap(item);
                        }
                    }
                }
            }
        }

        public void AddPyramid(Item item, int itemId)
        {
            if (_banzaiPyramids.ContainsKey(itemId))
            {
                _banzaiPyramids[itemId] = item;
            }
            else
            {
                _banzaiPyramids.TryAdd(itemId, item);
            }
        }

        public void RemovePyramid(int itemId)
        {
            _banzaiPyramids.TryRemove(itemId, out var Item);
        }

        public void AddTeleport(Item item, int itemId)
        {
            if (_banzaiTeleports.ContainsKey(itemId))
            {
                _banzaiTeleports[itemId] = item;
            }
            else
            {
                _banzaiTeleports.TryAdd(itemId, item);
            }
        }

        public void RemoveTeleport(int itemId)
        {
            _banzaiTeleports.TryRemove(itemId, out var Item);
        }

        public void OnTeleportRoomUserEnter(RoomUser user, Item item)
        {
            var items = _banzaiTeleports.Values.Where(p => p.Id != item.Id);

            var count = items.Count();

            var countID = rnd.Next(0, count);
            var countAmount = 0;

            if (count == 0)
            {
                return;
            }

            foreach (var i in items.ToList())
            {
                if (i == null)
                {
                    continue;
                }

                if (countAmount == countID)
                {
                    i.ExtraData = "1";
                    i.UpdateNeeded = true;

                    _room.GetGameMap().TeleportToItem(user, item);

                    i.ExtraData = "1";
                    i.UpdateNeeded = true;
                    i.UpdateState();
                    i.UpdateState();
                }

                countAmount++;
            }
        }

        public void Dispose()
        {
            if (_banzaiTeleports != null)
            {
                _banzaiTeleports.Clear();
            }

            if (_banzaiPyramids != null)
            {
                _banzaiPyramids.Clear();
            }

            _banzaiPyramids = null;
            _banzaiTeleports = null;
            _room = null;
            rnd = null;
        }
    }
}