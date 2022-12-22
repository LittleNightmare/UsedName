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
// using Dalamud.Data;
using Dalamud.Logging;
using System.Runtime.InteropServices;
using Dalamud.Data;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using UsedName.Structures;
using Lumina.Excel.GeneratedSheets;
using static UsedName.Configuration;
using System.Text;

namespace UsedName
{
    public sealed class UsedName : IDalamudPlugin
    {

        public string Name => "Used Name";

        private const string commandName = "/pname";

        // interrupt between UI and ContextMenu

        internal bool DetectOpcode { get; set; } = false;


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
                this.GetDataFromMemory();
            }

            if (Service.Configuration.SocialListOpcode == 0)
            {
                this.DetectOpcode = true;
                //this.UpdateOpcode();
            }
            var gameVersiontext = Service.DataManager.GameData.Repositories.First(repo => repo.Key == "ffxiv").Value.Version;
            if (new GameVersion(Service.Configuration.GameVersion) < new GameVersion(gameVersiontext)&& Service.Configuration.AutoCheckOpcodeUpdate)
            {
                this.DetectOpcode = true;
            }

            Service.PluginInterface.UiBuilder.Draw += DrawUI;
            Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            Service.Network.NetworkMessage += OnNetworkEventDetectOpcode;
            Service.Network.NetworkMessage += OnNetworkEvent;


        }

        //public void UpdateOpcode()
        //{
        //    //not need anymore
        //    var lastOpcodeVersion = new GameVersion(Configuration.GameVersion);
        //    if (this.ClientState.ClientLanguage == ClientLanguage.ChineseSimplified)
        //    {
        //        var lastestOpcodeVersionCN = new GameVersion("2022.07.22.0000.0000");
        //        if (lastOpcodeVersion < lastestOpcodeVersionCN)
        //        {
        //            // 6.1
        //            Configuration.SocialListOpcode = 0x0396;
        //            Configuration.GameVersion = lastestOpcodeVersionCN.ToString();
        //        }
                
        //    }
        //    else
        //    {
        //        var lastestOpcodeVersionGlobal = new GameVersion("2022.07.08.0000.0000");
        //        if (lastOpcodeVersion < lastestOpcodeVersionGlobal)
        //        {
        //            // waiting for someone find it, now is 6.15? i guess
        //            Configuration.SocialListOpcode = 0x0303;
        //            Configuration.GameVersion = lastestOpcodeVersionGlobal.ToString();
        //        }
        //    }
        //    this.Configuration.Save();
        //}

        public void Dispose()
        {
            Service.PluginInterface.UiBuilder.Draw -= DrawUI;
            Service.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
            Service.PluginUi.Dispose();
            Service.Network.NetworkMessage -= OnNetworkEventDetectOpcode;
            Service.Network.NetworkMessage -= OnNetworkEvent;
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
                this.GetDataFromMemory();
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
        
        private void OnNetworkEvent(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            if (!Service.Configuration.EnableAutoUpdate) return;
            if (this.DetectOpcode) return;
            if (!Service.ClientState.IsLoggedIn) return;
            if (direction != NetworkMessageDirection.ZoneDown) return;
            if (Service.ClientState.LocalPlayer == null || Service.ClientState.TerritoryType == 0) return;
            if (opCode != Service.Configuration.SocialListOpcode) return;

            int size = Marshal.SizeOf(typeof(Structures.SocialList));
            try
            {
                byte[] bytes = new byte[size];
                Marshal.Copy(dataPtr, bytes, 0, size);
                GetDataFromNetwork(bytes);
            }
            catch (Exception)
            {
                throw;
            }
            
        }
        private bool IncludesBytes(byte[] source, byte[] search)
        {
            if (search == null) return false;

            for (var i = 0; i < source.Length - search.Length; ++i)
            {
                var result = true;
                for (var j = 0; j < search.Length; ++j)
                {
                    if (search[j] != source[i + j])
                    {
                        result = false;
                        break;
                    }
                }

                if (result)
                {
#if DEBUG
                    PluginLog.Debug($"Find search target at {i}");
#endif
                    return true;
                }
            }

            return false;
        }

        public void OnNetworkEventDetectOpcode(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            if (!this.DetectOpcode) return;
            if (!Service.ClientState.IsLoggedIn) return;
            if (direction != NetworkMessageDirection.ZoneDown) return;
            if (Service.ClientState.LocalPlayer == null || Service.ClientState.TerritoryType == 0) return;
            // IDK how to get length of packaget, so i ignore it
            int size = size = Marshal.SizeOf(typeof(SocialList)); ;
            byte[] bytes = new byte[size];
            try
            {
                Marshal.Copy(dataPtr, bytes, 0, size);
            }
            catch (Exception e)
            {
                PluginLog.LogError(e.Message);
            }
#if DEBUG
            // CN 6.2
            var corretOpcode = 0x0368;
            if (corretOpcode == opCode)
            {
                PluginLog.Debug("opcode is correct, copied length:" + bytes.Length);
            }
            var playerCID = Service.ClientState.LocalContentId;
            IncludesBytes(bytes, BitConverter.GetBytes(playerCID));
#endif
            //if (bytes.Length != 896) return;
            if (bytes[13-1] != 1) return;
            var playerName = Service.ClientState.LocalPlayer.Name.ToString();
            if (!IncludesBytes(bytes, System.Text.Encoding.UTF8.GetBytes(playerName))) return;
#if DEBUG
            PluginLog.Debug("Character name successfully detected");
#endif
            try
            {
                var temp = Structures.StructureReader.Read(bytes, Structures.StructureReader.StructureType.SocialList);
                if (temp[0] != "1") return;
                if (!temp.TryGetValue(Service.ClientState.LocalContentId,out var tempname)) return;
                if (!tempname.Equals(playerName)) return;
            }
            catch (Exception e)
            {
#if DEBUG
                PluginLog.Debug(e.ToString());
#endif
                return;
            }
#if DEBUG
            PluginLog.Debug("Pass memory test");
#endif

            var gameVersiontext = Service.DataManager.GameData.Repositories.First(repo => repo.Key == "ffxiv").Value.Version;
            Service.Configuration.GameVersion = gameVersiontext;
            Service.Configuration.SocialListOpcode = opCode;
            Service.Configuration.Save();
            this.DetectOpcode = false;
            Service.Chat.Print(string.Format(Service.Loc.Localize("Opcode detected\n Game version: {0}\nOpcode: {1}"), gameVersiontext, opCode));
        }

        private unsafe void GetDataFromNetwork(byte[] data)
        {
            IDictionary<ulong, string> currentPlayersList;
            currentPlayersList = Structures.StructureReader.Read(data, Structures.StructureReader.StructureType.SocialList);
            // type: 1 = Party List; 2 = Friend List; 3 = Linkshells 4 = Player Search;
            // 5 = Members Online and on Home World; 6 = company member; 7 = Application of Company;
            // 8 = New Adventurer/Returner; 9 = Mentor
            var type = currentPlayersList.TryGetValue(0, out _) ? currentPlayersList[0] : "";
            currentPlayersList.Remove(0);
  
            string[] knownType = { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            if (!knownType.Contains(type))
            {
#if DEBUG
                Service.Chat.Print($"UsedName: Find Unknown type: {type}");
#endif
                return;
            }

            string[] acceptType = { "1", "2", "4" };

            if ((type == "1" && !Service.Configuration.UpdateFromPartyList)||
                (type == "2" && !Service.Configuration.UpdateFromFriendList)||
                (type == "4" && !Service.Configuration.UpdateFromPlayerSearch)||
                !acceptType.Contains(type))
            {
                return;
            }
            // party list includes the player hiself, remove it
            currentPlayersList.Remove(Service.ClientState.LocalContentId);
#if DEBUG
            foreach (var player in currentPlayersList)
            {
                PluginLog.Debug($"{player.Key}:{player.Value}:{Service.Configuration.playersNameList.ContainsKey(player.Key)}");
            }
#endif
            this.UpdatePlayerNames(currentPlayersList, showHint: false);
        }
        
        internal void GetDataFromMemory()
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
