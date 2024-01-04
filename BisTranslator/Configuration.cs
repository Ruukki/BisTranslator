using BisTranslator.Translator;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BisTranslator
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {        
        public int Version { get; set; } = 0;

        public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;
        public bool UseFullMatch { get; set; } = false;
        public string CommandRegex
        {
            get
            {
                if (UseFullMatch)
                {
                    return $@"(?i)^(?:{Name},)\s+(?:\((.*?)\)|(.+))"; //Full match
                }
                return @$"(?i)^(?:{Name},)\s+(?:\((.*?)\)|(\w+))"; //original                
            }
        }
        
        public string Name { get; set; } = "your new name";
        public string CommandMatch
        {
            get
            {
                return $"\"{Name}, \"";
            }
        }

        public bool BigPussy { get; set; } = false;
        public bool SuperSecretFeature { get; set; } = false;

        public bool ForcedWalk { get; set; } = true;

        #region Ability related
        public bool GilCheck { get; set;} = true;
        public long GilLimit { get; set; } = 110000;
        public AbilityRestrictionLevel AbilityRestrictionLevel { get; set; } = AbilityRestrictionLevel.None;
        public List<ActionRoles> BannedActionRoles { get; set; } = new List<ActionRoles>();
        public bool canSelfCast = true;
        #endregion

        [NonSerialized]
        public bool lockedUiOverride = false;
        [NonSerialized]
        public bool isConfigOverriden = false;
        [NonSerialized]
        public bool tester = false;

        // System
        [NonSerialized]
        public bool GilOverflow = false;
        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? _pluginInterface;
        public Configuration(DalamudPluginInterface pluginInterface)
        {
            _pluginInterface = pluginInterface;
        }
        public Configuration() { }

        public void LoadInterface(DalamudPluginInterface pluginInterface)
        {
            _pluginInterface = pluginInterface;
        }

        public void Save()
        {
            Translations.SetName(Name);

            _pluginInterface?.SavePluginConfig(this);
        }

        public void Override(Configuration configuration, DalamudPluginInterface dalamudPluginInterface)
        {
            Version = configuration.Version;
            SomePropertyToBeSavedAndWithADefault = configuration.SomePropertyToBeSavedAndWithADefault;
            Name = configuration.Name;
            BigPussy = configuration.BigPussy;
            SuperSecretFeature = configuration.SuperSecretFeature;
            ForcedWalk = configuration.ForcedWalk;
            GilCheck = configuration.GilCheck;
            GilLimit = configuration.GilLimit;
            AbilityRestrictionLevel = configuration.AbilityRestrictionLevel;
            BannedActionRoles = configuration.BannedActionRoles;

            lockedUiOverride = configuration.lockedUiOverride;
            isConfigOverriden = true;
            tester = configuration.tester;

            _pluginInterface = dalamudPluginInterface;
            Translations.SetName(Name);
        }

        /*public Configuration Load(DirectoryInfo configDirectory)
        {
            var pluginConfigPath = new FileInfo(Path.Combine(configDirectory.Parent!.FullName, "BisTranslator.json"));

            if (!pluginConfigPath.Exists)
                return new Configuration();

            var data = File.ReadAllText(pluginConfigPath.FullName);
            var conf = JsonConvert.DeserializeObject<Configuration>(data);
            return conf ?? new Configuration();
        }*/
    }
}
