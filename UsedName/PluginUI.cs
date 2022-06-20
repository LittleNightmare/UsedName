using ImGuiNET;
using System;
using System.Numerics;

namespace UsedName
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private Configuration configuration;
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
            this.configuration = plugin.Configuration;
        }
        
        public void Dispose()
        {
            
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
            if (ImGui.Begin(this.plugin.loc.Localize("Used Name: Add nick name"), ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.Text(this.plugin.tempPlayerName + this.plugin.loc.Localize("'s current nick name:"));
                var target = this.plugin.GetPlayerByNameFromFriendList(this.plugin.tempPlayerName);
                if (target.Equals( new XivCommon.Functions.FriendList.FriendListEntry())||! this.configuration.playersNameList.TryGetValue(target.ContentId, out _))
                {
                    ImGui.Text(String.Format(this.plugin.loc.Localize("NO PLAYER FOUND. Please makesure {0} is your friend.\nThen, try update FriendList"), this.plugin.tempPlayerName));
                    ImGui.Spacing();
                    if (ImGui.Button(this.plugin.loc.Localize("Update FriendList")))
                    {
                        this.plugin.UpdatePlayerNames();
                    }
                }
                else
                {
                    var nickName = this.configuration.playersNameList[target.ContentId].nickName;
                    // var nickName = target.nickName;
                    if (ImGui.InputText("##CurrentNickName", ref nickName, 15))
                    {
                        this.configuration.playersNameList[target.ContentId].nickName = nickName;
                        this.configuration.Save();
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

            ImGui.SetNextWindowSize(new Vector2(300, 350), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Used Name Settings", ref this.settingsVisible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (ImGui.Button(this.plugin.loc.Localize("Update FriendList")))
                {
                    this.plugin.UpdatePlayerNames();
                }

                if (ImGui.Checkbox(this.plugin.loc.Localize("Name Change Check"), ref this.configuration.ShowNameChange))
                {
                    // can save immediately on change, if you don't want to provide a "Save and Close" button
                    this.configuration.Save();
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(this.plugin.loc.Localize("Show palyer who changed name when update FriendList"));
                }
                if (ImGui.Checkbox(this.plugin.loc.Localize("Enable Search In Context"), ref this.configuration.EnableSearchInContext))
                {
                    this.configuration.Save();
                }

                if (this.configuration.EnableSearchInContext)
                {
                    ImGui.Spacing();
                    ImGui.Indent();
                    ImGui.TextUnformatted(this.plugin.loc.Localize("Search in Context String"));
                    ImGui.SameLine();
                    if (ImGui.InputText("##SearchInContextString", ref this.configuration.SearchString, 15))
                    {
                        this.configuration.Save();
                    }
                    ImGui.Unindent();

                }
                if (ImGui.Checkbox(this.plugin.loc.Localize("Enable Add Nick Name"), ref this.configuration.EnableAddNickName))
                {
                    this.configuration.Save();
                }
                if (this.configuration.EnableAddNickName)
                {
                    ImGui.Spacing();
                    ImGui.Indent();
                    ImGui.TextUnformatted(this.plugin.loc.Localize("Add Nick Name String"));
                    ImGui.SameLine();
                    if (ImGui.InputText("##AddNickNameString", ref this.configuration.AddNickNameString, 15))
                    {
                        this.configuration.Save();
                    }
                    ImGui.Unindent();
                }
                ImGui.End();
            }
        }
    }
}
