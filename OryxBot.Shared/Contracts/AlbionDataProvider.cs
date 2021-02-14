using System;
using OryxBot.Shared.Events;

namespace OryxBot.Shared.Contracts
{
    public interface AlbionDataProvider
    {
        public event EventHandler<MoveEventArgs> Move;
    }
}
