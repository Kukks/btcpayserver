﻿using System.Collections.Generic;
using System.Linq;
using BTCPayServer.Payments;
using BTCPayServer.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Controllers.NewApi
{
    [ApiController]
    [IncludeInOpenApiDocs]
    [Route("api/v0.1/PaymentMethods")]
    [Authorize()]
    public class PaymentMethodsController : ControllerBase
    {
        [HttpGet("")]
        public ActionResult<IEnumerable<string>> GetPaymentMethods()
        {
            return Ok(new List<string>()
            {
                nameof(PaymentTypes.BTCLike)
            });
        }
    }
    
    
    
    [ApiController]
    [IncludeInOpenApiDocs]
    [Route("api/v0.1/PaymentMethods/" + nameof(PaymentTypes.BTCLike))]
    [Authorize()]
    public class BTCLikePaymentMethodsController : ControllerBase
    {
        private readonly BTCPayNetworkProvider _btcPayNetworkProvider;

        public BTCLikePaymentMethodsController(BTCPayNetworkProvider btcPayNetworkProvider)
        {
            _btcPayNetworkProvider = btcPayNetworkProvider;
        }
        [HttpGet("")]
        public ActionResult<IEnumerable<BtcLikePaymentMethod>> GetBtcLikePaymentMethods()
        {
            return Ok(_btcPayNetworkProvider
                .GetAll()
                .Select(network => new BtcLikePaymentMethod()
                {
                    CryptoCode = network.CryptoCode
                }));
        }
    }

    public class BtcLikePaymentMethod
    {
        public string CryptoCode { get; set; }
    
    }
}
