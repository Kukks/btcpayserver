using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Abstractions.Models;
using BTCPayServer.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BTCPayServer.Plugins.FixedFloat
{
    public class FixedFloatPlugin : BaseBTCPayServerPlugin
    {
        public const string StoreBlobKey = "FixedFloatSettings";
        public override string Identifier => "BTCPayServer.Plugins.FixedFloat";
        public override string Name => "FixedFloat";


        public override IBTCPayServerPlugin.PluginDependency[] Dependencies { get; } =
        {
            new IBTCPayServerPlugin.PluginDependency() { Identifier = nameof(BTCPayServer), Condition = ">=1.4.6.0" }
        };

        public override string Description =>
            "Allows you to embed a FixedFloat conversion screen to allow customers to pay with altcoins.";

        public override void Execute(IServiceCollection applicationBuilder)
        {
            applicationBuilder.AddSingleton<FixedFloatService>();
            applicationBuilder.AddSingleton<IUIExtension>(new UIExtension("FixedFloat/StoreIntegrationFixedFloatOption",
                "store-integrations-list"));
            applicationBuilder.AddSingleton<IUIExtension>(new UIExtension("FixedFloat/CheckoutContentExtension",
                "checkout-bitcoin-post-content"));
            applicationBuilder.AddSingleton<IUIExtension>(new UIExtension("FixedFloat/CheckoutContentExtension",
                "checkout-lightning-post-content"));
            applicationBuilder.AddSingleton<IUIExtension>(new UIExtension("FixedFloat/CheckoutTabExtension",
                "checkout-bitcoin-post-tabs"));
            applicationBuilder.AddSingleton<IUIExtension>(new UIExtension("FixedFloat/CheckoutTabExtension",
                "checkout-lightning-post-tabs"));
            applicationBuilder.AddSingleton<IUIExtension>(new UIExtension("FixedFloat/CheckoutEnd",
                "checkout-end"));
            applicationBuilder.AddSingleton<IUIExtension>(new UIExtension("FixedFloat/FixedFloatNav",
                "store-integrations-nav"));
            base.Execute(applicationBuilder);
        }
    }
}
