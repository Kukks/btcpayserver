using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Abstractions.Extensions;
using BTCPayServer.Client;
using BTCPayServer.Client.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;

namespace BTCPayServer.Plugins.TicketTailor
{
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie)]
    [Authorize(Policy = Policies.CanModifyStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Cookie)]
    [Route("plugins/{storeId}/TicketTailor")]
    public class TicketTailorController : Controller
    {
        [AllowAnonymous]
        [HttpGet("")]
        public async Task<IActionResult> View(string storeId)
        {
            var config = await _ticketTailorService.GetTicketTailorForStore(storeId);
            try
            {
                if (config?.ApiKey is not null && config?.EventId is not null)
                {
                    var client = new TicketTailorClient(_httpClientFactory, config.ApiKey);
                    var evt = await client.GetEvent(config.EventId);
                    if (evt is null)
                    {
                        return NotFound();
                    }

                    return View(evt);
                }
            }
            catch (Exception e)
            {
            }

            return NotFound();
        }


        [AllowAnonymous]
        [HttpPost("")]
        public async Task<IActionResult> Purchase(string storeId, string ticketTypeId, string firstName, string lastName, string email)
        {
            var config = await _ticketTailorService.GetTicketTailorForStore(storeId);
            try
            {
                if (config?.ApiKey is not null && config?.EventId is not null)
                {
                    var client = new TicketTailorClient(_httpClientFactory, config.ApiKey);
                    var evt = await client.GetEvent(config.EventId);
                    if (evt is null || evt.Unavailable == "true" || evt.TicketsAvailable == "false")
                    {
                        return NotFound();
                    }

                    var ticketType = evt.TicketTypes.FirstOrDefault(type => type.Id == ticketTypeId);
                    if (ticketType is null || ticketType.Status != "on_sale" ||  ticketType.Quantity <= 0)
                    {
                        return NotFound();
                    }

                    var btcpayClient =
                        await _btcPayServerClientFactory.Create(null, new[] {storeId}, HttpContext);
                    var redirectUrl = Request.GetAbsoluteUri(Url.Action("Receipt",
                        "TicketTailor", new {storeId, invoiceId = "kukkskukkskukks"}));
                    redirectUrl = redirectUrl.Replace("kukkskukkskukks", "{InvoiceId}");
                    var inv = await btcpayClient.CreateInvoice(storeId,
                        new CreateInvoiceRequest()
                        {
                            Amount = ticketType.Price,
                            Currency = evt.Currency,
                            Type = InvoiceType.Standard,
                            AdditionalSearchTerms = new[] {"tickettailor", ticketTypeId, evt.Id},
                            Checkout =
                            {
                                RequiresRefundEmail = true,
                                RedirectAutomatically = ticketType.Price > 0,
                                RedirectURL = redirectUrl,
                            },
                            Metadata = JObject.FromObject(new
                            {
                                buyerName = $"{firstName} {lastName}",
                                buyerEmail = email,
                                ticketTypeId
                            })
                        });
                    
                    while (inv.Amount == 0 && inv.Status == InvoiceStatus.New)
                    {
                        if(inv.Status == InvoiceStatus.New )
                            inv = await btcpayClient.GetInvoice(inv.StoreId, inv.Id);
                    }

                    if(inv.Status == InvoiceStatus.Settled)
                        return RedirectToAction("Receipt", new {storeId, invoiceId = inv.Id});
                    return Redirect(inv.CheckoutLink);
                }
            }
            catch (Exception e)
            {
            }

            return RedirectToAction("View", new {storeId});
        }

        

        [AllowAnonymous]
        [HttpGet("receipt")]
        public async Task<IActionResult> Receipt(string storeId, string invoiceId)
        {
            
            var btcpayClient =
                await _btcPayServerClientFactory.Create(null, new[] {storeId}, ControllerContext.HttpContext);
            try
            {
                var result = new TicketReceiptPage() {InvoiceId = invoiceId};
                var invoice = await btcpayClient.GetInvoice(storeId, invoiceId);
                if (invoice.Status == InvoiceStatus.Settled)
                {
                    result.Status = invoice.Status;
                    if (invoice.Metadata.TryGetValue("ticketId", out var ticketId))
                    {
                        var settings = await _ticketTailorService.GetTicketTailorForStore(storeId);
                        var client = new TicketTailorClient(_httpClientFactory, settings.ApiKey);
                        result.Ticket = await client.GetTicket(ticketId.ToString());
                        var evt = await client.GetEvent(settings.EventId);
                        result.Event = evt;
                        result.TicketType =
                            evt.TicketTypes.FirstOrDefault(type => type.Id == result.Ticket.TicketTypeId);
                    }
                }

                return View(result);
            }
            catch (Exception e)
            {
                return NotFound();
            }
            
            
        }

        public class TicketReceiptPage
        {
            public string InvoiceId { get; set; }
            public InvoiceStatus Status { get; set; }
            public TicketTailorClient.IssuedTicket Ticket { get; set; }
            public TicketTailorClient.Event Event { get; set; }
            public TicketTailorClient.TicketType TicketType { get; set; }
        }
        
        
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TicketTailorService _ticketTailorService;
        private readonly IBTCPayServerClientFactory _btcPayServerClientFactory;

        public TicketTailorController(IHttpClientFactory httpClientFactory,
            TicketTailorService ticketTailorService,
            IBTCPayServerClientFactory btcPayServerClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _ticketTailorService = ticketTailorService;
            _btcPayServerClientFactory = btcPayServerClientFactory;
        }

        [HttpGet("update")]
        public async Task<IActionResult> UpdateTicketTailorSettings(string storeId)
        {
            UpdateTicketTailorSettingsViewModel vm = new();
            TicketTailorSettings TicketTailor = null;
            try
            {
                TicketTailor = await _ticketTailorService.GetTicketTailorForStore(storeId);
                if (TicketTailor is not null)
                {
                    vm.ApiKey = TicketTailor.ApiKey;
                    vm.EventId = TicketTailor.EventId;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                if (!string.IsNullOrEmpty(vm.ApiKey))
                {
                    var client = new TicketTailorClient(_httpClientFactory, vm.ApiKey);
                    var evts = await client.GetEvents();
                    if (vm.EventId is not null && evts.All(e => e.Id != vm.EventId))
                    {
                        vm.EventId = null;
                    }

                    evts = evts.Prepend(new TicketTailorClient.Event() {Id = null, Title = "Select an event"}).ToArray();
                    vm.Events = new SelectList(evts, nameof(TicketTailorClient.Event.Id),
                        nameof(TicketTailorClient.Event.Title), vm.EventId);
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError(nameof(vm.ApiKey), "Api key did not work.");
            }

            return View(vm);
        }
        

        [HttpPost("update")]
        public async Task<IActionResult> UpdateTicketTailorSettings(string storeId,
            UpdateTicketTailorSettingsViewModel vm,
            string command, [FromServices] BTCPayServerClient btcPayServerClient)
        {
            try
            {
                if (!string.IsNullOrEmpty(vm.ApiKey))
                {
                    var client = new TicketTailorClient(_httpClientFactory, vm.ApiKey);
                    var evts = await client.GetEvents();
                    if (vm.EventId is not null && evts.All(e => e.Id != vm.EventId))
                    {
                        vm.EventId = null;
                    }

                    vm.Events = new SelectList(evts, nameof(TicketTailorClient.Event.Id),
                        nameof(TicketTailorClient.Event.Title), vm.EventId);
                    
                    evts = evts.Prepend(new TicketTailorClient.Event() {Id = null, Title = "Select an event"}).ToArray();
                }
                else
                {
                    vm.Events = null;
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError(nameof(vm.ApiKey), "Api key did not work.");
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var settings = new TicketTailorSettings() {ApiKey = vm.ApiKey, EventId = vm.EventId};
            if (vm.ApiKey is not null && vm.EventId is not null)
            {
                try
                {
                    var webhook = await btcPayServerClient.GetWebhook(storeId, vm.WebhookId);
                    settings.WebhookId = webhook.Id;
                }
                catch (Exception e)
                {
                    var wh = await btcPayServerClient.CreateWebhook(storeId,
                        new CreateStoreWebhookRequest()
                        {
                            Enabled = true,
                            Url = Request.GetAbsoluteUri(Url.Action("Callback",
                                "TicketTailor", new {storeId})),
                            AuthorizedEvents = new StoreWebhookBaseData.AuthorizedEventsData()
                            {
                                SpecificEvents = new[] {WebhookEventType.InvoiceSettled}
                            },
                            AutomaticRedelivery = true
                        });
                    settings.WebhookId = wh.Id;
                }
            }

            switch (command?.ToLowerInvariant())
            {
                case "save":
                    await _ticketTailorService.SetTicketTailorForStore(storeId, settings);
                    TempData["SuccessMessage"] = "TicketTailor settings modified";
                    return RedirectToAction(nameof(UpdateTicketTailorSettings), new {storeId});

                default:
                    return View(vm);
            }
        }

        [AllowAnonymous]
        [HttpPost("callback")]
        public async Task<IActionResult> Callback(string storeId, [FromBody] WebhookInvoiceSettledEvent response)
        {
            if (response.StoreId != storeId)
            {
                return BadRequest();
            }

            var settings = await _ticketTailorService.GetTicketTailorForStore(storeId);
            if (settings is null || settings.ApiKey is null)
            {
                return BadRequest();
            }

            var btcPayClient =
                await _btcPayServerClientFactory.Create(null, new[] {storeId}, ControllerContext.HttpContext);
            var invoice = await btcPayClient.GetInvoice(storeId, response.InvoiceId);
            if (invoice.Status != InvoiceStatus.Settled)
            {
                return BadRequest();
            }

            if (invoice.Metadata.ContainsKey("ticketId"))
            {
                return Ok();
            }
            var ticketTypeId = invoice.Metadata["ticketTypeId"].ToString();
            var email = invoice.Metadata["buyerEmail"].ToString();
            var name = invoice.Metadata["buyerName"]?.ToString();
            invoice.Metadata.TryGetValue("posData", out var posData);
            posData ??= new JObject();
            var client = new TicketTailorClient(_httpClientFactory, settings.ApiKey);
            try
            {
                var ticket = await client.CreateTicket(new TicketTailorClient.IssueTicketRequest()
                {
                    Reference = invoice.Id,
                    Email = email, 
                    EventId = settings.EventId, 
                    TicketTypeId = ticketTypeId, 
                    FullName = name, 
                });
                invoice.Metadata["ticketId"] = ticket.Id;
                posData["Ticket Code"] = ticket.Barcode;
                invoice.Metadata["posData"] = posData;
                await btcPayClient.UpdateInvoice(storeId, invoice.Id,
                    new UpdateInvoiceRequest() {Metadata = invoice.Metadata});
                
                var url =
                    Request.GetAbsoluteUri(Url.Action("Receipt", new {storeId, invoiceId = invoice.Id}));
                try
                {
                    await btcPayClient.SendEmail(storeId,
                        new SendEmailRequest()
                        {
                            Subject = "Your ticket is available now.",
                            Email = email,
                            Body =
                                $"Your payment has been settled and the event ticket has been issued successfully. Please go to <a href='{url}'>{url}</a>"
                        });
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
            catch (Exception e)
            {

                posData["Error"] = "Ticket could not be created. You should refund customer.";
                invoice.Metadata["posData"] = posData;
                await btcPayClient.UpdateInvoice(storeId, invoice.Id,
                    new UpdateInvoiceRequest() {Metadata = invoice.Metadata});
                await btcPayClient.MarkInvoiceStatus(storeId, invoice.Id,
                    new MarkInvoiceStatusRequest() {Status = InvoiceStatus.Invalid});
                
            }
            return Ok();
        }
    }
}
