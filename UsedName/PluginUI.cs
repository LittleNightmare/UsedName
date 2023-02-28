using Dalamud.Logging;
using Dalamud.Utility;
using ImGuiNET;
using System;
using System.Linq;
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
        private bool edittingVisible = false;
        public bool EdittingVisible
        {
            get { return this.edittingVisible; }
            set { this.edittingVisible = value; }
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
            DrawEdittingWindow();
            DrawSettingsWindow();
        }

        private string[] tableColum = new string[]
        {
            "CurrentName","NickName","FirstUsedName","ShowMoreUsedName","Edit","Remove"
        };

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }
            ImGui.SetNextWindowSize(new Vector2(550, 410), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Used Name", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (ImGui.Button(Service.Loc.Localize("Setting")))
                {
                    this.SettingsVisible = !this.SettingsVisible;
                }
                ImGui.SameLine();
                ImGui.Text(Service.Loc.Localize("Search:"));
                ImGui.SameLine();
                ImGui.SetNextItemWidth(200);
                var searchContent = "";
                ImGui.InputTextWithHint("",Service.Loc.Localize("Enter player's name here"), ref searchContent, 250);

                if (ImGui.BeginTable("SocialList", tableColum.Length, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable))
                {
                    foreach (var t in tableColum)
                    {
                        var c = Service.Loc.Localize(t);
                        ImGui.TableSetupColumn(c, ImGuiTableColumnFlags.None, c.Length);
                    }
                    ImGui.TableHeadersRow();
                    var index = 0;
                    foreach(var (id, player) in Service.Configuration.playersNameList.Where(item => item.Value.currentName.Contains(searchContent)||
                                                                                                    item.Value.nickName.Contains(searchContent)||
                                                                                                    item.Value.usedNames.Any(u => u.Contains(searchContent))))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text(player.currentName);
                        ImGui.TableNextColumn();
                        ImGui.Text(player.nickName);
                        ImGui.TableNextColumn();
                        var firstUsedName = player.usedNames.Where(n => !n.IsNullOrEmpty()).ToList().Count>=1 ? player.usedNames.Where(n=>!n.IsNullOrEmpty()).First() : "";
                        ImGui.Text(firstUsedName);
                        ImGui.TableNextColumn();
                        if (ImGui.Button(Service.Loc.Localize("Show") +$"##{index}"))
                        {
                            var temp = string.IsNullOrEmpty(player.nickName) ? "" : "(" + player.nickName + ")";
                            Service.Chat.Print($"{player.currentName}{temp}: [{string.Join(",", player.usedNames)}]");
                        }
                        ImGui.TableNextColumn();
                        if (ImGui.Button(Service.Loc.Localize("Edit") + $"##{index}"))
                        {
                            Service.TempPlayerName = player.currentName;
                            Service.TempPlayerID = id;
                            this.EdittingVisible = true;
                        }
                        ImGui.TableNextColumn();
                        if (ImGui.Button(Service.Loc.Localize("Remove") + $"##{index}") &&ImGui.IsKeyPressed(ImGuiKey.LeftCtrl))
                        {
                            this.plugin.RemovePlayer(id);
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip(Service.Loc.Localize("Holding LeftCtrl to Remove this record. It will be re-added on update base on your setting,\nbut will not contain the previous data (e.g. used names, nickname)"));
                        }
                        ImGui.TableNextColumn();
                        index++;
                    }
                }
                ImGui.EndTable();

            }

        }

        public void DrawEdittingWindow()
        {
            if (!EdittingVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin(Service.Loc.Localize("Used Name: Edit nick name"), ref this.edittingVisible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.Text(Service.TempPlayerName + Service.Loc.Localize("'s current nick name:"));
                var target = this.plugin.GetPlayerByNameFromFriendList(Service.TempPlayerName);
                bool isEmpty = target.Equals(new XivCommon.Functions.FriendList.FriendListEntry());
                ulong targetID = isEmpty? Service.TempPlayerID : target.ContentId;
                if (Service.Configuration.playersNameList.TryGetValue(targetID, out var tar1) && tar1.currentName == Service.TempPlayerName)
                {
                    var nickName = Service.Configuration.playersNameList[targetID].nickName;
                    // var nickName = target.nickName;
                    if (ImGui.InputText("##CurrentNickName", ref nickName, 250))
                    {
                        Service.Configuration.playersNameList[targetID].nickName = nickName;
                        Service.Configuration.storeNames();
                    }
                }
                else
                {
                    ImGui.Text(String.Format(Service.Loc.Localize("NO PLAYER FOUND. Please makesure {0} is your friend.\nThen, try update FriendList"), Service.TempPlayerName));
                    ImGui.Spacing();
                    if (ImGui.Button(Service.Loc.Localize("Update FriendList")))
                    {
                        this.plugin.GetDataFromXivCommon();
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
                    this.plugin.GetDataFromXivCommon();
                }
                ImGui.SameLine();
                if (ImGui.Button(Service.Loc.Localize("Open Main Window")))
                {
                    this.Visible= !this.Visible;
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
                if(ImGui.Checkbox(Service.Loc.Localize("Enable Auto Update"), ref Service.Configuration.EnableAutoUpdate))
                {
                    Service.Configuration.Save();
                    if (Service.Configuration.EnableAutoUpdate)
                    {
                        Service.GetSocialListHook?.Enable();
                    }else
                    {
                        Service.GetSocialListHook?.Disable();
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
                ImGui.End();
            }
        }
    }
}
