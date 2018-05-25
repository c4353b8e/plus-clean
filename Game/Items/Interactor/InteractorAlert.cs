namespace Plus.Game.Items.Interactor
{
    using Players;

    public class InteractorAlert : IFurniInteractor
    {
        public void OnPlace(Player session, Item item)
        {
            item.ExtraData = "0";
            item.UpdateNeeded = true;
        }

        public void OnRemove(Player session, Item item)
        {
            item.ExtraData = "0";
        }

        public void OnTrigger(Player session, Item item, int request, bool hasRights)
        {
            if (!hasRights)
            {
                return;
            }

            if (item.ExtraData != "0")
            {
                return;
            }

            item.ExtraData = "1";
            item.UpdateState(false, true);
            item.RequestUpdate(4, true);
        }

        public void OnWiredTrigger(Item item)
        {
            if (item.ExtraData == "0")
            {
                item.ExtraData = "1";
                item.UpdateState(false, true);
                item.RequestUpdate(4, true);
            }
        }
    }
}