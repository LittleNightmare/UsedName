using ImGuiNET;
using System;
using System.Numerics;

namespace UsedName
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private readonly UsedName plugin;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        // passing in the image here just for simplicity
        public PluginUI(UsedName Plugin)
        {
            this.plugin = Plugin;
        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
            DrawSettingsWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin(Service.Loc.Localize("Used Name: Add nick name"), ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.Text(Service.TempPlayerName + Service.Loc.Localize("'s current nick name:"));
                var target = this.plugin.GetPlayerByNameFromFriendList(Service.TempPlayerName);
                if (target.Equals( new XivCommon.Functions.FriendList.FriendListEntry())||!Service.Configuration.playersNameList.TryGetValue(target.ContentId, out _))
                {
                    ImGui.Text(String.Format(Service.Loc.Localize("NO PLAYER FOUND. Please makesure {0} is your friend.\nThen, try update FriendList"), Service.TempPlayerName));
                    ImGui.Spacing();
                    if (ImGui.Button(Service.Loc.Localize("Update FriendList")))
                    {
                        this.plugin.GetDataFromMemory();
                    }
                }
                else
                {
                    var nickName = Service.Configuration.playersNameList[target.ContentId].nickName;
                    // var nickName = target.nickName;
                    if (ImGui.InputText("##CurrentNickName", ref nickName, 15))
                    {
                        Service.Configuration.playersNameList[target.ContentId].nickName = nickName;
                        Service.Configuration.Save();
                    }
                }

            }
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(550, 410), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Used Name Settings", ref this.settingsVisible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (ImGui.Button(Service.Loc.Localize("Update FriendList")))
                {
                    this.plugin.GetDataFromMemory();
                }
                ImGui.Spacing();
                ImGui.Text(Service.Loc.Localize("Language:"));
                ImGui.SameLine();
                ImGui.SetNextItemWidth(200);
                if (ImGui.BeginCombo("##Language", Service.Configuration.Language))
                {
                    foreach (var lang in Service.Loc.GetLanguages())
                    {
                        if (ImGui.Selectable(lang.ToString()))
                        {
                            Service.Configuration.Language = lang;
                            Service.Configuration.Save();
                            Service.Loc.currentLanguage = lang;
                        }
                    }
                    ImGui.EndCombo();
                }

                // checkbox EnableAutoUpdate
                if(ImGui.Checkbox(Service.Loc.Localize("Enable Auto Update"), ref Service.Configuration.EnableAutoUpdate))
                {
                    Service.Configuration.Save();
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(Service.Loc.Localize("Automatically update player name when opening FriendList"));
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
                ImGui.Spacing();
                if (ImGui.Checkbox(Service.Loc.Localize("Auto Renew Opcode"), ref Service.Configuration.AutoCheckOpcodeUpdate))
                {
                    Service.Configuration.Save();
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(Service.Loc.Localize("Auto renew Opcode after game update, please open PartyList to renew Opcode") +
                        Service.Loc.Localize("\nIf you find that the plugin cannot update the player information automatically, it may be that the detected opcode is wrong. Please click Renew Opcode to solve it"));
                }
                ImGui.SameLine();
                if (ImGui.Button(Service.Loc.Localize("Renew Opcode")))
                {
                    this.plugin.DetectOpcode = true;
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(Service.Loc.Localize("Manually renew for Opcode updates, please open PartyList after click button immediately to renew Opcode"));
                }
                ImGui.End();
            }
        }
    }
}
