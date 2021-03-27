using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using OryxBot.Shared.Game;

namespace OryxBot.Client.Windows.Bot.Contracts
{
    public interface TradeMissionRouteManager
    {
        public bool CustomRoutes { get; }
        event EventHandler? ModeChanged;
        Route? Route();
        Route? RouteBack();
        Route? RouteFromBankToQuest();
        Route? RouteFromQuestToBank();
        void ToggleCustomMode();
        StreamWriter SaveRouteStream();
        void SetDefaultRouteCity(City city);
    }
}
