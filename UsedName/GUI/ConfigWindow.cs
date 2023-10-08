using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using UsedName.Structs;

namespace UsedName.GUI
{
    internal class ConfigWindow : Window, IDisposable
    {
        public ConfigWindow() : base(
            "Used Name Settings", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(375, 330),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public override void Draw()
        {
            if (ImGui.Button(Service.Loc.Localize("Update FriendList")))
            {
                Service.GameDataManager.UpdateDataFromXivCommon();
            }
            ImGui.SameLine();
            if (ImGui.Button(Service.Loc.Localize("Open Main Window")))
            {
                Service.MainWindow.Toggle();
            }
            ImGui.SameLine();
            if (ImGui.Button(Service.Loc.Localize("Open Subscription Window")))
            {
                Service.SubscriptionWindow.Toggle();
            }
            ImGui.Spacing();
            ImGui.Text(Service.Loc.Localize("Language:"));
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            if (ImGui.BeginCombo("##Language", Service.Configuration.Language))
            {
                var languages = Service.Loc.GetLanguages();
                foreach (var lang in languages)
                {
                    if (ImGui.Selectable(lang, lang == Service.Configuration.Language))
                    {
                        Service.Configuration.Language = lang;
                        Service.Loc.LoadLanguage(lang);
                        Service.Configuration.Save();
                    }
                }
                ImGui.EndCombo();
            }

            // checkbox EnableAutoUpdate
            if (ImGui.Checkbox(Service.Loc.Localize("Enable Auto Update"), ref Service.Configuration.EnableAutoUpdate))
            {
                Service.Configuration.Save();
                if (Service.Configuration.EnableAutoUpdate)
                {
                    Service.GameDataManager.GetSocialListHook?.Enable();
                }
                else
                {
                    Service.GameDataManager.GetSocialListHook?.Disable();
                }
            }
            var typeNames = Enum.GetNames(typeof(ListType)).Select(Service.Loc.Localize);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(Service.Loc.Localize("Automatically record the checked social list.\nOptionally visible when checked.\nAnd automatically get updates\nwhen the following lists are opened.")+ "\n\n" + string.Join("\n",typeNames));
            }
            if (Service.Configuration.EnableAutoUpdate)
            {
                ImGui.Spacing();
                ImGui.Indent();
                if (ImGui.Checkbox(Service.Loc.Localize("Update From PartyList"), ref Service.Configuration.UpdateFromPartyList))
                {
                    Service.Configuration.Save();
                }
                if (ImGui.Checkbox(Service.Loc.Localize("Update From FriendList"), ref Service.Configuration.UpdateFromFriendList))
                {
                    Service.Configuration.Save();
                }
                if (ImGui.Checkbox(Service.Loc.Localize("Update From CompanyMember"), ref Service.Configuration.UpdateFromCompanyMember))
                {
                    Service.Configuration.Save();
                }
                if (ImGui.Checkbox(Service.Loc.Localize("Update From PlayerSearch"), ref Service.Configuration.UpdateFromPlayerSearch))
                {
                    Service.Configuration.Save();
                }
                if (ImGui.Checkbox(Service.Loc.Localize("Enable Subscription"), ref Service.Configuration.EnableSubscription))
                {
                    Service.Configuration.Save();
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(Service.Loc.Localize("Add a new subscription list.\nDuring updates, if a subscribed player's name exists,\nthis player will be added to the plugin's stored players'name list")+"\n"+
                        Service.Loc.Localize("Players in the subscription list will be\nautomatically removed after successful capture of information\nor cleared when closing the game"));
                }
                if (Service.Configuration.EnableSubscription)
                {
                    ImGui.Spacing();
                    ImGui.Indent();
                    ImGui.TextUnformatted(Service.Loc.Localize("Subscription String"));
                    ImGui.SameLine();
                    if (ImGui.InputText("##SubscriptionString", ref Service.Configuration.SubscriptionString, 15))
                    {
                        Service.Configuration.Save();
                    }
                    ImGui.Unindent();
                }
                ImGui.Unindent();
            }

            if (ImGui.Checkbox(Service.Loc.Localize("Name Change Check"), ref Service.Configuration.ShowNameChange))
            {
                Service.Configuration.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(Service.Loc.Localize("Show player who changed name when update FriendList"));
            }
            if (ImGui.Checkbox(Service.Loc.Localize("Enable Search In Context"), ref Service.Configuration.EnableSearchInContext))
            {
                Service.Configuration.Save();
            }

            if (Service.Configuration.EnableSearchInContext)
            {
                ImGui.Spacing();
                ImGui.Indent();
                ImGui.TextUnformatted(Service.Loc.Localize("Search in Context String"));
                ImGui.SameLine();
                if (ImGui.InputText("##SearchInContextString", ref Service.Configuration.SearchString, 15))
                {
                    Service.Configuration.Save();
                }
                ImGui.Unindent();

            }
            if (ImGui.Checkbox(Service.Loc.Localize("Enable Add Nick Name"), ref Service.Configuration.EnableAddNickName))
            {
                Service.Configuration.Save();
            }
            if (Service.Configuration.EnableAddNickName)
            {
                ImGui.Spacing();
                ImGui.Indent();
                ImGui.TextUnformatted(Service.Loc.Localize("Add Nick Name String"));
                ImGui.SameLine();
                if (ImGui.InputText("##AddNickNameString", ref Service.Configuration.AddNickNameString, 15))
                {
                    Service.Configuration.Save();
                }
                ImGui.Unindent();
            }
            if(ImGui.Checkbox(Service.Loc.Localize("Modify Store Path"), ref Service.Configuration.modifyStorePath))
            {
                Service.Configuration.Save();
            }
            if (Service.Configuration.modifyStorePath)
            {
                ImGui.Spacing();
                ImGui.Indent();
                ImGui.TextUnformatted(Service.Loc.Localize("Store Path:"));
                ImGui.SameLine();
                var storePath = Service.Configuration.storeNamesPath;
                if (ImGui.InputText("##StorePath", ref storePath, 260))
                {
                    if (storePath != Service.Configuration.storeNamesPath &&
                        Directory.Exists(Path.GetDirectoryName(storePath)))
                    {
                        Service.Configuration.storeNamesPath = storePath;
                        Service.Configuration.Save(true);
                    }
                    else
                    {
                        storePath = Service.Configuration.storeNamesPath;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button(Service.Loc.Localize("Reset Path")))
                {
                    var path = Path.Join(Service.PluginInterface.ConfigDirectory.FullName, "storeNames.json");
                    if (!Directory.Exists(Path.GetDirectoryName(path)))
                    {
                        Service.PluginInterface.ConfigDirectory.Create();
                    }
                    Service.Configuration.storeNamesPath = path;
                    Service.Configuration.modifyStorePath = false;
                    Service.Configuration.Save(true);

                }
                ImGui.Unindent();
            }
        }
    }
}
