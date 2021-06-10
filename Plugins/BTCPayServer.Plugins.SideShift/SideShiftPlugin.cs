using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Abstractions.Models;
using BTCPayServer.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BTCPayServer.Plugins.SideShift
{
    public class SideShiftPlugin : BaseBTCPayServerPlugin
    {
        public const string StoreBlobKey = "SideShiftSettings";
        public override string Identifier => "BTCPayServer.Plugins.SideShift";
        public override string Name => "SideShift";

        public override string Description =>
            "Allows you to embed a SideShift conversion screen to allow customers to pay with altcoins.";

        public override void Execute(IServiceCollection applicationBuilder)
        {
            applicationBuilder.AddSingleton<SideShiftService>();
            applicationBuilder.AddSingleton<IUIExtension>(new UIExtension("SideShift/StoreIntegrationSideShiftOption",
                "store-integrations-list"));
            applicationBuilder.AddSingleton<IUIExtension>(new UIExtension("SideShift/CheckoutContentExtension",
                "checkout-bitcoin-post-content"));
            applicationBuilder.AddSingleton<IUIExtension>(new UIExtension("SideShift/CheckoutContentExtension",
                "checkout-ethereum-post-content"));
            applicationBuilder.AddSingleton<IUIExtension>(new UIExtension("SideShift/CheckoutTabExtension",
                "checkout-bitcoin-post-tabs"));
            applicationBuilder.AddSingleton<IUIExtension>(new UIExtension("SideShift/CheckoutTabExtension",
                "checkout-ethereum-post-tabs"));
            applicationBuilder.AddSingleton<IUIExtension>(new UIExtension("SideShift/CheckoutEnd",
                "checkout-end"));
            base.Execute(applicationBuilder);
        }
    }
}
