using System.Collections.Generic;
using OryxBot.Shared.Design;

namespace OryxBot.Bot.Contracts
{
    public interface TradeMissionRouteProvider
    {
        LinkedList<TradeMissionRecord.RecordableStep>? Route();
        LinkedList<TradeMissionRecord.RecordableStep>? RouteFromBankToQuest();
        LinkedList<TradeMissionRecord.RecordableStep>? RouteFromQuestToBank();
    }
}
