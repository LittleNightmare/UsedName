using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud;
using Dalamud.Game;
using Dalamud.Game.Network;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.ContextMenu;
using Dalamud.Interface.Internal.Notifications;
using XivCommon;
using System.Collections.Generic;
using System.Linq;
using System;
using Dalamud.Logging;
using System.Runtime.InteropServices;
using Dalamud.Data;
using UsedName.Structs;
using Lumina.Excel.GeneratedSheets;
using System.Text;
using Dalamud.Hooking;
namespace UsedName
{
    public sealed class UsedName : IDalamudPlugin
    {

        public string Name => "Used Name";

        private const string commandName = "/pname";

        public UsedName(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();
            Service.ContextMenu = new DalamudContextMenu();

            Service.Configuration = Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Service.Configuration.Initialize();

            Service.Common = new XivCommonBase();
            Service.ContextMenuManager = new ContextMenu(this);

            if (string.IsNullOrEmpty(Service.Configuration.Language))
            {
                Service.Configuration.Language = Service.ClientState.ClientLanguage switch
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
            Service.Loc = new Localization(Service.Configuration.Language);

            if (Service.Scanner.TryScanText("48 89 5c 24 ?? 56 48 ?? ?? ?? 48 ?? ?? ?? ?? ?? ?? 48 ?? ?? e8 ?? ?? ?? ?? 48 ?? ?? 48 ?? ?? 0f 84 ?? ?? ?? ?? 0f", out var ptr0))
            {
                Service.GetSocialListHook = Hook<Service.GetSocialListDelegate>.FromAddress(ptr0, GetSocialListDetour);
            }

            // you might normally want to embed resources and load them from the manifest stream
            Service.PluginUi = new PluginUI(this);
            
            Service.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = Service.Loc.Localize("Use '/pname' or '/pname update' to update data from FriendList\n") +
                Service.Loc.Localize("Use '/pname search firstname lastname' to search 'firstname lastname's used name. I **recommend** using the right-click menu to search\n") +
                Service.Loc.Localize("Use '/pname nick firstname lastname nickname' set 'firstname lastname's nickname to 'nickname'\n") +
                Service.Loc.Localize("(Format require:first last nickname; first last nick name)\n") +
                Service.Loc.Localize("Use '/pname config' show plugin's setting")
            }) ;

            // first time
            if (Service.Configuration.playersNameList.Count <= 0)
            {
                this.GetDataFromXivCommon();
            }

            Service.PluginInterface.UiBuilder.Draw += DrawUI;
            Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            if(Service.Configuration.EnableAutoUpdate)
                Service.GetSocialListHook?.Enable();
        }

        public void Dispose()
        {
            Service.GetSocialListHook?.Disable();
            Service.GetSocialListHook?.Dispose();
            Service.PluginInterface.UiBuilder.Draw -= DrawUI;
            Service.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
            Service.PluginUi.Dispose();
            Service.CommandManager.RemoveHandler(commandName);
            Service.Common.Dispose();
            Service.ContextMenuManager.Dispose();
            Service.ContextMenu.Dispose();
            Service.Configuration.Save(storeName:true);
#if DEBUG
            Service.Loc.StoreLanguage();
#endif
            GC.SuppressFinalize(this);
        }

        private void OnCommand(string command, string args)
        {
            if (args == "update"|| args == "")
            {
                this.GetDataFromXivCommon();
            }
            else if (args.StartsWith("search"))
            {
                var temp = args.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                string targetName;
                if (temp.Length == 2)
                {
                    targetName = temp[1];
                }
                else if (temp.Length == 3)
                {
                    targetName = temp[1] + " " + temp[2];
                }
                else
                {
                    Service.Chat.PrintError(string.Format(Service.Loc.Localize("Parameter error, length is '{0}'"),temp.Length));
                    return;
                }
                this.SearchPlayerResult(targetName);
            }
            else if (args.StartsWith("nick"))
            {
                //  "PalyerName nickname", "Palyer Name nick name", "Palyer Name nickname"
                string[] parseName = ParseNameText(args.Substring(4));
                string targetName = parseName[0];
                string nickName = parseName[1];
                this.AddNickName(targetName, nickName);
            }
            else if (args.StartsWith("config"))
            {
                this.DrawConfigUI();
            }
            else
            {
                Service.Chat.PrintError(Service.Loc.Localize($"Invalid parameter: ")+args);
            }
        }


