using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace UsedName.GUI;

public class MainWindow : Window, IDisposable
{

    public MainWindow() : base(
        "Used Name Main", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
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
    private readonly string[] TableColum = new string[]
    {
        "CurrentName","NickName","FirstUsedName","ShowMoreUsedName","Edit","Remove"
    };

    private string _searchContent = "";

    public override void Draw()
    {
        if (ImGui.Button(Service.Loc.Localize("Setting")))
        {
            Service.ConfigWindow.Toggle();
        }
        ImGui.SameLine();
        ImGui.Text(Service.Loc.Localize("Search:"));
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200);
        ImGui.InputTextWithHint("##searchContent", Service.Loc.Localize("Enter player's name here"), ref _searchContent, 250);

        if (ImGui.BeginTable($"SocialList##{_searchContent}", TableColum.Length, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable))
        {
            foreach (var t in TableColum)
            {
                var c = Service.Loc.Localize(t);
                ImGui.TableSetupColumn(c, ImGuiTableColumnFlags.None, c.Length);
            }
            ImGui.TableHeadersRow();
            var index = 0;
            //TODO: not case sensitive
            foreach (var (id, player) in Service.PlayersNamesManager.SearchPlayer(_searchContent,true,false))
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(player.currentName);
                ImGui.TableNextColumn();
                ImGui.Text(player.nickName);
                ImGui.TableNextColumn();
                ImGui.Text(player.firstUsedname);
                ImGui.TableNextColumn();
                if (ImGui.Button(Service.Loc.Localize("Show") + $"##{index}"))
                {
                    var temp = string.IsNullOrEmpty(player.nickName) ? "" : "(" + player.nickName + ")";
                    Service.Chat.Print($"{player.currentName}{temp}: [{string.Join(",", player.usedNames)}]");
                }
                ImGui.TableNextColumn();
                if (ImGui.Button(Service.Loc.Localize("Edit") + $"##{index}"))
                {
                    Service.PlayersNamesManager.TempPlayerName = player.currentName;
                    Service.PlayersNamesManager.TempPlayerID = id;
                    Service.EditingWindow.TrustOpen = true;
                    Service.EditingWindow.IsOpen = true;
                }
                ImGui.TableNextColumn();
                if (ImGui.Button(Service.Loc.Localize("Remove") + $"##{index}") && ImGui.IsKeyPressed(ImGuiKey.LeftCtrl))
                {
                    Service.PlayersNamesManager.RemovePlayer(id);
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(Service.Loc.Localize("Holding LeftCtrl to Remove this record. It will be re-added on update base on your setting,\nbut will not contain the previous data (e.g. used names, nickname)"));
                }
                ImGui.TableNextColumn();
                index++;
            }
            ImGui.EndTable();
        }
    }
}