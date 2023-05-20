using Dalamud.Configuration;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text;
using System.Text.Json;
using System.Linq;
using Dalamud;
using Dalamud.Logging;
using Dalamud.Utility;

namespace UsedName
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 3;

        public string? Language = null;

        public bool ShowNameChange = true;

        public bool EnableSearchInContext = true;
        public string SearchString = "Search Used Name";

        public bool EnableAddNickName = true;
        public string AddNickNameString = "Add Nick Name";

        public bool EnableAutoUpdate = false;
        public bool UpdateFromPartyList = false;
        public bool UpdateFromFriendList = true;
        public bool UpdateFromCompanyMember = false;


        public class PlayersNames
        {
            public string currentName { get; set; }

            public string nickName { get; set; }
            public List<string> usedNames { get; set; }

            [JsonIgnore]
            public string firstUsedname
            {
                get
                {
                    return this.usedNames.Where(n => !n.IsNullOrEmpty()).ToList().Count >= 1 ? this.usedNames.First(n => !n.IsNullOrEmpty()) : "";
                }
                set { }
            }

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

        public bool EnableSubscription = false;
        public string SubscriptionString = "Subscription";

        public bool modifyStorePath = false;
        public string storeNamesPath = String.Empty;


        // the below exist just to make saving less cumbersome

        public void Initialize()
        {
            if (string.IsNullOrEmpty(this.Language))
            {
                this.Language = Service.ClientState.ClientLanguage switch
                {
                    ClientLanguage.English => "en",
                    ClientLanguage.Japanese => "en",
                    ClientLanguage.French => "en",
                    ClientLanguage.German => "en",
                    // ClientLanguage.ChineseSimplified only exist in CN ver of dalamud
                    // ClientLanguage.ChineseSimplified => "zh_CN",
                    _ => "zh_CN",
                };
            }
            var defaultPath = Path.Join(Service.PluginInterface.ConfigDirectory.FullName, "storeNames.json");
            if (String.IsNullOrEmpty(storeNamesPath))
            {
                Service.PluginInterface.ConfigDirectory.Create();
                storeNamesPath = defaultPath;
            }
            // TODO cannot keep user data due to not completely move configs e.g. user only move UsedName.json not include storeNames.json
            // Check path problem
            if (!defaultPath.Equals(storeNamesPath))
            {
                if (modifyStorePath)
                {
                    if (File.Exists(defaultPath) && File.Exists(storeNamesPath))
                    {
                        var hint = Service.Loc.Localize(
                                       "You modify path of storeNames.json, but there is other storeNames.json at orginal path\n" + 
                                       "Please, delete one that you don't want to use after game close\n") + 
                                   $"Your Path(Current Loading): {storeNamesPath}\nOrginal Path:{defaultPath}";
                        PluginLog.Warning(hint);
                        Service.Chat.PrintError(hint);
                    }
                }
                else
                {
                    // it may happen when user move Dalamud to another place. assume as good user
                    storeNamesPath = defaultPath;

                }
            }
            // if not exist, it means old version config. The old playerNameList would load. Just save it.
            if (File.Exists(storeNamesPath))
            {
                LoadStoreName();
            }
            Save(storeName: true);
        }

        private void LoadStoreName()
        {
            using (StreamReader r = new StreamReader(storeNamesPath))
            {
                string json = r.ReadToEnd();
                // init load playersNameList would be empty. it is ok, just load here
                if (playersNameList.Equals(new Dictionary<ulong, PlayersNames>()))
                {
                    playersNameList = System.Text.Json.JsonSerializer.Deserialize<Dictionary<ulong, PlayersNames>>(json);
                }
                else
                {
                    // not empty means playerNameList not only contain in old config, but also new config. So, merge them. 
                    var temp = System.Text.Json.JsonSerializer.Deserialize<Dictionary<ulong, PlayersNames>>(json);
                    if (temp != null && !temp.Equals((new Dictionary<ulong, PlayersNames>())))
                    {
                        playersNameList = mergePlayerList(playersNameList, temp);
                    }
                }
            }
        }

        public void Save(bool storeName = false)
        {
            if (storeName)
            {
                StoreNames();
            }           
            Service.PluginInterface!.SavePluginConfig(this);
        }

        public void StoreNames()
        {
            string jsonString = System.Text.Json.JsonSerializer.Serialize(playersNameList, new JsonSerializerOptions() { WriteIndented = true, Encoder= JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            using (StreamWriter outputFile = new StreamWriter(storeNamesPath, false, System.Text.Encoding.UTF8))
            {
                outputFile.WriteLine(jsonString);
            }
        }

        internal IDictionary<ulong, PlayersNames> mergePlayerList(IDictionary<ulong, PlayersNames> main, IDictionary<ulong, PlayersNames> sub)
        {
            var result = main;
            foreach(var item in sub)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value);
                }else if(result[item.Key].Equals(item.Value))
                {
                    continue;
                }
                else
                {
                    if (result[item.Key].currentName != item.Value.currentName)
                    {
                        if(item.Value.usedNames.Contains(result[item.Key].currentName))
                        {
                            result[item.Key] = item.Value;
                        }

                    }
                    var tempUsedName = result[item.Key].usedNames;
                    tempUsedName.AddRange(item.Value.usedNames);
                    result[item.Key].usedNames = tempUsedName.Distinct().ToList();
                    if (result[item.Key].nickName != item.Value.nickName)
                    {
                        if (String.IsNullOrEmpty(result[item.Key].nickName))
                        {
                            result[item.Key].nickName = item.Value.nickName;
                        }

                        // use result[item.Key].nickName in other situations
                    }
                }
            }
            return result;
        }
    }
}
