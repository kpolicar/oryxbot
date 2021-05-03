using System;

namespace OryxBot.Shared.Contracts
{
    public interface Hotkey
    {
        public event EventHandler? F1;
        public event EventHandler? F2;
        public event EventHandler? F3;
        public event EventHandler? F5;
        public event EventHandler? Insert;
        public event EventHandler? F10;
        public event EventHandler? Escape;
    }
}
