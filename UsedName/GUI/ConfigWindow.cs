using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
                Service.GameDataManager.GetDataFromXivCommon();
            }
            ImGui.SameLine();
            if (ImGui.Button(Service.Loc.Localize("Open Main Window")))
            {
                Service.MainWindow.Toggle();
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
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(Service.Loc.Localize("Automatically update the list of choices\nUpdate after opening the selected list\nThe option appears after checking"));
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
                if (ImGui.Checkbox(Service.Loc.Localize("Update From PlayerSearch"), ref Service.Configuration.UpdateFromPlayerSearch))
                {
                    Service.Configuration.Save();
                }
                ImGui.Unindent();
            }

            if (ImGui.Checkbox(Service.Loc.Localize("Name Change Check"), ref Service.Configuration.ShowNameChange))
            {
                Service.Configuration.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(Service.Loc.Localize("Show palyer who changed name when update FriendList"));
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
        }
    }
}
