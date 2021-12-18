
using OryxBot.Shared.Game;

namespace OryxBot.Shared
{
    public readonly struct RunConfiguration
    {
        public enum ContractType {
            Minor=3, Medium=7, Major=15
        }
        
        public readonly ContractType Contract { get; }
        public readonly string? ResumeFromAlias { get; }
        public readonly bool HasProgresedQuest { get; }
        
        public RunConfiguration(ContractType type, string? resumeFromRegion=null, bool hasProgresedQuest=false) =>
            (Contract, ResumeFromAlias, HasProgresedQuest) = (type, resumeFromRegion, hasProgresedQuest);
    }
}
