namespace Plus.Game.Items.Interactor
{
    using System;
    using Players;
    using Rooms.Games.Teams;

    public class InteractorBanzaiScoreCounter : IFurniInteractor
    {
        public void OnPlace(Player Session, Item Item)
        {
            if (Item.team == Team.None)
            {
                return;
            }

            Item.ExtraData = Item.GetRoom().GetGameManager().Points[Convert.ToInt32(Item.team)].ToString();
            Item.UpdateState(false, true);
        }

        public void OnRemove(Player Session, Item Item)
        {
        }

        public void OnTrigger(Player Session, Item Item, int Request, bool HasRights)
        {
            if (HasRights)
            {
                Item.GetRoom().GetGameManager().Points[Convert.ToInt32(Item.team)] = 0;

                Item.ExtraData = "0";
                Item.UpdateState();
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}