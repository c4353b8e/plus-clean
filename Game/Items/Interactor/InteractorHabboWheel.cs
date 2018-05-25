namespace Plus.Game.Items.Interactor
{
    using Players;

    public class InteractorHabboWheel : IFurniInteractor
    {
        public void OnPlace(Player Session, Item Item)
        {
            Item.ExtraData = "-1";
            Item.RequestUpdate(10, true);
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

            if (Item.ExtraData != "-1")
            {
                Item.ExtraData = "-1";
                Item.UpdateState();
                Item.RequestUpdate(10, true);
            }
        }

        public void OnWiredTrigger(Item Item)
        {
            if (Item.ExtraData != "-1")
            {
                Item.ExtraData = "-1";
                Item.UpdateState();
                Item.RequestUpdate(10, true);
            }
        }
    }
}