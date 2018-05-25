namespace Plus.Game.Items.Interactor
{
    using Players;

    public interface IFurniInteractor
    {
        void OnPlace(Player session, Item item);
        void OnRemove(Player session, Item item);
        void OnTrigger(Player session, Item item, int request, bool hasRights);
        void OnWiredTrigger(Item item);
    }
}