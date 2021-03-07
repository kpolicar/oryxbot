using OryxBot.Shared.Design;

namespace OryxBot.Shared.Contracts
{
    public interface ActionFactory
    {
        void MoveTowards(Position origin, Position target);
        void BankRewardItems();
        void UnbankTokenItem();
        void NpcQuestOpenTradeMissionsTab();
        void NpcQuestOpenTradeMissionsContractTab();
        void NpcQuestSelectTradeMissionsContract();
        void NpcQuestAcceptTradeMissionsContract();
        void NpcQuestProgress();
        void StopAllActions();
        void InteractWith(Position origin, Position target);
    }
}
