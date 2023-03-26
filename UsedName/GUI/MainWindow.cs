using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Linq;
using System.Numerics;
using UsedName;

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

    private string searchContent = "";

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
        ImGui.InputTextWithHint("##searchContent", Service.Loc.Localize("Enter player's name here"), ref searchContent, 250);

        if (ImGui.BeginTable($"SocialList##{searchContent}", TableColum.Length, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable))
        {
            foreach (var t in TableColum)
            {
                var c = Service.Loc.Localize(t);
                ImGui.TableSetupColumn(c, ImGuiTableColumnFlags.None, c.Length);
            }
            ImGui.TableHeadersRow();
            var index = 0;
            //TODO: not case sensitive
            foreach (var (id, player) in Service.Configuration.playersNameList.Where(item =>
                         item.Value.currentName.Contains(searchContent) ||
                         item.Value.nickName.Contains(searchContent) ||
                         item.Value.usedNames.Any(u => u.Contains(searchContent))))
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(player.currentName);
                ImGui.TableNextColumn();
                ImGui.Text(player.nickName);
                ImGui.TableNextColumn();
                var firstUsedName = player.usedNames.Where(n => !n.IsNullOrEmpty()).ToList().Count >= 1 ? player.usedNames.Where(n => !n.IsNullOrEmpty()).First() : "";
                ImGui.Text(firstUsedName);
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
        }
        ImGui.EndTable();
    }
}