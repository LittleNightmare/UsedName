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
using UsedName.GUI;
using UsedName.Manager;

namespace UsedName
{
    internal class Service
    {
        internal static Configuration Configuration { get; set; } = null!;
        internal static Commands Commands { get; set; } = null!;
        internal static GameDataManager GameDataManager { get; set; } = null!;
        internal static PlayersNamesManager PlayersNamesManager { get; set; } = null!;
        internal static MainWindow MainWindow { get; set; } = null!;
        internal static ConfigWindow ConfigWindow { get; set; } = null!;
        internal static EditingWindow EditingWindow { get; set; } = null!;
        internal static SubscriptionWindow SubscriptionWindow { get; set; } = null!;
        internal static XivCommonBase Common { get; set; } = null!;
        internal static DalamudContextMenu ContextMenu { get; set; } = null!;
        internal static Localization Loc { get; set; } = null!;

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