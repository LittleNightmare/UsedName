using System;
using System.Linq;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;
using Lumina.Excel.Sheets;

namespace UsedName;

public class ContextMenu
{
    public static void Enable()
    {
        Service.ContextMenu.OnMenuOpened -= OnOpenContextMenu;
        Service.ContextMenu.OnMenuOpened += OnOpenContextMenu;
    }

    public static void Disable()
    {
        Service.ContextMenu.OnMenuOpened -= OnOpenContextMenu;
    }

    private static bool IsMenuValid(IMenuArgs menuOpenedArgs)
    {
        if (menuOpenedArgs.Target is not MenuTargetDefault menuTargetDefault)
        {
            return false;
        }
        switch (menuOpenedArgs.AddonName)
        {
            case null: // Nameplate/Model menu
            case "LookingForGroup":
            case "PartyMemberList":
            case "FriendList":
            case "FreeCompany":
            case "SocialList":
            case "ContactList":
            case "ChatLog":
            case "_PartyList":
            case "LinkShell":
            case "CrossWorldLinkshell":
            case "ContentMemberList": // Eureka/Bozja/...
            case "BeginnerChatList":
                return menuTargetDefault.TargetName != null && menuTargetDefault.TargetHomeWorld.RowId != 0 && menuTargetDefault.TargetHomeWorld.RowId != 65535;
            case "BlackList":
                return menuTargetDefault.TargetName != string.Empty;

            default:
                return false;
        }
    }

    private static void OnOpenContextMenu(IMenuOpenedArgs menuOpenedArgs)
    {
        if (menuOpenedArgs.Target is not MenuTargetDefault menuTargetDefault)
        {
            return;
        }
        if (!IsMenuValid(menuOpenedArgs))
            return;

        if (Service.Configuration.EnableSearchInContext)
        {
            menuOpenedArgs.AddMenuItem(new MenuItem
            {
                PrefixChar = 'U',
                Name = Service.Configuration.SearchString,
                OnClicked = Search
            });
        }

        var playerName = (menuTargetDefault.TargetName ?? new SeString()).ToString();
        var playerInPluginFriendList = Service.PlayersNamesManager.SearchPlayer(playerName).Count >= 1;

        if (Service.Configuration.EnableAddNickName)
        {
            menuOpenedArgs.AddMenuItem(new MenuItem
            {
                PrefixChar = 'U',
                Name = Service.Configuration.AddNickNameString,
                OnClicked = AddNickName
            });
        }

        if (Service.Configuration.EnableSubscription && !playerInPluginFriendList &&
            !Service.PlayersNamesManager.Subscriptions.Exists(x => x == playerName))
        {
            menuOpenedArgs.AddMenuItem(new MenuItem
            {
                PrefixChar = 'U',
                Name = Service.Configuration.SubscriptionString,
                OnClicked = AddSubscription
            });
        }
    }

    private static void AddNickName(IMenuArgs args)
    {
        if (args.Target is not MenuTargetDefault menuTargetDefault)
        {
            return;
        }
        var playerName = (menuTargetDefault.TargetName ?? new SeString()).ToString();
        Service.PlayersNamesManager.TempPlayerName = playerName;
        var searchResult = Service.PlayersNamesManager.SearchPlayer(playerName);
        Service.PlayersNamesManager.TempPlayerID = searchResult.Count > 0 ? searchResult.First().Key : (ulong)0;
        Service.EditingWindow.TrustOpen = false;
        Service.EditingWindow.IsOpen = true;
    }

    private static void AddSubscription(IMenuArgs args)
    {
        if (args.Target is not MenuTargetDefault menuTargetDefault)
        {
            return;
        }
        var world = new ExcelResolver<World>(menuTargetDefault.TargetHomeWorld.RowId);
        if (world == null)
            return;
        var playerName = (menuTargetDefault.TargetName ?? new SeString()).ToString();
        if (string.IsNullOrEmpty(playerName))
            return;
        Service.PlayersNamesManager.Subscriptions.Add(playerName);
        Service.PlayersNamesManager.Subscriptions.Sort();
        Service.Chat.Print(String.Format(Service.Loc.Localize("Added {0} to subscription list"), playerName));
    }

    private static void Search(IMenuItemClickedArgs args)
    {
        if (args.Target is not MenuTargetDefault menuTargetDefault)
        {
            return;
        }
        var target = (menuTargetDefault.TargetName ?? new SeString()).ToString(); ;
        if (!string.IsNullOrEmpty(target))
        {
            Service.PlayersNamesManager.SearchPlayerResult(target);
        }
        else
        {
            Service.Chat.PrintError("Cannot find");
        }

    }
}