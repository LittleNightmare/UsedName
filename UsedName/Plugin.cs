using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.ContextMenu;
using Dalamud.Interface.Internal.Notifications;
using XivCommon;
using System.Collections.Generic;
using System.Linq;

namespace UsedName
{
    public sealed class UsedName : IDalamudPlugin
    {

        public string Name => "Used Name";

        private const string commandName = "/pname";

        private XivCommonBase Common { get; }
        public ChatGui Chat { get; private set; }

        public DalamudContextMenuBase ContextMenuBase { get; private set; }
        public ContextMenu ContextMenu { get; private set; }

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }
        // interrupt between UI and ContextMenu
        internal string tempPlayerName { get; set;  } = "";
        internal Localization loc { get; set; }


        public UsedName(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            ChatGui chatGUI)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.ContextMenuBase = new DalamudContextMenuBase();

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            this.Common = new XivCommonBase();
            this.Chat = chatGUI;
            this.ContextMenu = new ContextMenu(this);

            //this.loc = new Localization(this.Configuration.Language);
            this.loc = new Localization(this.Configuration.Language);

            // you might normally want to embed resources and load them from the manifest stream
            this.PluginUi = new PluginUI(this);

            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Use '/pname' or '/pname update' to update data from FriendList\n" +
                "Use '/pname search firstname lastname' to search 'firstname lastname's used name. I **recommend** using the right-click menu to search\n" +
                "Use '/pname nick firstname lastname nickname' set 'firstname lastname's nickname to 'nickname', only support player from FriendList\n" +
                "(Format require:first last nickname; first last nick name)\n" +
                "Use '/pname config' show plugin's setting"
            }) ;

            // first time
            if (this.Configuration.playersNameList.Count <= 0)
            {
                this.UpdatePlayerNames();
            }

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

        }

        public void Dispose()
        {
            this.PluginUi.Dispose();
            this.CommandManager.RemoveHandler(commandName);
            this.Common.Dispose();
            this.ContextMenu.Dispose();
#if DEBUG
            this.loc.StoreLanguage();
#endif
        }

        private void OnCommand(string command, string args)
        {
            if (args == "update"|| args == "")
            {
                this.UpdatePlayerNames();
            }
            else if (args.StartsWith("search"))
            {
                var temp = args.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                var targetName = "";
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
                    Chat.PrintError(string.Format(this.loc.Localize("Parameter error, length is '{0}'"),temp.Length));
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
                Chat.PrintError(this.loc.Localize($"Invalid parameter: ")+args);
            }
        }


        private void DrawUI()
        {
            this.PluginUi.Draw();
        }

        internal void DrawMainUI()
        {
            this.PluginUi.Visible = true;
        }
        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }

        internal void UpdatePlayerNames()
        {
            var friendList = Common.Functions.FriendList.List.GetEnumerator();
            var savedFriednList = this.Configuration.playersNameList;
            while (friendList.MoveNext())
            {
                var player = friendList.Current;
                var contentId = player.ContentId;
                var name = player.Name.ToString();
                if (savedFriednList.ContainsKey(contentId))
                {
                    if (!this.Configuration.playersNameList[contentId].currentName.Equals(name))
                    {
                        savedFriednList[contentId].usedNames.Add(name);
                        savedFriednList[contentId].currentName = name;
                        if (Configuration.ShowNameChange)
                        {
                            var temp = string.IsNullOrEmpty(savedFriednList[contentId].nickName) ? name : $"({savedFriednList[contentId].nickName})";
                            Chat.Print(temp + this.loc.Localize($" changed name to ") + $"{savedFriednList[contentId].currentName}\n");
                        }
                    }
                }
                else
                {
                    savedFriednList.Add(contentId, new Configuration.PlayersNames(name, "", new List<string> { }));
                }

            }
            this.Configuration.playersNameList = savedFriednList;
            this.Configuration.Save();
            Chat.Print(this.loc.Localize("Update FriendList completed"));
        }

        public IDictionary<ulong, Configuration.PlayersNames> SearchPlayer(string targetName, bool useNickName=false)
        {
            var result = new Dictionary<ulong, Configuration.PlayersNames>();
            targetName = targetName.ToLower();
            foreach (var player in this.Configuration.playersNameList)
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
            Chat.Print(this.loc.Localize($"Search result(s) for target ")+$"[{targetName}]:\n{result}");
            return result;
        }
        
        public XivCommon.Functions.FriendList.FriendListEntry GetPlayerByNameFromFriendList(string name)
        {
            var friendList = Common.Functions.FriendList.List.GetEnumerator();
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
                Chat.PrintError(string.Format(this.loc.Localize("Cannot find player '{0}', Please try using '/pusedname update' to update FriendList, or check the spelling"), playerName));
                return;
            }
            if (player.Count > 1)
            {
                Chat.PrintError(string.Format(this.loc.Localize("Find multiple '{0}', please search for players using the exact name"), playerName));
                return;
            }
            this.Configuration.playersNameList[player.First().Key].nickName = nickName;
            this.Configuration.Save();
            Chat.Print(string.Format(this.loc.Localize("The nickname of {0} has been set to {1}"), playerName, nickName));
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
