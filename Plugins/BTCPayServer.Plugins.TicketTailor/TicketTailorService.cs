using System.Threading.Tasks;
using BTCPayServer.Abstractions.Contracts;

namespace BTCPayServer.Plugins.TicketTailor;

public class TicketTailorService
{
    private readonly ISettingsRepository _settingsRepository;

    public TicketTailorService(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }
        
    public async Task<TicketTailorSettings> GetTicketTailorForStore(string storeId)
    {
        return await _settingsRepository.GetSettingAsync<TicketTailorSettings>(
            $"{nameof(TicketTailorSettings)}_{storeId}");
    }

    public async Task SetTicketTailorForStore(string storeId, TicketTailorSettings ticketTailorSettings)
    {
        await _settingsRepository.UpdateSetting(ticketTailorSettings, $"{nameof(TicketTailorSettings)}_{storeId}");
    }
}
