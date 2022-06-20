using System;
using System.Linq;
using Dalamud.ContextMenu;
using UsedName;
using Lumina.Excel.GeneratedSheets;

namespace UsedName;

public class ContextMenu : IDisposable
{
    private readonly UsedName plugin;
    public ContextMenu(UsedName Plugin)
    {
        this.plugin = Plugin;
        Enable();
    }

    public void Enable()
    {
        plugin.ContextMenuBase.Functions.ContextMenu.OnOpenGameObjectContextMenu -= OnOpenContextMenu;
        plugin.ContextMenuBase.Functions.ContextMenu.OnOpenGameObjectContextMenu += OnOpenContextMenu;
    }

    public void Disable()
    {
        plugin.ContextMenuBase.Functions.ContextMenu.OnOpenGameObjectContextMenu -= OnOpenContextMenu;
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
        
        if (plugin.Configuration.EnableSearchInContext)
        {
            args.AddCustomItem(new GameObjectContextMenuItem(plugin.Configuration.SearchString, Search));
        }
        
        if (plugin.Configuration.EnableAddNickName)
        {
            args.AddCustomItem(new GameObjectContextMenuItem(plugin.Configuration.AddNickNameString, AddNickName));
        }
    }

    private void AddNickName(GameObjectContextMenuItemSelectedArgs args)
    {
        plugin.tempPlayerName = args.Text.ToString();
        plugin.DrawMainUI();
    }

    private void Search(GameObjectContextMenuItemSelectedArgs args)
    {
        if (!IsMenuValid(args))
            return;
        var target = args.Text.ToString();
        if (!string.IsNullOrEmpty(target))
        {
            plugin.SearchPlayerResult(args.Text.ToString());
        }
        else
        {
            plugin.Chat.PrintError("Cannot find");
        }
        
    }
}