using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Abstractions.Extensions;
using BTCPayServer.Abstractions.Models;
using BTCPayServer.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using NBXplorer;


namespace BTCPayServer.Controllers
{
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie)]
    [Authorize(Policy = Policies.CanModifyStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Cookie)]
    [AutoValidateAntiforgeryToken]
    public class StoreLiquidController : Controller
    {
        private readonly BTCPayNetworkProvider _btcPayNetworkProvider;
        private readonly ExplorerClientProvider _explorerClientProvider;

        public StoreLiquidController(BTCPayNetworkProvider btcPayNetworkProvider,
            ExplorerClientProvider explorerClientProvider)
        {
            _btcPayNetworkProvider = btcPayNetworkProvider;
            _explorerClientProvider = explorerClientProvider;
        }

        [HttpGet("stores/{storeId}/liquid")]
        public async Task<IActionResult> GenerateLiquidScript(Dictionary<string, BitcoinExtKey> bitcoinExtKeys = null)
        {
            Dictionary<string, string> generated = new Dictionary<string, string>();
            var allNetworks = _btcPayNetworkProvider.GetAll().OfType<ElementsBTCPayNetwork>()
                .GroupBy(network => network.NetworkCryptoCode);
            var allNetworkCodes = allNetworks
                .SelectMany(networks => networks.Select(network => network.CryptoCode.ToUpperInvariant()))
                .ToArray();
            Dictionary<string, BitcoinExtKey> privKeys = bitcoinExtKeys ?? new Dictionary<string, BitcoinExtKey>();

            var derivationSchemes = CurrentStore.GetSupportedPaymentMethods(_btcPayNetworkProvider)
                .OfType<DerivationSchemeSettings>()
                .Where(settings => allNetworkCodes.Contains(settings.PaymentId.CryptoCode)).ToList();

            if (derivationSchemes.Any() is false)
            {
                TempData.SetStatusMessageModel(new StatusMessageModel()
                {
                    Severity = StatusMessageModel.StatusSeverity.Info,
                    Message = "There are no wallets configured that use Liquid or an elements side-chain."
                });
                return View(new GenerateLiquidImportScripts());
            }

            foreach (var der in derivationSchemes.Where(settings => settings.IsHotWallet))
            {
                if (!privKeys.TryGetValue(der.Network.CryptoCode, out var key))
                {
                    var explorerClient = _explorerClientProvider.GetExplorerClient(der.Network);
                    key = await explorerClient.GetMetadataAsync<BitcoinExtKey>(der.AccountDerivation,
                        WellknownMetadataKeys.AccountHDKey);
                    if (key is null)
                        continue;
                }

                var sharedWallet = derivationSchemes.Where(settings =>
                    (der.Network as ElementsBTCPayNetwork).NetworkCryptoCode ==
                    (settings.Network as ElementsBTCPayNetwork).NetworkCryptoCode &&
                    settings.AccountDerivation.ToString() == der.AccountDerivation.ToString());
                foreach (DerivationSchemeSettings derivationSchemeSettings in sharedWallet)
                {
                    privKeys.TryAdd(derivationSchemeSettings.PaymentId.CryptoCode, key);
                }
            }


            foreach (var networkSet in allNetworks)
            {
                var sb = new StringBuilder();
                var explorerClient = _explorerClientProvider.GetExplorerClient(networkSet.Key);
                foreach (var sub in networkSet)
                {
                    var nbxnet = sub.NBXplorerNetwork as NBXplorerNetworkProvider.LiquidNBXplorerNetwork;
                    var deriv = CurrentStore.GetDerivationSchemeSettings(_btcPayNetworkProvider, sub.CryptoCode);
                    if (deriv is null)
                    {
                        continue;
                    }

                    var utxos = await explorerClient.GetUTXOsAsync(deriv.AccountDerivation, CancellationToken.None);
                    privKeys.TryGetValue(sub.CryptoCode, out var privKey);
                    foreach (var utxo in utxos.GetUnspentUTXOs())
                    {
                        var addr = nbxnet.CreateAddress(deriv.AccountDerivation, utxo.KeyPath, utxo.ScriptPubKey);

                        if (privKey is null)
                        {
                            sb.AppendLine(
                                $"elements-cli importaddress \"{addr}\" \"{utxo.KeyPath} from {deriv.AccountDerivation}\" true");
                        }
                        else
                        {
                            sb.AppendLine(
                                $"elements-cli importprivkey \"{privKey.Derive(utxo.KeyPath).PrivateKey.GetWif(nbxnet.NBitcoinNetwork)}\" \"{utxo.KeyPath} from {deriv.AccountDerivation}\" true");
                        }

                        if (!deriv.AccountDerivation.Unblinded())
                        {
                            var blindingKey =
                                NBXplorerNetworkProvider.LiquidNBXplorerNetwork.GenerateBlindingKey(
                                    deriv.AccountDerivation, utxo.KeyPath);
                            sb.AppendLine($"elements-cli importblindingkey {addr} {blindingKey.ToHex()}");
                        }
                    }
                }

                generated.Add(networkSet.Key, sb.ToString());
            }

            return View(new GenerateLiquidImportScripts()
            {
                Wallets = derivationSchemes.Select(settings =>
                    new GenerateLiquidImportScripts.GenerateLiquidImportScriptWalletKeyVm()
                    {
                        CryptoCode = settings.PaymentId.CryptoCode,
                        KeyPresent = privKeys.ContainsKey(settings.PaymentId.CryptoCode),
                        ManualKey = null
                    }).ToArray(),
                Scripts = generated
            });
        }


        [HttpPost("stores/{storeId}/liquid")]
        public async Task<IActionResult> GenerateLiquidScript(GenerateLiquidImportScripts vm)
        {
            Dictionary<string, BitcoinExtKey> privKeys = new Dictionary<string, BitcoinExtKey>();
            for (var index = 0; index < vm.Wallets.Length; index++)
            {
                var wallet = vm.Wallets[index];
                if (string.IsNullOrEmpty(wallet.ManualKey))
                    continue;

                var n =
                    _btcPayNetworkProvider.GetNetwork<ElementsBTCPayNetwork>(wallet.CryptoCode);
                ExtKey extKey = null;
                try
                {
                    var mnemonic = new Mnemonic(wallet.ManualKey);
                    extKey = mnemonic.DeriveExtKey();
                }
                catch (Exception)
                {
                }

                if (extKey == null)
                {
                    try
                    {
                        extKey = ExtKey.Parse(wallet.ManualKey, n.NBitcoinNetwork);
                    }
                    catch (Exception)
                    {
                    }
                }

                if (extKey == null)
                {
                    vm.AddModelError(scripts => scripts.Wallets[index].ManualKey,
                        "Invalid key (must be seed or root xprv or account xprv)", this);
                    continue;
                }

                var der = CurrentStore.GetDerivationSchemeSettings(_btcPayNetworkProvider, wallet.CryptoCode);
                if (der.AccountDerivation.GetExtPubKeys().Count() > 1)
                {
                    vm.AddModelError(scripts => scripts.Wallets[index].ManualKey, "cannot handle multsig", this);
                    continue;
                }

                var first = der
                    .AccountDerivation
                    .GetExtPubKeys().First();
                if (first != extKey.Neuter())
                {
                    KeyPath kp = null;
                    switch (der.AccountDerivation.ScriptPubKeyType())
                    {
                        case ScriptPubKeyType.Legacy:
                            kp = new KeyPath($"m/44'/{n.CoinType}/0'");
                            break;
                        case ScriptPubKeyType.Segwit:

                            kp = new KeyPath($"m/84'/{n.CoinType}/0'");
                            break;
                        case ScriptPubKeyType.SegwitP2SH:
                            kp = new KeyPath($"m/49'/{n.CoinType}/0'");
                            break;
                        default:
                            vm.AddModelError(scripts => scripts.Wallets[index].ManualKey, "cannot handle wallet type",
                                this);
                            continue;
                    }

                    extKey = extKey.Derive(kp);
                    if (first != extKey.Neuter())
                    {
                        vm.AddModelError(scripts => scripts.Wallets[index].ManualKey, "key did not match", this);
                        continue;
                    }
                }

                privKeys.TryAdd(der.PaymentId.CryptoCode, extKey.GetWif(n.NBitcoinNetwork));
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            return await GenerateLiquidScript(privKeys);
        }

        public StoreData CurrentStore
        {
            get
            {
                return this.HttpContext.GetStoreData();
            }
        }

        public class GenerateLiquidImportScripts
        {
            public class GenerateLiquidImportScriptWalletKeyVm
            {
                public string CryptoCode { get; set; }
                public bool KeyPresent { get; set; }
                public string ManualKey { get; set; }
            }

            public GenerateLiquidImportScriptWalletKeyVm[] Wallets { get; set; } =
                Array.Empty<GenerateLiquidImportScriptWalletKeyVm>();

            public Dictionary<string, string> Scripts { get; set; } = new Dictionary<string, string>();
        }
    }
}
