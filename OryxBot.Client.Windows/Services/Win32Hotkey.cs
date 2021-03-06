using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using OryxBot.Shared.Contracts;

namespace OryxBot.Client.Windows.Services
{
    public class Win32Hotkey : Hotkey, IDisposable
    {
        public event EventHandler? F1;
        public event EventHandler? F2;
        public event EventHandler? F3;
        public event EventHandler? Escape;
        private IKeyboardMouseEvents? m_GlobalHook;
        private Random rand = new Random();
        
        public Win32Hotkey() {
            m_GlobalHook = Hook.GlobalEvents();
            
            var combinations = new Dictionary<Combination, Action> {
                {Combination.FromString("F1"), () => F1?.Invoke(this, EventArgs.Empty)},
                {Combination.FromString("F2"), () => F2?.Invoke(this, EventArgs.Empty)},
                {Combination.FromString("F3"), () => F3?.Invoke(this, EventArgs.Empty)},
                {Combination.FromString("Escape"), () => Escape?.Invoke(this, EventArgs.Empty)}
            };
            
            m_GlobalHook.OnCombination(combinations);
        }

        public void Dispose() {
            m_GlobalHook?.Dispose();
        }
    }
}
