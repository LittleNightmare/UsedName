using Dalamud.Game.Command;
using System;

namespace UsedName
{
    public class Commands : IDisposable
    {
        private const string CommandName = "/pname";

        public Commands()
        {
            Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = Service.Loc.Localize("Use '/pname' or '/pname update' to update data from FriendList\n") +
                Service.Loc.Localize("Use '/pname main' to open Main window\n") +
                Service.Loc.Localize("Use '/pname sub' show plugin's Subscription window\n") +
                Service.Loc.Localize("Use '/pname search firstname lastname' to search 'firstname lastname's used name. I **recommend** using the right-click menu to search\n") +
                Service.Loc.Localize("Use '/pname nick firstname lastname nickname' set 'firstname lastname's nickname to 'nickname'\n") +
                Service.Loc.Localize("(Format require:first last nickname; first last nick name)\n") +
                Service.Loc.Localize("Use '/pname config' show plugin's setting")
            });
        }

        public void Dispose()
        {
            Service.CommandManager.RemoveHandler(CommandName);
        }

        public void OnCommand(string command, string args)
        {
            if (args == "update" || args == "")
            {
                Service.GameDataManager.UpdateDataFromXivCommon();
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
                    Service.Chat.PrintError(string.Format(Service.Loc.Localize("Parameter error, length is '{0}'"), temp.Length));
                    return;
                }
                Service.PlayersNamesManager.SearchPlayerResult(targetName);
            }
            else if (args.StartsWith("nick"))
            {
                //  "PalyerName nickname", "Palyer Name nick name", "Palyer Name nickname"
                string[] parseName = ParseNameText(args.Substring(4));
                string targetName = parseName[0];
                string nickName = parseName[1];
                Service.PlayersNamesManager.AddNickName(targetName, nickName);
            }
            else if (args.StartsWith("config"))
            {
                Service.ConfigWindow.Toggle();
            }
            else if (args.StartsWith("main"))
            {
                Service.MainWindow.Toggle();
            }
            else if (args.StartsWith("sub"))
            {
                Service.SubscriptionWindow.Toggle();
            }
            else
            {
                Service.Chat.PrintError(Service.Loc.Localize($"Invalid parameter: ") + args);
            }
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
