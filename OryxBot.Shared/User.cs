using System;
using System.Reflection;

namespace OryxBot.Shared
{
    #pragma warning disable 8618
    public class User
    {
        public string email => KEwTWbPyWmdjUKh;
        [Obfuscation(Exclude = true)]
        public string KEwTWbPyWmdjUKh;
        
        public string name => efteqXlZxvUNNvi;
        [Obfuscation(Exclude = true)]
        public string efteqXlZxvUNNvi;
        
        public bool is_subscribed => MVsdYkjeqDKCQBD;
        [Obfuscation(Exclude = true)]
        public bool MVsdYkjeqDKCQBD;

        public bool on_free_trial => uCdeLPhkzFyOSTE;
        [Obfuscation(Exclude = true)]
        public bool uCdeLPhkzFyOSTE;
        
        public bool can_use_custom_routes => is_subscribed && !on_free_trial;
    }
    #pragma warning restore 8618
}
