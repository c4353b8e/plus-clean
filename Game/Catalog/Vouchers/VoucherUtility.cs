namespace Plus.Game.Catalog.Vouchers
{
    public static class VoucherUtility
    {
        public static VoucherType GetType(string type)
        {
            switch (type)
            {
                default:
                    return VoucherType.Credit;
                case "ducket":
                    return VoucherType.Ducket;
            }
        }
        
    }
}
