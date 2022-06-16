using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
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
        private ChatGui Chat { get;  set; }

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }


        public UsedName(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            ChatGui chatGUI)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            this.playersNameList = Configuration.playersNameList;
            this.Common = new XivCommonBase();
            this.Chat = chatGUI;


            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);
            this.PluginUi = new PluginUI(this.Configuration, goatImage);

            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "使用'/pusedname'显示还没用的窗口\n使用'/pusedname update'更新好友列表\n使用'/pusedname search xxxx'填入当前名字搜索曾用名"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

        }

        public void Dispose()
        {
            this.PluginUi.Dispose();
            this.CommandManager.RemoveHandler(commandName);
            this.Common.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            if (args == "" || args == "config")
            {
                this.PluginUi.Visible = !this.PluginUi.Visible;
            }
            else if(args == "update")
            {
                this.UpdatePlayerNames();
            }
            else if (args.StartsWith("search"))
            {
                var targetName = args.Split(" ")[1];
                var result = this.searchPlayer(targetName);
                Chat.Print($"目标[{targetName}]的搜索结果为: {result}");
            }
        }

        private string searchPlayer(string targetName)
        {
            string result = "";
            foreach (var player in this.playersNameList)
            {
                var current = player.Value.currentName;
                if (current.ToLower().Contains(targetName.ToLower()))
                {
                    result = $"{player.Key.ToString("X")}: [{string.Join(",", player.Value.usedNames)}]";
                    break;
                }
            }
            if (result == "")
            {
                result = "没有找到玩家";
            }
            return result;
        }

        private void DrawUI()
        {
            this.PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }
        
        private void UpdatePlayerNames () {
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
                    this.playersNameList.Add(contentId, new Configuration.PlayersNames(name, new List<string> {}));
                    this.Configuration.Save();
                }

            }
        }
    }
}
