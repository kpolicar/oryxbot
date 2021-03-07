using System;
using System.Collections.Generic;
using System.Configuration;

namespace OryxBot.Client.Windows
{
    public static class UserSettings
    {
        public static void AddUpdateAppSettings(string key, string value) =>
            AddUpdateAppSettings(new Dictionary<string, string> {
                {key, value}
            });
        
        public static void AddUpdateAppSettings(Dictionary<string, string> keyValues)  
        {  
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);  
            var settings = configFile.AppSettings.Settings;
            
            foreach (var keyValuePair in keyValues) {
                if (settings[keyValuePair.Key] == null)  
                {  
                    settings.Add(keyValuePair.Key, keyValuePair.Value);  
                }  
                else  
                {  
                    settings[keyValuePair.Key].Value = keyValuePair.Value;  
                } 
            } 
            configFile.Save(ConfigurationSaveMode.Modified);  
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);  
        }  
        
        public static void Reset()  
        {  
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);  
            var settings = configFile.AppSettings.Settings;
            settings.Clear();
            configFile.Save(ConfigurationSaveMode.Modified);  
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);  
        }  
    }
}
