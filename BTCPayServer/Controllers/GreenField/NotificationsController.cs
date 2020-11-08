using System;
using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Client;
using BTCPayServer.Client.Models;
using BTCPayServer.Contracts;
using BTCPayServer.Data;
using BTCPayServer.Security;
using BTCPayServer.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotificationData = BTCPayServer.Client.Models.NotificationData;

namespace BTCPayServer.Controllers.GreenField
{
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
    [EnableCors(CorsPolicies.All)]
    public class NotificationsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationManager _notificationManager;

        public NotificationsController(UserManager<ApplicationUser> userManager,
            NotificationManager notificationManager)
        {
            _userManager = userManager;
            _notificationManager = notificationManager;
        }

        [Authorize(Policy = Policies.CanViewNotificationsForUser,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/users/me/notifications")]
        public async Task<IActionResult> GetNotifications(bool? seen = null)
        {
            var store = HttpContext.GetStoreData();
            if (store == null)
            {
                return NotFound();
            }

            var items = await _notificationManager.GetNotifications(new NotificationsQuery()
            {
                Seen = seen, UserId = _userManager.GetUserId(User)
            });

            return Ok(items.Items.Select(ToModel));
        }

        [Authorize(Policy = Policies.CanViewNotificationsForUser,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/users/me/notifications/{id}")]
        public async Task<IActionResult> GetNotification(string id)
        {
            var store = HttpContext.GetStoreData();
            if (store == null)
            {
                return NotFound();
            }

            var items = await _notificationManager.GetNotifications(new NotificationsQuery()
            {
                Ids = new []{id}, UserId = _userManager.GetUserId(User)
            });

            if (items.Count == 0)
            {
                return NotFound();
            }
            return Ok(ToModel(items.Items.First()));
        }
        
        [Authorize(Policy = Policies.CanManageNotificationsForUser,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/users/me/notifications/{id}")]
        public async Task<IActionResult> UpdateNotification(string id, UpdateNotification request)
        {
            var store = HttpContext.GetStoreData();
            if (store == null)
            {
                return NotFound();
            }

            var items = await _notificationManager.ToggleSeen(new NotificationsQuery()
            {
                Ids = new []{id}, UserId = _userManager.GetUserId(User)
            }, request.Seen);

            if (items.Count == 0)
            {
                return NotFound();
            }
            return Ok(ToModel(items.First()));
        }
        
        [Authorize(Policy = Policies.CanManageNotificationsForUser,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpDelete("~/api/v1/users/me/notifications/{id}")]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            var store = HttpContext.GetStoreData();
            if (store == null)
            {
                return NotFound();
            }

            await _notificationManager.Remove(new NotificationsQuery()
            {
                Ids = new[] {id}, UserId = _userManager.GetUserId(User)
            });

            return Ok();
        }

        private NotificationData ToModel(NotificationViewModel entity)
        {
            return new NotificationData()
            {
                Id = entity.Id,
                CreatedTime = entity.Created,
                Body = entity.Body,
                Seen = entity.Seen,
                Link = string.IsNullOrEmpty(entity.ActionLink) ? null : new Uri(entity.ActionLink)
            };
        }
    }
}
