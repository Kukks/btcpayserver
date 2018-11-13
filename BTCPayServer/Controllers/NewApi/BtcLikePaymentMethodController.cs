﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Data;
using BTCPayServer.Payments;
using BTCPayServer.Security;
using BTCPayServer.Services.Wallets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NBXplorer.DerivationStrategy;

namespace BTCPayServer.Controllers.NewApi
{
    [ApiController]
    [IncludeInOpenApiDocs]
    [Route("api/v0.1/stores/{storeId}/PaymentMethods/" + nameof(PaymentTypes.BTCLike))]
    [Authorize(Policy = Policies.CanModifyStoreSettings.Key)]
    [Authorize()]
    public class BtcLikePaymentMethodController : ControllerBase
    {
        private StoreData Store => HttpContext.GetStoreData();
        private readonly BTCPayNetworkProvider _btcPayNetworkProvider;
        private readonly ExplorerClientProvider _explorerClientProvider;
        private readonly BTCPayWalletProvider _walletProvider;

        public BtcLikePaymentMethodController(BTCPayNetworkProvider btcPayNetworkProvider,
            ExplorerClientProvider explorerClientProvider, BTCPayWalletProvider walletProvider)
        {
            _btcPayNetworkProvider = btcPayNetworkProvider;
            _explorerClientProvider = explorerClientProvider;
            _walletProvider = walletProvider;
        }

        [HttpGet("")]
        public ActionResult<IEnumerable<BtcLikePaymentMethod>> GetBtcLikePaymentMethods(
            [FromQuery] bool enabledOnly = false)
        {
            var excludedPaymentMethods = Store.GetStoreBlob().GetExcludedPaymentMethods();
            return Ok(Store.GetSupportedPaymentMethods(_btcPayNetworkProvider)
                .Where((method) => method.PaymentId.PaymentType == PaymentTypes.BTCLike)
                .OfType<DerivationStrategy>()
                .Select(strategy =>
                    new BtcLikePaymentMethod(strategy, !excludedPaymentMethods.Match(strategy.PaymentId)))
                .Where((result) => !enabledOnly || result.Enabled)
                .ToList()
            );
        }

        [HttpGet("{cryptoCode}")]
        public ActionResult<BtcLikePaymentMethod> GetBtcLikePaymentMethod(string cryptoCode)
        {
            if (!GetCryptoCodeWallet(cryptoCode, out var network, out var wallet))
            {
                return NotFound();
            }

            return Ok(GetExistingBtcLikePaymentMethod(cryptoCode));
        }

        [HttpGet("{cryptoCode}/preview")]
        public ActionResult<BtcLikePaymentMethodPreviewResult> GetBtcLikePaymentAddressPreview(string cryptoCode,
            int offset = 0, int amount = 10)
        {
            if (!GetCryptoCodeWallet(cryptoCode, out var network, out var wallet))
            {
                return NotFound();
            }

            var paymentMethod = GetExistingBtcLikePaymentMethod(cryptoCode);
            if (string.IsNullOrEmpty(paymentMethod.DerivationScheme))
            {
                return BadRequest();
            }

            var strategy = ParseDerivationStrategy(cryptoCode, network);

            var line = strategy.DerivationStrategyBase.GetLineFor(DerivationFeature.Deposit);
            var result = new BtcLikePaymentMethodPreviewResult();
            for (var i = offset; i < amount; i++)
            {
                var address = line.Derive((uint)i);
                result.Addresses.Add(
                    new BtcLikePaymentMethodPreviewResult.BtcLikePaymentMethodPreviewResultAddressItem()
                    {
                        KeyPath = DerivationStrategyBase.GetKeyPath(DerivationFeature.Deposit).Derive((uint)i)
                            .ToString(),
                        Address = address.ScriptPubKey.GetDestinationAddress(strategy.Network.NBitcoinNetwork)
                            .ToString()
                    });
            }

            return Ok(result);
        }

        [HttpPost("{cryptoCode}/preview")]
        public ActionResult<BtcLikePaymentMethodPreviewResult> GetBtcLikePaymentAddressPreview(string cryptoCode,
            [FromBody] BtcLikePaymentMethod paymentMethod,
            int offset = 0, int amount = 10)
        {
            if (!GetCryptoCodeWallet(cryptoCode, out var network, out var wallet))
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(paymentMethod.DerivationScheme))
            {
                return BadRequest();
            }

