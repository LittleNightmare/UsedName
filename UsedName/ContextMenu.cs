using System;
using System.Linq;
using Dalamud.ContextMenu;
using UsedName;
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
            args.AddCustomItem(new GameObjectContextMenuItem(Service.Configuration.SearchString, Search, true));
        }
        
        if (Service.Configuration.EnableAddNickName)
        {
            args.AddCustomItem(new GameObjectContextMenuItem(Service.Configuration.AddNickNameString, AddNickName, true));
        }
    }

    private void AddNickName(GameObjectContextMenuItemSelectedArgs args)
    {
        if (!IsMenuValid(args))
        {
            return;
        }
        Service.PlayersNamesManager.TempPlayerName = args.Text.ToString();
        Service.EditingWindow.IsOpen= true;
    }

    private void Search(GameObjectContextMenuItemSelectedArgs args)
    {
        if (!IsMenuValid(args))
            return;
        var target = args.Text.ToString();
        if (!string.IsNullOrEmpty(target))
        {
            Service.PlayersNamesManager.SearchPlayerResult(args.Text.ToString());
        }
        else
        {
            Service.Chat.PrintError("Cannot find");
        }
        
    }
}