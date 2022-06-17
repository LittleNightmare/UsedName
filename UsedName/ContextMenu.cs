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
        if (plugin.Configuration.ContextMenu)
        {
            Enable();
        }
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
            //case null: // Nameplate/Model menu
            //case "LookingForGroup":
            //case "PartyMemberList":
            case "FriendList":
            //case "FreeCompany":
            //case "SocialList":
            //case "ContactList":
            //case "ChatLog":
            //case "_PartyList":
            //case "LinkShell":
            //case "CrossWorldLinkshell":
            //case "ContentMemberList": // Eureka/Bozja/...
            //case "BeginnerChatList":
                return args.Text != null && args.ObjectWorld != 0 && args.ObjectWorld != 65535;

            default:
                return false;
        }
    }

    private void OnOpenContextMenu(GameObjectContextMenuOpenArgs args)
    {
        if (!IsMenuValid(args))
            return;
        
        if (plugin.Configuration.ContextMenu)
        {
            args.AddCustomItem(new GameObjectContextMenuItem("Search Used Name", Search));
        }
    }

    private void Search(GameObjectContextMenuItemSelectedArgs args)
    {
        if (!IsMenuValid(args))
            return;
        var target = args.Text.ToString();
        if (!string.IsNullOrEmpty(target))
        {
            plugin.SearchPlayer(args.Text.ToString());
        }
        else
        {
            plugin.Chat.PrintError("Cannot find");
        }
        
    }
}