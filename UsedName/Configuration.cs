using Dalamud.Configuration;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace UsedName
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        public string Language = "en"; 

        public bool ShowNameChange = true;
        
        public bool EnableSearchInContext = false;
        public string SearchString = "Search Used Name";
        
        public bool EnableAddNickName = true;
        public string AddNickNameString  = "Add Nick Name";

        public bool EnableAutoUpdate = false;

        public class PlayersNames
        {
            public string currentName { get; set; }

            public string nickName { get; set; }
            public List<string> usedNames { get; set; }
            public PlayersNames(string CurrentName, string NickName, List<string> UsedNames)
            {
                this.nickName = NickName;
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
