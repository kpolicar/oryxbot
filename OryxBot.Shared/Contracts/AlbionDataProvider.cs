using System;
using OryxBot.Shared.Events;
using OryxBot.Shared.Game;

namespace OryxBot.Shared.Contracts
{
    public interface AlbionDataProvider
    {
        public Character LocalCharacter {
            get;
        }
    }
}
