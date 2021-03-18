using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace OryxBot.Client.Windows.Bot.Contracts
{
    public interface TradeMissionRouteProvider
    {
        public bool CustomRoutes { get; }
        event EventHandler? ModeChanged;
        LinkedList<TradeMissionRecord.RecordableStep>? Route();
        LinkedList<TradeMissionRecord.RecordableStep>? RouteBack();
        LinkedList<TradeMissionRecord.RecordableStep>? RouteFromBankToQuest();
        LinkedList<TradeMissionRecord.RecordableStep>? RouteFromQuestToBank();
        void ToggleCustomMode();
    }
}
