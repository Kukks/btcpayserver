using System;
using System.Threading.Tasks;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Client;
using Microsoft.Extensions.Caching.Memory;

namespace BTCPayServer.Plugins.SideShift
{
    public class SideShiftService: IDisposable
    {
        private readonly IBTCPayServerClientFactory _btcPayServerClientFactory;
        private readonly IMemoryCache _memoryCache;
        private BTCPayServerClient _client;

        public SideShiftService(IBTCPayServerClientFactory btcPayServerClientFactory,IMemoryCache memoryCache)
        {
            _btcPayServerClientFactory = btcPayServerClientFactory;
            _memoryCache = memoryCache;
        }
        
        public async Task<SideShiftSettings> GetSideShiftForInvoice(string id)
        {

            _client ??= await _btcPayServerClientFactory.Create("");
            return await _memoryCache.GetOrCreateAsync<SideShiftSettings>($"{nameof(SideShiftService)}-{id}", async entry =>
            {
                try
                {

                    var i = await _client.AdminGetInvoice(id);
                }
                catch (Exception e)
                {
                    return null;
                }
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return new SideShiftSettings() { Enabled = true };
                // var d = await _storeRepository.GetStoreByInvoiceId(id);
                //
                // return d?.GetStoreBlob()?.GetSideShiftSettings();
            });
        }

        public void Dispose()
        {
            _memoryCache?.Dispose();
        }
    }
}
