using Microsoft.AspNetCore.Mvc.Rendering;

namespace BTCPayServer.Plugins.TicketTailor;

public class UpdateTicketTailorSettingsViewModel
{
    public string ApiKey { get; set; }
    public SelectList Events { get; set; }
    public string EventId { get; set; }
    public string WebhookId { get; set; }
}
