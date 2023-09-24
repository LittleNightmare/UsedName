using Dalamud;
using Dalamud.ContextMenu;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UsedName.GUI;
using UsedName.Manager;
using XivCommon;

namespace UsedName
{
    public sealed class UsedName : IDalamudPlugin
    {

        public string Name => "Used Name";

        private readonly ContextMenu contextMenu;


        public UsedName(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();
            Service.ContextMenu = new DalamudContextMenu();

            Service.Configuration = Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Service.Configuration.Initialize();

            Service.Common = new XivCommonBase();
            this.contextMenu = new ContextMenu();

            Service.Loc = new Localization();

            Service.PlayersNamesManager = new PlayersNamesManager();
            Service.GameDataManager = new GameDataManager();
            Service.Commands = new Commands();
            Service.WindowSystem = new WindowSystem("UsedName");

            Service.MainWindow = new MainWindow();
            Service.ConfigWindow = new ConfigWindow();
            Service.EditingWindow = new EditingWindow();
            Service.SubscriptionWindow = new SubscriptionWindow();
            Service.WindowSystem.AddWindow(Service.MainWindow);
            Service.WindowSystem.AddWindow(Service.ConfigWindow);
            Service.WindowSystem.AddWindow(Service.EditingWindow);
            Service.WindowSystem.AddWindow(Service.SubscriptionWindow);

            Service.PluginInterface.UiBuilder.Draw += Service.WindowSystem.Draw;
            Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

        }

        public void Dispose()
        {
            Service.Commands.Dispose();
            Service.GameDataManager.Dispose();
            Service.Common.Dispose();
            this.contextMenu.Dispose();
            Service.ContextMenu.Dispose();
            Service.Configuration.Save(storeName: true);

            Service.PluginInterface.UiBuilder.Draw -= Service.WindowSystem.Draw;
            Service.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
#if DEBUG
            Service.Loc.StoreLanguage();
#endif
            GC.SuppressFinalize(this);
        }

        private void DrawConfigUI()
        {
            Service.ConfigWindow.IsOpen = true;
        }

    }
}
