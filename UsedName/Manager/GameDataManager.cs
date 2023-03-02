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
                this.GetDataFromXivCommon();
            }
        }
        public void Dispose()
        {
            this.GetSocialListHook?.Disable();
            this.GetSocialListHook?.Dispose();
        }
        private readonly string[] KnowType = { "1", "2", "3", "4", "5", "6", "7", "10", "11" };
        private readonly string[] AcceptType = { "1", "2", "4" };
        private unsafe void GetSocialListDetour(uint targetId, IntPtr data)
        {
            var socialList = Marshal.PtrToStructure<SocialListResult>(data);
            this.GetSocialListHook?.Original(targetId, data);
            string listType = socialList.ListType.ToString();
            PluginLog.Debug($"CommunityID:{socialList.CommunityID:X}:{socialList.Index}:{socialList.NextIndex}:{socialList.RequestKey}:{socialList.RequestParam}");
            PluginLog.Debug($"ListType:{socialList.ListType:X}");
            // type: 1 = Party List; 2 = Friend List; 3 = Linkshells 4 = Player Search;
            // 5 = Members Online and on Home World; 6 = company member; 7 = Application of Company;
            // 10 = Mentor;11 = New Adventurer/Returner; 

            if (!KnowType.Contains(listType))
            {
#if DEBUG
                Service.Chat.Print($"UsedName: Find Unknown type: {listType}");
#endif
                return;
            }


            if ((listType == "1" && !Service.Configuration.UpdateFromPartyList) ||
                (listType == "2" && !Service.Configuration.UpdateFromFriendList) ||
                (listType == "4" && !Service.Configuration.UpdateFromPlayerSearch) ||
                !AcceptType.Contains(listType))
            {
                return;
            }
            var result = new Dictionary<ulong, string>();
            foreach (var c in socialList.CharacterEntries)
            {
                if (c.CharacterID == 0 ||
                    c.CharacterID == Service.ClientState.LocalContentId ||
                    c.CharacterName.IsNullOrEmpty())
                    continue;
                if (!result.TryAdd(c.CharacterID, c.CharacterName))
                {
                    PluginLog.LogWarning($"Duplicate entry {c.CharacterID} {c.CharacterName}");
                }
            }
            Service.PlayersNamesManager.UpdatePlayerNames(result, false);

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
