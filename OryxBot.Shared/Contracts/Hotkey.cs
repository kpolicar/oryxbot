using System;

namespace OryxBot.Shared.Contracts
{
    public interface Hotkey
    {
        public event EventHandler? F1;
        public event EventHandler? Escape;
    }
}
