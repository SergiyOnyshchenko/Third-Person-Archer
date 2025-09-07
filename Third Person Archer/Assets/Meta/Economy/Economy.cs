namespace Meta.Economy
{
    /// <summary>
    /// Tiny static facade to avoid global singletons while keeping usage ergonomic.
    /// </summary>
    public static class Economy
    {
        public static IWallet Wallet { get; private set; }

        internal static void Initialize(IWallet wallet) => Wallet = wallet;
    }
}