using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UsedName.Structs;

namespace UsedName.Manager
{
    internal class GameDataManager : IDisposable
    {
        internal unsafe delegate void GetSocialListDelegate(uint targetId, IntPtr SocialList);
        internal Hook<GetSocialListDelegate> GetSocialListHook { get; set; } = null!;
        public GameDataManager()
        {
            if (Service.Scanner.TryScanText("48 89 5c 24 ?? 56 48 ?? ?? ?? 48 ?? ?? ?? ?? ?? ?? 48 ?? ?? e8 ?? ?? ?? ?? 48 ?? ?? 48 ?? ?? 0f 84 ?? ?? ?? ?? 0f", out var ptr0))
            {
                this.GetSocialListHook = Hook<GetSocialListDelegate>.FromAddress(ptr0, GetSocialListDetour);
            }
            if (Service.Configuration.EnableAutoUpdate)
                this.GetSocialListHook?.Enable();

            // first time
            if (Service.Configuration.playersNameList.Count <= 0)
            {
                this.UpdateDataFromXivCommon();
            }
        }
        public void Dispose()
        {
            this.GetSocialListHook?.Disable();
            this.GetSocialListHook?.Dispose();
        }

        internal readonly List<ListType> AcceptType = new()
        {
            ListType.PartyList,
            ListType.FriendList,
            ListType.CompanyMember
        };
        private void GetSocialListDetour(uint targetId, IntPtr data)
        {
            var socialList = Marshal.PtrToStructure<SocialListResult>(data);
            this.GetSocialListHook?.Original(targetId, data);
            var listType = socialList.ListType;
            PluginLog.Debug($"CommunityID:{socialList.CommunityID:X}:{socialList.Index}:{socialList.NextIndex}:{socialList.RequestKey}:{socialList.RequestParam}");
            PluginLog.Debug($"ListType:{socialList.ListTypeByte:X}");
            // type: 1 = Party List; 2 = Friend List; 3 = Linkshells 4 = Player Search;
            // 5 = Members Online and on Home World; 6 = company member; 7 = Application of Company;
            // 10 = Mentor;11 = New Adventurer/Returner; 

            if (listType is null)
            {
#if DEBUG
                var hint = $"UsedName: Find Unknown type: {socialList.ListTypeByte}";
                Service.Chat.Print(hint);
                foreach (var character in socialList.CharacterEntries)
                {
                    hint += $"\n{character.CharacterID:X}:{character.CharacterName}";
                }
                PluginLog.Warning(hint);
#endif
                return;
            }


            switch (listType)
            {
                case ListType.PartyList when !Service.Configuration.UpdateFromPartyList:
                case ListType.FriendList when !Service.Configuration.UpdateFromFriendList:
                case ListType.CompanyMember when !Service.Configuration.UpdateFromCompanyMember:
                case var _ when !AcceptType.Contains((ListType)listType) && !Service.Configuration.EnableSubscription:
                    return;
            }
            var result = new Dictionary<ulong, string>();
            var notInGameFriendListFriend = Service.PlayersNamesManager.NotInGameFriendListFriend();
            var subList = Service.PlayersNamesManager.Subscriptions;
            foreach (var c in socialList.CharacterEntries)
            {
                if (c.CharacterID == 0 ||
                    c.CharacterID == Service.ClientState.LocalContentId ||
                    c.CharacterName.IsNullOrEmpty())
                    continue;
                if (AcceptType.Contains((ListType)listType) ||
                    (Service.Configuration.EnableSubscription &&
                                        (subList.RemoveAll(x => x == c.CharacterName)>0 || 
                                         notInGameFriendListFriend.Exists(id => id == c.CharacterID))))
                {
                    if (!result.TryAdd(c.CharacterID, c.CharacterName))
                    {
                        PluginLog.LogWarning($"Duplicate entry {c.CharacterID} {c.CharacterName}");
                    }
                }

            }
            Service.PlayersNamesManager.Subscriptions = subList;
            if (result.Count <= 0)
                return;
            Service.PlayersNamesManager.UpdatePlayerNames(result, false);

        }

        internal IDictionary<ulong, string>? GetDataFromXivCommon()
        {
            var friendList = Service.Common.Functions.FriendList.List;
            if (friendList.Count <= 0)
            {
                return null;
            }
            var friendListEnumerator = Service.Common.Functions.FriendList.List.GetEnumerator();
            IDictionary<ulong, string> currentPlayersList = new Dictionary<ulong, string>();
            while (friendListEnumerator.MoveNext())
            {
                var player = friendListEnumerator.Current;
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
                    return null;
                }

            }
            return currentPlayersList;
        }
        internal void UpdateDataFromXivCommon()
        {
            var currentPlayersList = GetDataFromXivCommon();
            if(currentPlayersList != null)
                Service.PlayersNamesManager.UpdatePlayerNames(currentPlayersList);
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


    }
}
