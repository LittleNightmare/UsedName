using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace UsedName
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

        public class PlayersNames
        {
            public string currentName { get; set; }
            public List<string> usedNames { get; set; }

            public PlayersNames(string CurrentName, List<string> UsedNames)
            {
                this.currentName = CurrentName;
                this.usedNames = UsedNames;
            }

        }

        public IDictionary<ulong, PlayersNames> playersNameList = new Dictionary<ulong, PlayersNames>();

        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
