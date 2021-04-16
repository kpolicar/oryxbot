using OryxBot.Shared.Design;
using OryxBot.Shared.Game;
using static OryxBot.Shared.RunConfiguration;

namespace OryxBot.Shared.Contracts
{
    public interface ActionFactory
    {
        void MoveTowards(Position target);
        void BankRewardItems();
        void UnbankTokenItem();
        void NpcQuestOpenTradeMissionsTab();
        void NpcQuestOpenTradeMissionsContractTab(City origin, City destination);
        void NpcQuestSelectTradeMissionsContract(City origin, City destination, ContractType contract=ContractType.Minor);
        void NpcQuestAcceptTradeMissionsContract();
        void NpcQuestProgress();
        void StopAllActions();
        void InteractWith(Position target);
    }
}
