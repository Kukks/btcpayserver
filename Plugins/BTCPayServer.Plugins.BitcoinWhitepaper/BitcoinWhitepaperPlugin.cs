using BTCPayServer.Abstractions.Models;

namespace BTCPayServer.Plugins.BitcoinWhitepaper
{
    public class BitcoinWhitepaperPlugin: BaseBTCPayServerPlugin
    {
        public  override string Identifier { get; } = "BTCPayServer.Plugins.BitcoinWhitepaper";
        public  override string Name { get; } = "Bitcoin Whitepaper";
        public  override string Description { get; } = "This makes the Bitcoin whitepaper available on your BTCPay Server.";
    }
}
