namespace Plus.Game.Items.Interactor
{
    using Players;
    using Rooms;
    using Rooms.Games.Teams;

    internal class InteractorFreezeTile : IFurniInteractor
    {
        public void OnPlace(Player Session, Item Item)
        {
        }

        public void OnRemove(Player Session, Item Item)
        {
        }

        public void OnTrigger(Player Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null || !Session.GetHabbo().InRoom || Item == null || Item.InteractingUser > 0)
            {
                return;
            }

            var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            if (User.Team != Team.None)
            {
                User.FreezeInteracting = true;
                Item.InteractingUser = Session.GetHabbo().Id;

                if (Item.Data.InteractionType == InteractionType.FREEZE_TILE_BLOCK)
                {
                    if (Gamemap.TileDistance(User.X, User.Y, Item.GetX, Item.GetY) < 2)
                    {
                        Item.GetRoom().GetFreeze().onFreezeTiles(Item, Item.freezePowerUp);
                    }
                }
            }
        }

        public void OnWiredTrigger(Item Item)
        {

        }
    }
}