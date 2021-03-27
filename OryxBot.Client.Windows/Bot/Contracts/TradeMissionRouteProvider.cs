using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace OryxBot.Client.Windows.Bot.Contracts
{
    public interface TradeMissionRouteProvider
    {
        public bool CustomRoutes { get; }
        event EventHandler? ModeChanged;
        Route? Route();
        Route? RouteBack();
        Route? RouteFromBankToQuest();
        Route? RouteFromQuestToBank();
        void ToggleCustomMode();
    }
}
