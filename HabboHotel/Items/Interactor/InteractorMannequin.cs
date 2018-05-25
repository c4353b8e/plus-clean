namespace Plus.HabboHotel.Items.Interactor
{
    using System;
    using System.Collections.Generic;
    using Communication.Packets.Outgoing.Rooms.Engine;

    internal class InteractorMannequin : IFurniInteractor
    {
        public void OnPlace(GameClients.GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClients.GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClients.GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Item.ExtraData.Contains(Convert.ToChar(5).ToString()))
            {
                var Stuff = Item.ExtraData.Split(Convert.ToChar(5));
                Session.GetHabbo().Gender = Stuff[0].ToUpper();
                var NewFig = new Dictionary<string, string>();
                NewFig.Clear();
                foreach (var Man in Stuff[1].Split('.'))
                {
                    foreach (var Fig in Session.GetHabbo().Look.Split('.'))
                    {
                        if (Fig.Split('-')[0] == Man.Split('-')[0])
                        {
                            if (NewFig.ContainsKey(Fig.Split('-')[0]) && !NewFig.ContainsValue(Man))
                            {
                                NewFig.Remove(Fig.Split('-')[0]);
                                NewFig.Add(Fig.Split('-')[0], Man);
                            }
                            else if (!NewFig.ContainsKey(Fig.Split('-')[0]) && !NewFig.ContainsValue(Man))
                            {
                                NewFig.Add(Fig.Split('-')[0], Man);
                            }
                        }
                        else
                        {
                            if (!NewFig.ContainsKey(Fig.Split('-')[0]))
                            {
                                NewFig.Add(Fig.Split('-')[0], Fig);
                            }
                        }
                    }
                }

                var Final = "";
                foreach (var Str in NewFig.Values)
                {
                    Final += Str + ".";
                }


                Session.GetHabbo().Look = Final.TrimEnd('.');

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE users SET look = @look, gender = @gender WHERE id = '" + Session.GetHabbo().Id + "' LIMIT 1");
                    dbClient.AddParameter("look", Session.GetHabbo().Look);
                    dbClient.AddParameter("gender", Session.GetHabbo().Gender);
                    dbClient.RunQuery();
                }

                var Room = Session.GetHabbo().CurrentRoom;
                if (Room != null)
                {
                    var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);
                    if (User != null)
                    {
                        Session.SendPacket(new UserChangeComposer(User, true));
                        Session.GetHabbo().CurrentRoom.SendPacket(new UserChangeComposer(User, false));
                    }
                }
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}
