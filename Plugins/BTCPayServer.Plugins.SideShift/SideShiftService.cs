using System;
using System.Threading.Tasks;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Client;

namespace BTCPayServer.Plugins.SideShift
{
    public class SideShiftService
    {
        private readonly ISettingsRepository _settingsRepository;

        public SideShiftService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }


        public async Task<SideShiftSettings> GetSideShiftForStore(string storeId)
        {
            return await _settingsRepository.GetSettingAsync<SideShiftSettings>(
                $"{nameof(SideShiftSettings)}_{storeId}");
        }

        public async Task SetSideShiftForStore(string storeId, SideShiftSettings sideShiftSettings)
        {
            await _settingsRepository.UpdateSetting(sideShiftSettings, $"{nameof(SideShiftSettings)}_{storeId}");
        }
    }
}
