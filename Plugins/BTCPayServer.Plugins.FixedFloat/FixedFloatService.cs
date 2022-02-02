using System.Threading.Tasks;
using BTCPayServer.Abstractions.Contracts;

namespace BTCPayServer.Plugins.FixedFloat
{
    public class FixedFloatService
    {
        private readonly ISettingsRepository _settingsRepository;

        public FixedFloatService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }


        public async Task<FixedFloatSettings> GetFixedFloatForStore(string storeId)
        {
            return await _settingsRepository.GetSettingAsync<FixedFloatSettings>(
                $"{nameof(FixedFloatSettings)}_{storeId}");
        }

        public async Task SetFixedFloatForStore(string storeId, FixedFloatSettings FixedFloatSettings)
        {
            await _settingsRepository.UpdateSetting(FixedFloatSettings, $"{nameof(FixedFloatSettings)}_{storeId}");
        }
    }
}
