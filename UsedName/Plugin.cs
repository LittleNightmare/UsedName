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

namespace UsedName
{
    public sealed class UsedName : IDalamudPlugin
    {

        private IDictionary<ulong, Configuration.PlayersNames> playersNameList;

        public string Name => "Used Name";

        private const string commandName = "/pusedname";

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
                HelpMessage = "使用'/pusedname update'更新好友列表\n" +
                "使用'/pusedname search xxxx'搜索xxxx的曾用名(不想支持昵称)\n" +
                "使用'/pusedname nick xxxx aaaa'设置xxxx的昵称为aaaa，仅支持好友"
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
        }

        private void OnCommand(string command, string args)
        {
            if (args == "" || args == "config")
            {
                this.PluginUi.Visible = !this.PluginUi.Visible;
                Chat.Print("使用'/pusedname update'更新列表\n" +
                    "使用'/pusedname search xxxx'搜索xxxx的曾用名(不想支持昵称)\n"
                    + "使用'/pusedname nick xxxx aaaa'设置xxxx的昵称为aaaa，仅支持好友");
            }
            else if (args == "update")
            {
                this.UpdatePlayerNames();
            }
            else if (args.StartsWith("search"))
            {
                var targetName = args.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries)[1];
                this.SearchPlayer(targetName);
            }
            else if (args.StartsWith("nick"))
            {
                //  "palyername nickname", "palyer name nick name", "palyer name nickname"
                string[] parseName = ParseNameText(args.Substring(4));
                string targetName = parseName[0];
                string nickName = parseName[1];
                this.AddNickName(targetName, nickName);
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
                        this.Configuration.Save();
                    }
                }
                else
                {
                    this.playersNameList.Add(contentId, new Configuration.PlayersNames(name, "", new List<string> { }));
                    this.Configuration.Save();
                }

            }
        }

        public string SearchPlayer(string targetName, bool check = false)
        {
            string result = "";
            foreach (var player in this.playersNameList)
            {
                var current = player.Value.currentName;
                if (current.ToLower().Contains(targetName.ToLower()))
                {
                    var temp = string.IsNullOrEmpty(player.Value.nickName) ? player.Value.currentName : "player.Value.nickName";
                    result += $"{temp}: [{string.Join(",", player.Value.usedNames)}]\n";
                }
            }
            Chat.Print($"目标[{targetName}]的搜索结果为:\n{result}");
            return result;
        }

        private XivCommon.Functions.FriendList.FriendListEntry GetPlayerByName(string name)
        {
            var friendList = Common.Functions.FriendList.List.GetEnumerator();
            while (friendList.MoveNext())
            {
                var player = friendList.Current;
                if (player.Name.ToString().ToLower().Contains(name))
                {
                    return player;
                }
            }
            return new XivCommon.Functions.FriendList.FriendListEntry();
        }

        private void AddNickName(string playerName, string nickName)
        {
            var player = GetPlayerByName(playerName);
            if (player.Equals(new XivCommon.Functions.FriendList.FriendListEntry()))
            {
                Chat.PrintError($"没有找到玩家{playerName}，请尝试使用'/pusedname update'更新好友列表");
                return;
            }
            if (this.playersNameList.ContainsKey(player.ContentId))
            {
                this.playersNameList[player.ContentId].nickName = nickName;
                this.Configuration.Save();
            }
            else
            {
                this.playersNameList.Add(player.ContentId, new Configuration.PlayersNames(player.Name.ToString(), nickName, new List<string> { }));
                this.Configuration.Save();
            }
            Chat.Print($"{player.Name.ToString()}的昵称已经设置为{nickName}");
        }
        // name from command, try to solve "palyer name nick name", "palyer name nickname", not support "palyername nick name"
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
