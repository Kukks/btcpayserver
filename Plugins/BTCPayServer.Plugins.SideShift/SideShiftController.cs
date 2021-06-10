using System;
using System.Threading.Tasks;
using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace BTCPayServer.Plugins.SideShift
{
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie)]
    [Authorize(Policy = Policies.CanModifyStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Cookie)]
    [Route("plugins/{storeId}/SideShift")]
    public class SideShiftController : Controller
    {
        private readonly BTCPayServerClient _btcPayServerClient;

        public SideShiftController(BTCPayServerClient btcPayServerClient)
        {
            _btcPayServerClient = btcPayServerClient;
        }

        [HttpGet("")]
        public async Task<IActionResult> UpdateSideShiftSettings(string storeId)
        {
            var store = await _btcPayServerClient.GetStore(storeId);

            UpdateSideShiftSettingsViewModel vm = new UpdateSideShiftSettingsViewModel();
            vm.StoreName = store.Name;
            SideShiftSettings SideShift = null;
            try
            {
                SideShift = (await _btcPayServerClient.GetStoreAdditionalDataKey(storeId, SideShiftPlugin.StoreBlobKey))
                    .ToObject<SideShiftSettings>();
            }
            catch (Exception e)
            {
                // ignored
            }

            SetExistingValues(SideShift, vm);
            return View(vm);
        }

        private void SetExistingValues(SideShiftSettings existing, UpdateSideShiftSettingsViewModel vm)
        {
            if (existing == null)
                return;
            vm.Enabled = existing.Enabled;
        }

        [HttpPost("")]
        public async Task<IActionResult> UpdateSideShiftSettings(string storeId, UpdateSideShiftSettingsViewModel vm,
            string command)
        {
            var store = await _btcPayServerClient.GetStore(storeId);
            if (vm.Enabled)
            {
                if (!ModelState.IsValid)
                {
                    return View(vm);
                }
            }

            var sideShiftSettings = new SideShiftSettings()
            {
                Enabled = vm.Enabled,
            };

            switch (command)
            {
                case "save":
                    await _btcPayServerClient.UpdateStoreAdditionalDataKey(storeId, SideShiftPlugin.StoreBlobKey,
                        JObject.FromObject(sideShiftSettings));
                    TempData["SuccessMessage"] = "SideShift settings modified";
                    return RedirectToAction(nameof(UpdateSideShiftSettings), new {storeId});

                default:
                    return View(vm);
            }
        }
    }
}
