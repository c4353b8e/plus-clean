namespace Plus.Game.Items.Interactor
{
    using System;
    using System.Drawing;
    using Communication.Packets.Outgoing.Rooms.Furni.LoveLocks;
    using Players;
    using Rooms;

    public class InteractorLoveLock : IFurniInteractor
    {
        public void OnPlace(Player Session, Item Item)
        {
        }

        public void OnRemove(Player Session, Item Item)
        {
        }

        public void OnTrigger(Player Session, Item Item, int Request, bool HasRights)
        {
            RoomUser User = null;

            if (Session != null)
            {
                User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            }

            if (User == null)
            {
                return;
            }

            if (Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
            {
                if (Item.ExtraData == null || Item.ExtraData.Length <= 1 || !Item.ExtraData.Contains(Convert.ToChar(5).ToString()))
                {
                    Point pointOne;
                    Point pointTwo;

                    switch (Item.Rotation)
                    {
                        case 2:
                            pointOne = new Point(Item.GetX, Item.GetY + 1);
                            pointTwo = new Point(Item.GetX, Item.GetY - 1);
                            break;

                        case 4:
                            pointOne = new Point(Item.GetX - 1, Item.GetY);
                            pointTwo = new Point(Item.GetX + 1, Item.GetY);
                            break;

                        default:
                            return;
                    }

                    var UserOne = Item.GetRoom().GetRoomUserManager().GetUserForSquare(pointOne.X, pointOne.Y);
                    var UserTwo = Item.GetRoom().GetRoomUserManager().GetUserForSquare(pointTwo.X, pointTwo.Y);

                    if(UserOne == null || UserTwo == null)
                    {
                        Session.SendNotification("We couldn't find a valid user to lock this love lock with.");
                    }
                    else if(UserOne.GetClient() == null || UserTwo.GetClient() == null)
                    {
                        Session.SendNotification("We couldn't find a valid user to lock this love lock with.");
                    }
                    else if(UserOne.HabboId != Item.UserID && UserTwo.HabboId != Item.UserID)
                    {
                        Session.SendNotification("You can only use this item with the item owner.");
                    }
                    else
                    {
                        UserOne.CanWalk = false;
                        UserTwo.CanWalk = false;

                        Item.InteractingUser = UserOne.GetClient().GetHabbo().Id;
                        Item.InteractingUser2 = UserTwo.GetClient().GetHabbo().Id;

                        UserOne.GetClient().SendPacket(new LoveLockDialogueMessageComposer(Item.Id));
                        UserTwo.GetClient().SendPacket(new LoveLockDialogueMessageComposer(Item.Id));
                    }


                }
            }
            else
            {
                User.MoveTo(Item.SquareInFront);
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}