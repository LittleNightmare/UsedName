using Dalamud.Configuration;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

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
        public string AddNickNameString = "Add Nick Name";

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
        [JsonIgnore]
        public IDictionary<ulong, PlayersNames> playersNameList = new Dictionary<ulong, PlayersNames>();
        // Separate data and setting
        [JsonProperty("playersNameList")]
        private IDictionary<ulong, PlayersNames> playersNameListTemp
        {
            // get is intentionally omitted here
            set { playersNameList = value; }
        }

        public string storeNamesPath = String.Empty;


        // the below exist just to make saving less cumbersome

        public void Initialize()
        {

            if (String.IsNullOrEmpty(storeNamesPath))
            {
                var path = Service.PluginInterface.ConfigDirectory;
                path.Create();
                storeNamesPath = Path.Join(path.FullName, "storeNames.json");
            }

            if (File.Exists(storeNamesPath))
            {
                using (StreamReader r = new StreamReader(storeNamesPath))
                {
                    string json = r.ReadToEnd();
                    if (playersNameList.Equals(new Dictionary<ulong, PlayersNames>()))
                    {
                        playersNameList = System.Text.Json.JsonSerializer.Deserialize<Dictionary<ulong, PlayersNames>>(json);

                    }
                }
            }

        }

        public void Save(bool storeName = false)
        {
            if (storeName)
            {
                storeNames();
            }           
            Service.PluginInterface!.SavePluginConfig(this);
        }

        public void storeNames()
        {
            string jsonString = System.Text.Json.JsonSerializer.Serialize(playersNameList, new JsonSerializerOptions() { WriteIndented = true });
            using (StreamWriter outputFile = new StreamWriter(storeNamesPath, false, System.Text.Encoding.UTF8))
            {
                outputFile.WriteLine(jsonString);
            }
        }
    }
}
