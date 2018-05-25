namespace Plus.Communication.Rcon.Commands.Hotel
{
    using Packets.Outgoing.Catalog;

    internal class ReloadCatalogCommand : IRconCommand
    {
        public string Description => "This command is used to reload the catalog.";

        public string Parameters => "";

        public bool TryExecute(string[] parameters)
        {
            Program.GameContext.GetCatalog().Init(Program.GameContext.GetItemManager());
            Program.GameContext.PlayerController.SendPacket(new CatalogUpdatedComposer());
            return true;
        }
    }
}