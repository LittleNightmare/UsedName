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

        private IDictionary<ulong, Configuration.PlayersNames> playersNameList;

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

            this.playersNameList = Configuration.playersNameList;
            this.Common = new XivCommonBase();
            this.Chat = chatGUI;
            this.ContextMenu = new ContextMenu(this);


            // you might normally want to embed resources and load them from the manifest stream
            this.PluginUi = new PluginUI(this.Configuration);

            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "使用'/pname update'更新好友列表\n" +
                "使用'/pname search xxxx'搜索xxxx的曾用名(不想支持昵称)\n" +
                "使用'/pname nick xxxx aaaa'设置xxxx的昵称为aaaa，仅支持好友"
            });

            // first time
            if (Configuration.playersNameList.Count <= 0)
            {
                this.UpdatePlayerNames();
            }

            // this.PluginInterface.UiBuilder.Draw += DrawUI;
            // this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

        }

        public void Dispose()
        {
            // this.PluginUi.Dispose();
            this.CommandManager.RemoveHandler(commandName);
            this.Common.Dispose();
            this.ContextMenu.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            if (args == "" || args == "config")
            {
                this.PluginUi.Visible = !this.PluginUi.Visible;
                Chat.Print("使用'/pname update'更新列表\n" +
                    "使用'/pname search xxxx'搜索xxxx的曾用名\n"
                    + "使用'/pname nick xxxx aaaa'设置xxxx的昵称为aaaa，仅支持好友");
            }
            else if (args == "update")
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
                    Chat.PrintError($"参数错误,长度为'{temp.Length}'");
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
            else
            {
                Chat.PrintError($"无效的参数{args}");
            }
        }


        private void DrawUI()
        {
            this.PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }

        private void UpdatePlayerNames()
        {
            var friendList = Common.Functions.FriendList.List.GetEnumerator();
            while (friendList.MoveNext())
            {
                var player = friendList.Current;
                var contentId = player.ContentId;
                var name = player.Name.ToString();
                if (this.playersNameList.ContainsKey(contentId))
                {
                    if (!this.playersNameList[contentId].currentName.Equals(name))
                    {
                        this.playersNameList[contentId].usedNames.Add(name);
                        this.playersNameList[contentId].currentName = name;
                    }
                }
                else
                {
                    this.playersNameList.Add(contentId, new Configuration.PlayersNames(name, "", new List<string> { }));
                }

            }
            this.Configuration.Save();
            Chat.Print("更新好友列表完成");
        }

        public IDictionary<ulong, Configuration.PlayersNames> SearchPlayer(string targetName, bool useNickName=false)
        {
            var result = new Dictionary<ulong, Configuration.PlayersNames>();
            foreach (var player in this.playersNameList)
            {
                var current = player.Value.currentName;
                var nickNmae = player.Value.nickName.ToLower();
                if (current.Equals(targetName) || (useNickName && nickNmae.ToLower().Equals(targetName.ToLower())) || player.Value.usedNames.Any(name => name.Equals(targetName)))
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
            Chat.Print($"目标[{targetName}]的搜索结果为:\n{result}");
            return result;
        }

        private XivCommon.Functions.FriendList.FriendListEntry GetPlayerByNameFromFriendList(string name)
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
                Chat.PrintError($"没有找到玩家{playerName}，请尝试使用'/pusedname update'更新好友列表，或检查拼写");
                return;
            }
            if (player.Count > 1)
            {
                Chat.PrintError($"找到多个玩家{playerName}，请使用准确的名字搜索玩家");
                return;
            }
            this.playersNameList[player.First().Key].nickName = nickName;
            this.Configuration.Save();
            Chat.Print($"{playerName}的昵称已经设置为{nickName}");
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

            return new string[] { playerName, nickName };

        }
    }
}
