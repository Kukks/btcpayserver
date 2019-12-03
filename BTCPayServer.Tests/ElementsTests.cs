using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Controllers;
using BTCPayServer.Models.WalletViewModels;
using BTCPayServer.Tests.Logging;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using NBitcoin.RPC;
using NBitpayClient;
using Xunit;
using Xunit.Abstractions;

namespace BTCPayServer.Tests
{
    public class ElementsTests
    {
        
        public const int TestTimeout = 60_000;
        public ElementsTests(ITestOutputHelper helper)
        {
            Logs.Tester = new XUnitLog(helper) { Name = "Tests" };
            Logs.LogProvider = new XUnitLogProvider(helper);
        }
        
        [Fact]
        public async Task OnlyShowSupportedWallets()
        {
            using (var tester = ServerTester.Create())
            {
                await tester.StartAsync();
                await tester.EnsureChannelsSetup();
                var user = tester.NewAccount();
                user.GrantAccess();
                user.RegisterDerivationScheme("LBTC");
                user.RegisterDerivationScheme("BTC");
                user.RegisterDerivationScheme("USDT");
                
                Assert.Single(Assert.IsType<ListWalletsViewModel>(Assert.IsType<ViewResult>(await user.GetController<WalletsController>().ListWallets()).Model).Wallets);
            }
        }

        [Fact]
        public async Task ElementsAssetsAreHandledCorrectly()
        {
            using (var tester = ServerTester.Create())
            {
                await tester.StartAsync();
                var user = tester.NewAccount();
                user.GrantAccess();
                user.RegisterDerivationScheme("LBTC");
                user.RegisterDerivationScheme("USDT");
                
                //no tether on our regtest, lets create it and set it
                var tether = tester.NetworkProvider.GetNetwork<ElementsBTCPayNetwork>("USDT");
                var lbtc = tester.NetworkProvider.GetNetwork<ElementsBTCPayNetwork>("LBTC");
                var issueAssetResult = await tester.LBTCExplorerNode.SendCommandAsync("issueasset", 100000, 0);
                tether.AssetId = uint256.Parse(issueAssetResult.Result["asset"].ToString());
//               
//                await tester.LBTCExplorerNode.GenerateAsync(10 );
                
                Assert.Equal(tether.AssetId,  tester.NetworkProvider.GetNetwork<ElementsBTCPayNetwork>("USDT").AssetId);
                //test: register 2 assets on the same elements network and make sure paying an invoice on one does not affect the other in any way
                var invoice = await user.BitPay.CreateInvoiceAsync(new Invoice(0.1m, "BTC"));
                Assert.Equal(2, invoice.SupportedTransactionCurrencies.Count);
                var ci = invoice.CryptoInfo.Single(info => info.CryptoCode.Equals("LBTC"));
                //1 lbtc = 1 btc
                Assert.Equal(1, ci.Rate);
                var star = await tester.LBTCExplorerNode.SendCommandAsync("sendtoaddress", ci.Address,ci.Due, "", "", false, true,
                    1, "UNSET", lbtc.AssetId);
                
                TestUtils.Eventually(() =>
                {
                    var localInvoice = user.BitPay.GetInvoice(invoice.Id, Facade.Merchant);
                    Assert.Equal("paid", localInvoice.Status);
                    Assert.Single(localInvoice.CryptoInfo.Single(info => info.CryptoCode.Equals("LBTC")).Payments);
                });
                
                invoice = await user.BitPay.CreateInvoiceAsync(new Invoice(0.1m, "BTC"));
                
                ci = invoice.CryptoInfo.Single(info => info.CryptoCode.Equals("USDT"));
                Assert.Equal(2, invoice.SupportedTransactionCurrencies.Count);
                star = await tester.LBTCExplorerNode.SendCommandAsync("sendtoaddress", ci.Address, ci.Due, "", "", false, true,
                    1, "UNSET", tether.AssetId);
                
                TestUtils.Eventually(() =>
                {
                    var localInvoice = user.BitPay.GetInvoice(invoice.Id, Facade.Merchant);
                    Assert.Equal("paid", localInvoice.Status);
                    Assert.Single(localInvoice.CryptoInfo.Single(info => info.CryptoCode.Equals("LBTC")).Payments);
                });

            }
        }
    }
}
