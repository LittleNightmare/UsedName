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
        
        public string? Language = null; 

        public bool ShowNameChange = true;
        
        public bool EnableSearchInContext = false;
        public string SearchString = "Search Used Name";
        
        public bool EnableAddNickName = true;
        public string AddNickNameString  = "Add Nick Name";

        public bool EnableAutoUpdate = false;
        public bool UpdateFromPartyList = false;
        public bool UpdateFromFriendList = true;
        public bool UpdateFromPlayerSearch = false;

        public bool AutoCheckOpcodeUpdate = true;
        public int SocialListOpcode = 0;
        public string GameVersion = (new Dalamud.Game.GameVersion(0000)).ToString();

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

        public void Initialize()
        {
        }

        public void Save()
        {
            Service.PluginInterface!.SavePluginConfig(this);
        }
    }
}
