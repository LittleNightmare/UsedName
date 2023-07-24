using System;
using System.Linq;
using Dalamud.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Lumina.Excel.GeneratedSheets;

namespace UsedName;

public class ContextMenu : IDisposable
{
    public ContextMenu()
    {
        Enable();
    }

    public void Enable()
    {
        Service.ContextMenu.OnOpenGameObjectContextMenu -= OnOpenContextMenu;
        Service.ContextMenu.OnOpenGameObjectContextMenu += OnOpenContextMenu;
    }

    public void Disable()
    {
        Service.ContextMenu.OnOpenGameObjectContextMenu -= OnOpenContextMenu;
    }

    public void Dispose()
    {
        Disable();
        GC.SuppressFinalize(this);
    }

    private static bool IsMenuValid(BaseContextMenuArgs args)
    {
        switch (args.ParentAddonName)
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
                return args.Text != null && args.ObjectWorld != 0 && args.ObjectWorld != 65535;

            default:
                return false;
        }
    }

    private void OnOpenContextMenu(GameObjectContextMenuOpenArgs args)
    {
        if (!IsMenuValid(args))
            return;

        if (Service.Configuration.EnableSearchInContext)
        {
            args.AddCustomItem(
                new GameObjectContextMenuItem(new SeString(new TextPayload(Service.Configuration.SearchString)), Search,
                    true));
        }

        var playerName = (args.Text ?? new SeString()).ToString();
        var playerInPluginFriendList = Service.PlayersNamesManager.SearchPlayer(playerName).Count >= 1;
        if (Service.Configuration.EnableAddNickName)
        {
            args.AddCustomItem(new GameObjectContextMenuItem(
                new SeString(new TextPayload(Service.Configuration.AddNickNameString)), AddNickName, true));
        }

        if (Service.Configuration.EnableSubscription && !playerInPluginFriendList &&
            !Service.PlayersNamesManager.Subscriptions.Exists(x => x == playerName))
        {
            args.AddCustomItem(new GameObjectContextMenuItem(
                new SeString(new TextPayload(Service.Configuration.SubscriptionString)), AddSubscription, true));
        }
    }

    private void AddNickName(GameObjectContextMenuItemSelectedArgs args)
    {
        var playerName = (args.Text ?? new SeString()).ToString();
        Service.PlayersNamesManager.TempPlayerName = playerName;
        var searchResult = Service.PlayersNamesManager.SearchPlayer(playerName);
        Service.PlayersNamesManager.TempPlayerID = searchResult.Count > 0 ? searchResult.First().Key : (ulong)0;
        Service.EditingWindow.TrustOpen = false;
        Service.EditingWindow.IsOpen = true;
    }

    private void AddSubscription(GameObjectContextMenuItemSelectedArgs args)
    {
        var world = Service.DataManager.GetExcelSheet<World>()?.FirstOrDefault(x => x.RowId == args.ObjectWorld);
        if (world == null)
            return;
        var playerName = (args.Text ?? new SeString()).ToString();
        if (string.IsNullOrEmpty(playerName))
            return;
        Service.PlayersNamesManager.Subscriptions.Add(playerName);
        Service.PlayersNamesManager.Subscriptions.Sort();
        Service.Chat.Print(String.Format(Service.Loc.Localize("Added {0} to subscription list"), playerName));
    }

    private void Search(GameObjectContextMenuItemSelectedArgs args)
    {
        var target = (args.Text ?? new SeString()).ToString(); ;
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