        private void DrawUI()
        {
            Service.PluginUi.Draw();
        }

        internal void DrawMainUI()
        {
            Service.PluginUi.Visible = true;
        }
        private void DrawConfigUI()
        {
            Service.PluginUi.SettingsVisible = true;
        }

        private unsafe void GetSocialListDetour(uint targetId, IntPtr data)
        {
            var socialList = Marshal.PtrToStructure<SocialListResult>(data);
            string listType = socialList.ListType.ToString();
            PluginLog.Debug($"CommunityID:{socialList.CommunityID:X}");
            PluginLog.Debug($"ListType:{socialList.ListType:X}");
            // type: 1 = Party List; 2 = Friend List; 3 = Linkshells 4 = Player Search;
            // 5 = Members Online and on Home World; 6 = company member; 7 = Application of Company;
            // 10 = Mentor;11 = New Adventurer/Returner; 
            string[] knownType = { "1", "2", "3", "4", "5", "6", "7", "10", "11" };
            if (!knownType.Contains(listType))
            {
#if DEBUG
                Service.Chat.Print($"UsedName: Find Unknown type: {listType}");
#endif
                return;
            }

            string[] acceptType = { "1", "2", "4" };

            if ((listType == "1" && !Service.Configuration.UpdateFromPartyList) ||
                (listType == "2" && !Service.Configuration.UpdateFromFriendList) ||
                (listType == "4" && !Service.Configuration.UpdateFromPlayerSearch) ||
                !acceptType.Contains(listType))
            {
                return;
            }
            var result = new Dictionary<ulong, string>();
            foreach (var c in socialList.CharacterEntries)
            {
                if (c.CharacterID == 0 || c.CharacterID == Service.ClientState.LocalContentId)
                    continue;
                if (!result.TryAdd(c.CharacterID, c.CharacterName))
                {
                    PluginLog.LogWarning($"Duplicate entry {c.CharacterID} {c.CharacterName}");
                }
            }
            UpdatePlayerNames(result);
            Service.GetSocialListHook?.Original(targetId, data);
        }

        internal void GetDataFromXivCommon()
        {
            var friendList = Service.Common.Functions.FriendList.List.GetEnumerator();
            IDictionary<ulong, string> currentPlayersList = new Dictionary<ulong, string>();
            while (friendList.MoveNext())
            {
                var player = friendList.Current;
                var contentId = player.ContentId;
                var name = player.Name.ToString();
                try
                {
                    currentPlayersList.Add(contentId, name);
                }
                catch (ArgumentException e)
                {
                    PluginLog.Warning($"{e}");
                    PluginLog.Warning($"Unknown problem at {name}-{contentId}");
                    Service.Chat.PrintError(Service.Loc.Localize($"Update Player List Fail\nMay cause by incompatible version of XivCommon\nPlease contact to developer"));
                    return;
                }

            }
            UpdatePlayerNames(currentPlayersList);
        }

