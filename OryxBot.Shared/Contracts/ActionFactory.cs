using System.Numerics;
using OryxBot.Shared.Design;

namespace OryxBot.Shared.Contracts
{
    public interface ActionFactory
    {
        void MoveTowards(Position origin, Position target);
    }
}
