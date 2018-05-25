namespace Plus.Game.Items.Interactor
{
    using Players;

    public class InteractorScoreboard : IFurniInteractor
    {
        public void OnPlace(Player Session, Item Item)
        {
        }

        public void OnRemove(Player Session, Item Item)
        {
        }

        public void OnTrigger(Player Session, Item Item, int Request, bool HasRights)
        {
            if (!HasRights)
            {
                return;
            }

            var OldValue = 0;

            if (!int.TryParse(Item.ExtraData, out OldValue))
            {
            }


            if (Request == 1)
            {
                if (Item.pendingReset && OldValue > 0)
                {
                    OldValue = 0;
                    Item.pendingReset = false;
                }
                else
                {
                    OldValue = OldValue + 60;
                    Item.UpdateNeeded = false;
                }
            }
            else if (Request == 2)
            {
                Item.UpdateNeeded = !Item.UpdateNeeded;
                Item.pendingReset = true;
            }


            Item.ExtraData = OldValue.ToString();
            Item.UpdateState();
        }

        public void OnWiredTrigger(Item Item)
        {
            var OldValue = 0;

            if (!int.TryParse(Item.ExtraData, out OldValue))
            {
            }

            OldValue = OldValue + 60;
            Item.UpdateNeeded = false;

            Item.ExtraData = OldValue.ToString();
            Item.UpdateState();
        }
    }
}