            try
            {
                var strategy = ParseDerivationStrategy(cryptoCode, network);
                var line = strategy.DerivationStrategyBase.GetLineFor(DerivationFeature.Deposit);
                var result = new BtcLikePaymentMethodPreviewResult();
                for (var i = offset; i < amount; i++)
                {
                    var address = line.Derive((uint)i);
                    result.Addresses.Add(
                        new BtcLikePaymentMethodPreviewResult.BtcLikePaymentMethodPreviewResultAddressItem()
                        {
                            KeyPath = DerivationStrategyBase.GetKeyPath(DerivationFeature.Deposit).Derive((uint)i)
                                .ToString(),
                            Address = address.ScriptPubKey.GetDestinationAddress(strategy.Network.NBitcoinNetwork)
                                .ToString()
                        });
                }

                return Ok(result);
            }

            catch
            {
                ModelState.AddModelError(nameof(BtcLikePaymentMethod.DerivationScheme), "Invalid Derivation Scheme");
                return BadRequest(ModelState);
            }
        }

        [HttpPut("{cryptoCode}")]
        public async Task<ActionResult<BtcLikePaymentMethod>> UpdateBtcLikePaymentMethod(string cryptoCode,
            [FromBody] BtcLikePaymentMethod paymentMethod)
        {
            paymentMethod.CryptoCode = cryptoCode;

            var id = new PaymentMethodId(cryptoCode, PaymentTypes.BTCLike);

            if (!GetCryptoCodeWallet(cryptoCode, out var network, out var wallet))
            {
                return NotFound();
            }

            try
            {
                var store = Store;
                var storeBlob = store.GetStoreBlob();
                var strategy = ParseDerivationStrategy(paymentMethod.DerivationScheme, network);
                if (strategy != null)
                    await wallet.TrackAsync(strategy.DerivationStrategyBase);
                store.SetSupportedPaymentMethod(id, strategy);
                storeBlob.SetExcluded(id, paymentMethod.Enabled);
                store.SetStoreBlob(storeBlob);

                return Ok(GetExistingBtcLikePaymentMethod(cryptoCode, store));
            }
            catch
            {
                ModelState.AddModelError(nameof(BtcLikePaymentMethod.DerivationScheme), "Invalid Derivation Scheme");
                return BadRequest(ModelState);
            }
        }

        private bool GetCryptoCodeWallet(string cryptoCode, out BTCPayNetwork network, out BTCPayWallet wallet)
        {
            network = _btcPayNetworkProvider.GetNetwork(cryptoCode);
            wallet = network == null ? _walletProvider.GetWallet(network) : null;
            return wallet != null;
        }

        private DerivationStrategy ParseDerivationStrategy(string derivationScheme, BTCPayNetwork network)
        {
            var parser = new DerivationSchemeParser(network.NBitcoinNetwork);
            return new DerivationStrategy(parser.Parse(derivationScheme), network);
        }

        private BtcLikePaymentMethod GetExistingBtcLikePaymentMethod(string cryptoCode, StoreData store = null)
        {
            store = store ?? Store;
            var storeBlob = store.GetStoreBlob();
            var id = new PaymentMethodId(cryptoCode, PaymentTypes.BTCLike);
            var paymentMethod = store
                .GetSupportedPaymentMethods(_btcPayNetworkProvider)
                .OfType<DerivationStrategy>()
                .FirstOrDefault(method => method.PaymentId == id);

            var excluded = storeBlob.IsExcluded(paymentMethod.PaymentId);
            return paymentMethod == null
                ? new BtcLikePaymentMethod(cryptoCode, !excluded)
                : new BtcLikePaymentMethod(paymentMethod, !excluded);
        }
    }

    public class BtcLikePaymentMethodPreviewResult
    {
        public IList<BtcLikePaymentMethodPreviewResultAddressItem> Addresses { get; set; } =
            new List<BtcLikePaymentMethodPreviewResultAddressItem>();

        public class BtcLikePaymentMethodPreviewResultAddressItem
        {
            public string KeyPath { get; set; }
            public string Address { get; set; }
        }
    }

    public class BtcLikePaymentMethod
    {
        public bool Enabled { get; set; }
        public string CryptoCode { get; set; }
        [Required] public string DerivationScheme { get; set; }

        public BtcLikePaymentMethod()
        {
        }

        public BtcLikePaymentMethod(string cryptoCode, bool enabled)
        {
            CryptoCode = cryptoCode;
            Enabled = enabled;
        }

        public BtcLikePaymentMethod(DerivationStrategy derivationStrategy, bool enabled)
        {
            Enabled = enabled;
            CryptoCode = derivationStrategy.PaymentId.CryptoCode;
            DerivationScheme = derivationStrategy.DerivationStrategyBase.ToString();
        }
    }
}
