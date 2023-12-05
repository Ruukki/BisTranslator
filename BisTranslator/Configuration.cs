using BisTranslator.Translator;
using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace BisTranslator
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;
        public string CommandRegex { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string CommandMatch { get; set; } = string.Empty;

        public bool BigPussy { get; set; } = false;

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? _pluginInterface;

        public Configuration(DalamudPluginInterface pluginInterface)
        {
            _pluginInterface = pluginInterface;
        }

        public void Save()
        {
            CommandRegex = @$"(?i)^(?:{Name},)\s+(?:\((.*?)\)|(\w+))";
            CommandMatch = $"\"{Name}, \"";
            Translations.SetName(Name);
            _pluginInterface?.SavePluginConfig(this);
        }
    }
}
