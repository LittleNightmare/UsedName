using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UsedName.Manager
{
    public class PlayersNamesManager
    {
        internal string? TempPlayerName;
        internal ulong TempPlayerID;

        public List<string> Subscriptions = new List<string>();

        //public List<ulong> NotInGameFriendListFriend()
        //{
        //    var currentFriendList = Service.GameDataManager.GetDataFromXivCommon();
        //    if (currentFriendList == null) return new List<ulong>();
        //    var currentFriendListIDs = currentFriendList.Keys;
        //    var savedFriendListIDs = Service.Configuration.playersNameList.Keys;
        //    return savedFriendListIDs.Except(currentFriendListIDs).ToList();
        //}

        public void UpdatePlayerNames(IDictionary<ulong, string> currentPlayersList, IList<String> newlyAddedPlayersFromSub, bool showHint = true)
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
                            Service.Chat.Print(temp + " changed name to ".Loc() + $"{name}");
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
                Service.Configuration.StoreNames();
            }
            if (showHint)
            {
                if (newlyAddedPlayersFromSub.Count > 0)
                {
                    foreach (var player in newlyAddedPlayersFromSub)
                    {
                        Service.Chat.Print(string.Format("Successfully added {0} to plguin's player list".Loc(), player));
                    }
                }

                Service.Chat.Print("Update FriendList completed".Loc());
            }
        }

        public IDictionary<ulong, Configuration.PlayersNames> SearchPlayer(string targetName, bool useNickName = false, bool strictCompare = true)
        {
            var result = new Dictionary<ulong, Configuration.PlayersNames>();
            targetName = targetName.ToLower();
            bool Compare(string fullname, string nickName, List<string> usedNames)
            {
                if (strictCompare)
                {
                    return fullname.Equals(targetName) || (useNickName && nickName.Equals(targetName)) ||
                           usedNames.Any(name => name.Equals(targetName));
                }
                return fullname.Contains(targetName) || (useNickName && nickName.ToLower().Contains(targetName)) ||
                       usedNames.Any(name => name.Contains(targetName));
            }
            foreach (var player in Service.Configuration.playersNameList)
            {
                var current = player.Value.currentName.ToLower();
                var nickName = player.Value.nickName.ToLower();
                if (Compare(current, nickName, player.Value.usedNames))
                {
                    result.Add(player.Key, player.Value);
                }
            }
            return result;
        }

        public string SearchPlayerResult(string targetName)
        {
            StringBuilder resultBuilder = new StringBuilder();
            foreach (var player in SearchPlayer(targetName, true, false))
            {
                var temp = string.IsNullOrEmpty(player.Value.nickName) ? "" : "(" + player.Value.nickName + ")";
                resultBuilder.Append($"{player.Value.currentName}{temp}: [{string.Join(",", player.Value.usedNames)}]\n");
            }
            string result = resultBuilder.ToString();
            Service.Chat.Print(string.Format("Search result(s) for target [{0}]:".Loc(), targetName) + $"\n{result}");
            return result;
        }

        public void AddNickName(string playerName, string nickName)
        {
            var player = SearchPlayer(playerName);
            if (player.Count == 0)
            {
                Service.Chat.PrintError(string.Format("Cannot find player '{0}', Please try using '/pname update' to update FriendList, or check the spelling".Loc(), playerName));
                return;
            }
            if (player.Count > 1)
            {
                Service.Chat.PrintError(string.Format("Find multiple '{0}', please search for players using the exact name".Loc(), playerName));
                return;
            }
            Service.Configuration.playersNameList[player.First().Key].nickName = nickName;
            Service.Configuration.StoreNames();
            Service.Chat.Print(string.Format("The nickname of {0} has been set to {1}".Loc(), playerName, nickName));
        }


        public void RemovePlayer(ulong id)
        {
            Service.Chat.Print(string.Format("Remove player {0} from list".Loc(), Service.Configuration.playersNameList[id].currentName));
            Service.Configuration.playersNameList.Remove(id);
            Service.Configuration.StoreNames();
        }
    }
}
