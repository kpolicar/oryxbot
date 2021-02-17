using System;
using OryxBot.Shared.Events;

namespace OryxBot.Shared.Contracts
{
    public interface AlbionDataProvider
    {
        event EventHandler<MoveEventArgs> Move;
        event EventHandler<ChangeClusterEventArgs>? ChangeCluster;
    }
}
