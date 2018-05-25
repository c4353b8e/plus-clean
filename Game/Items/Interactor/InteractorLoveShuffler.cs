namespace Plus.Game.Items.Interactor
{
    using Players;

    public class InteractorLoveShuffler : IFurniInteractor
    {
        public void OnPlace(Player Session, Item Item)
        {
            Item.ExtraData = "-1";
            Item.UpdateNeeded = true;
        }

        public void OnRemove(Player Session, Item Item)
        {
            Item.ExtraData = "-1";
        }

        public void OnTrigger(Player Session, Item Item, int Request, bool HasRights)
        {
            if (!HasRights)
            {
                return;
            }

            if (Item.ExtraData != "0")
            {
                Item.ExtraData = "0";
                Item.UpdateState(false, true);
                Item.RequestUpdate(10, true);
            }
        }

        public void OnWiredTrigger(Item Item)
        {
            if (Item.ExtraData != "0")
            {
                Item.ExtraData = "0";
                Item.UpdateState(false, true);
                Item.RequestUpdate(10, true);
            }
        }
    }
}