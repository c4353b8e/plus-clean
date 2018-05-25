namespace Plus.Communication.Packets.Incoming.Inventory.Pets
{
    using Game.Players;
    using Outgoing.Inventory.Pets;

    internal class GetPetInventoryEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session.GetHabbo().GetInventoryComponent() == null)
            {
                return;
            }

            var pets = session.GetHabbo().GetInventoryComponent().GetPets();
            session.SendPacket(new PetInventoryComposer(pets));
        }
    }
}