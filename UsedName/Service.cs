using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.FlyText;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Network;
using XivCommon;
using Dalamud.ContextMenu;
using Dalamud.Hooking;
using System;

namespace UsedName
{
    internal class Service
    {
        internal static Configuration Configuration { get; set; } = null!;
        internal static PluginUI PluginUi { get; set; } = null!;
        internal static XivCommonBase Common { get; set; } = null!;
        internal static DalamudContextMenu ContextMenu { get; set; } = null!;
        internal static ContextMenu ContextMenuManager { get; set; } = null!;
        internal static string? TempPlayerName;
        internal static ulong TempPlayerID;
        internal static Localization Loc { get; set; } = null!;

        internal unsafe delegate void GetSocialListDelegate(uint targetId, IntPtr SocialList);
        internal static Hook<GetSocialListDelegate> GetSocialListHook { get; set; } = null!;

        [PluginService]
        internal static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService]
        internal static SigScanner Scanner { get; private set; } = null!;
        [PluginService]
        internal static ClientState ClientState { get; private set; } = null!;
        [PluginService]
        internal static DataManager DataManager { get; private set; } = null!;
        [PluginService]
        internal static ChatGui Chat { get; private set; } = null!;
        [PluginService]
        internal static CommandManager CommandManager { get; private set; } = null!;
        [PluginService]
        internal static GameNetwork Network { get; private set; } = null!;
        
    }
}