        internal void UpdatePlayerNames(IDictionary<ulong, string> currentPlayersList, bool showHint=true)
        {
            var savedFriendList = Service.Configuration.playersNameList;
            bool same = true;

            foreach (var player in currentPlayersList)
            {
                var contentId = player.Key;
                var name = player.Value;
                if (savedFriendList.ContainsKey(contentId))
                {
                    if (!savedFriendList[contentId].currentName.Equals(name))
                    {
                        same = false;
                        if (Service.Configuration.ShowNameChange)
                        {
                            var temp = string.IsNullOrEmpty(savedFriendList[contentId].nickName) ? savedFriendList[contentId].currentName : $"({savedFriendList[contentId].nickName})";
                            Service.Chat.Print(temp + Service.Loc.Localize($" changed name to ") + $"{name}");
                        }
                        savedFriendList[contentId].usedNames.Add(savedFriendList[contentId].currentName);
                        savedFriendList[contentId].currentName = name;
                    }
                }
                else
                {
                    same = false;
                    savedFriendList.Add(contentId, new Configuration.PlayersNames(name, "", new List<string> { }));
                }

            }
            if (!same)
            {
                Service.Configuration.playersNameList = savedFriendList;
                Service.Configuration.storeNames();
            }
            if (showHint)
            {
                Service.Chat.Print(Service.Loc.Localize("Update FriendList completed"));
            }
        }

        public IDictionary<ulong, Configuration.PlayersNames> SearchPlayer(string targetName, bool useNickName=false)
        {
            var result = new Dictionary<ulong, Configuration.PlayersNames>();
            targetName = targetName.ToLower();
            foreach (var player in Service.Configuration.playersNameList)
            {
                var current = player.Value.currentName.ToLower();
                var nickNmae = player.Value.nickName.ToLower();
                if (current.Equals(targetName) || (useNickName && nickNmae.ToLower().Equals(targetName)) || player.Value.usedNames.Any(name => name.Equals(targetName)))
                {
                    result.Add(player.Key, player.Value);
                }
            }
            return result;
        }

        public string SearchPlayerResult(string targetName)
        {
            string result = "";
            foreach (var player in SearchPlayer(targetName, true))
            {
                var temp = string.IsNullOrEmpty(player.Value.nickName) ? "" : "(" + player.Value.nickName + ")";
                result += $"{player.Value.currentName}{temp}: [{string.Join(",", player.Value.usedNames)}]\n";
            }
            Service.Chat.Print(string.Format(Service.Loc.Localize("Search result(s) for target [{0}]:"), targetName)+$"\n{result}");
            return result;
        }
        
        public XivCommon.Functions.FriendList.FriendListEntry GetPlayerByNameFromFriendList(string name)
        {
            var friendList = Service.Common.Functions.FriendList.List.GetEnumerator();
            while (friendList.MoveNext())
            {
                var player = friendList.Current;
                if (player.Name.ToString().Equals(name))
                {
                    return player;
                }
            }
            return new XivCommon.Functions.FriendList.FriendListEntry();
        }

        private void AddNickName(string playerName, string nickName)
        {
            var player = SearchPlayer(playerName);
            if (player.Count == 0)
            {
                Service.Chat.PrintError(string.Format(Service.Loc.Localize("Cannot find player '{0}', Please try using '/pname update' to update FriendList, or check the spelling"), playerName));
                return;
            }
            if (player.Count > 1)
            {
                Service.Chat.PrintError(string.Format(Service.Loc.Localize("Find multiple '{0}', please search for players using the exact name"), playerName));
                return;
            }
            Service.Configuration.playersNameList[player.First().Key].nickName = nickName;
            Service.Configuration.storeNames();
            Service.Chat.Print(string.Format(Service.Loc.Localize("The nickname of {0} has been set to {1}"), playerName, nickName));
        }
        // name from command, try to solve "Palyer Name nick name", "Palyer Name nickname", not support "PalyerName nick name"
        public string[] ParseNameText(string text)
        {
            var playerName = "";
            var nickName = "";
            var name = text.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (name.Length == 4)
            {
                playerName = name[0] + " " + name[1];
                nickName = name[2] + " " + name[3];
            }
            else if (name.Length == 3)
            {
                playerName = name[0] + " " + name[1];
                nickName = name[2];
            }
            else if (name.Length == 2)
            {
                playerName = name[0];
                nickName = name[1];
            }
            else if (name.Length == 1)
            {
                playerName = name[0];
            }

            return new string[] { playerName, nickName };

        }
    }
}
