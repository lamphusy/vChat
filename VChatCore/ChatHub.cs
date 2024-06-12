using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace VChatCore
{
    public class ChatHub : Hub
    {
        public static ConcurrentDictionary<string, string> users = new ConcurrentDictionary<string, string>();

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        public override Task OnConnectedAsync()
        {
            users.TryAdd(Context.ConnectionId, Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            string user;
            users.TryRemove(Context.ConnectionId, out user);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
