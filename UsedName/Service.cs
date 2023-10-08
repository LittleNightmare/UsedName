using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using XivCommon;
using Dalamud.ContextMenu;
using Dalamud.Interface.Windowing;
using UsedName.GUI;
using UsedName.Manager;
using Dalamud.Plugin.Services;

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
        internal static WindowSystem WindowSystem { get; set; } = null!;

        [PluginService]
        internal static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService]
        internal static ISigScanner Scanner { get; private set; } = null!;
        [PluginService]
        internal static IClientState ClientState { get; private set; } = null!;
        [PluginService]
        internal static IDataManager DataManager { get; private set; } = null!;
        [PluginService]
        internal static IChatGui Chat { get; private set; } = null!;
        [PluginService]
        internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService]
        internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
        [PluginService]
        internal static IPluginLog PluginLog { get; private set; } = null!;
    }
}