using System;
using System.Threading.Tasks;
using OryxBot.Client.Windows.Api;
using OryxBot.Client.Windows.Events;
using OryxBot.Shared;
using OryxBot.Shared.Events;

namespace OryxBot.Client.Windows.Contracts
{
    public interface AuthManager
    {
        public User? User {
            get;
        }
        public event EventHandler<ApiConnectionChangedEventArgs>? ConnectionChanged;
        public Task<ApiConnection?> Login(string username, string password);
        public void Logout();
    }
}
