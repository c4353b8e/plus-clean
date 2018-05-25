namespace Plus.Game.Items.Interactor
{
    using Players;
    using Quests;

    public class InteractorGenericSwitch : IFurniInteractor
    {
        public void OnPlace(Player Session, Item Item)
        {
        }

        public void OnRemove(Player Session, Item Item)
        {
        }

        public void OnTrigger(Player Session, Item Item, int Request, bool HasRights)
        {
            var Modes = Item.GetBaseItem().Modes - 1;

            if (Session == null || !HasRights || Modes <= 0)
            {
                return;
            }

            Program.GameContext.GetQuestManager().ProgressUserQuest(Session, QuestType.FurniSwitch);

            var CurrentMode = 0;
            var NewMode = 0;

            if (!int.TryParse(Item.ExtraData, out CurrentMode))
            {
            }

            if (CurrentMode <= 0)
            {
                NewMode = 1;
            }
            else if (CurrentMode >= Modes)
            {
                NewMode = 0;
            }
            else
            {
                NewMode = CurrentMode + 1;
            }

            Item.ExtraData = NewMode.ToString();
            Item.UpdateState();
        }

        public void OnWiredTrigger(Item Item)
        {
            var Modes = Item.GetBaseItem().Modes - 1;

            if (Modes == 0)
            {
                return;
            }

            var CurrentMode = 0;
            var NewMode = 0;

            if (string.IsNullOrEmpty(Item.ExtraData))
            {
                Item.ExtraData = "0";
            }

            if (!int.TryParse(Item.ExtraData, out CurrentMode))
            {
                return;
            }

            if (CurrentMode <= 0)
            {
                NewMode = 1;
            }
            else if (CurrentMode >= Modes)
            {
                NewMode = 0;
            }
            else
            {
                NewMode = CurrentMode + 1;
            }

            Item.ExtraData = NewMode.ToString();
            Item.UpdateState();
        }
    }
}