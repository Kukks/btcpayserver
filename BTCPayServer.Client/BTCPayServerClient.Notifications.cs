using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Client.Models;

namespace BTCPayServer.Client
{
    public partial class BTCPayServerClient
    {
        public virtual async Task<IEnumerable<NotificationData>> GetNotifications(bool? includeSeen = null,
            CancellationToken token = default)
        {
            var response =
                await _httpClient.SendAsync(
                    CreateHttpRequest($"api/v1/users/me/notifications",
                        new Dictionary<string, object>() {{nameof(includeSeen), includeSeen}}), token);
            return await HandleResponse<IEnumerable<NotificationData>>(response);
        }

        public virtual async Task<NotificationData> GetNotifications(string notificationId,
            CancellationToken token = default)
        {
            var response = await _httpClient.SendAsync(
                CreateHttpRequest($"api/v1/users/me/notifications/{notificationId}"), token);
            return await HandleResponse<NotificationData>(response);
        }

        public virtual async Task UpdateNotification(string notificationId, bool? seen,
            CancellationToken token = default)
        {
            var response = await _httpClient.SendAsync(
                CreateHttpRequest($"api/v1/users/me/notifications/{notificationId}",
                    method: HttpMethod.Put, bodyPayload: new UpdateNotification() {Seen = seen}), token);
            await HandleResponse(response);
        }

        public virtual async Task RemoveNotification(string notificationId, CancellationToken token = default)
        {
            var response = await _httpClient.SendAsync(
                CreateHttpRequest($"api/v1/users/me/notifications/{notificationId}",
                    method: HttpMethod.Delete), token);
            await HandleResponse(response);
        }
    }
}
