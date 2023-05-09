﻿using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Linq;
using System.Numerics;
using UsedName;

namespace UsedName.GUI;

public class SubscriptionWindow : Window, IDisposable
{

    public SubscriptionWindow() : base(
        "Used Name Subscription", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 400),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private string _searchContent = "";

    public override void Draw()
    {
        if (ImGui.BeginTable($"SocialList##{_searchContent}", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable))
        {
            var index = 0;
            //TODO: not case sensitive
            foreach (var name in Service.PlayersNamesManager.Subscriptions.Where(x=>x.Contains(_searchContent)))
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(name);
                ImGui.TableNextColumn();
                if (ImGui.Button(Service.Loc.Localize("Remove") + $"##{index}"))
                {
                    Service.PlayersNamesManager.Subscriptions.Remove(name);
                }
                ImGui.TableNextColumn();
                index++;
            }
        }
        ImGui.EndTable();
    }
}