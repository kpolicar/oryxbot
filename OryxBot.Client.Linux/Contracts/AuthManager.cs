using System;
using System.Threading.Tasks;
using OryxBot.Client.Linux.Api;
using OryxBot.Client.Linux.Events;
using OryxBot.Shared;

namespace OryxBot.Client.Linux.Contracts
{
    public interface AuthManager
    {
        public User? User { get; }

        public event EventHandler<ApiConnectionChangedEventArgs>? ConnectionChanged;
        public event EventHandler<AuthChangedEvent>? AuthChanged;
        public Task<ApiConnection?> Login(string username, string password);
        public ApiConnection? LoginWithToken(string token);
        public void Logout();
    }
}
