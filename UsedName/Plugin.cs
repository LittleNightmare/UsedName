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
        private readonly WindowSystem windowSystem;


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

            Service.Commands = new Commands();
            Service.GameDataManager = new GameDataManager();
            Service.PlayersNamesManager =  new PlayersNamesManager();
           
            Service.MainWindow = new MainWindow();
            Service.ConfigWindow = new ConfigWindow();
            Service.EditingWindow = new EditingWindow();
            this.windowSystem= new WindowSystem("UsedName");
            this.windowSystem.AddWindow(Service.MainWindow);
            this.windowSystem.AddWindow(Service.ConfigWindow);
            this.windowSystem.AddWindow(Service.EditingWindow);
            
            Service.PluginInterface.UiBuilder.Draw += this.windowSystem.Draw;
            Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            
        }

        public void Dispose()
        {
            Service.Commands.Dispose();
            Service.GameDataManager.Dispose();
            Service.Common.Dispose();
            this.contextMenu.Dispose();
            Service.ContextMenu.Dispose();
            Service.Configuration.Save(storeName:true);

            Service.PluginInterface.UiBuilder.Draw -= this.windowSystem.Draw;
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
