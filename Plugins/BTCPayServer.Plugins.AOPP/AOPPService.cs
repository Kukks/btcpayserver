using System.Threading.Tasks;
using BTCPayServer.Abstractions.Contracts;

namespace BTCPayServer.Plugins.AOPP
{
    public class AOPPService
    {
        private readonly ISettingsRepository _settingsRepository;

        public AOPPService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }


        public async Task<AOPPSettings> GetAOPPForStore(string storeId)
        {
            return await _settingsRepository.GetSettingAsync<AOPPSettings>(
                $"{nameof(AOPPSettings)}_{storeId}");
        }

        public async Task SetAOPPForStore(string storeId, AOPPSettings AOPPSettings)
        {
            await _settingsRepository.UpdateSetting(AOPPSettings, $"{nameof(AOPPSettings)}_{storeId}");
        }
    }
}
