using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Linq;
using System.Numerics;
using UsedName;

namespace UsedName.GUI;

public class EditingWindow : Window, IDisposable
{

    public EditingWindow() : base(
        "Used Name Editing", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
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
        ImGui.Text(Service.PlayersNamesManager.TempPlayerName + Service.Loc.Localize("'s current nick name:"));
        var target = Service.GameDataManager.GetPlayerByNameFromFriendList(Service.PlayersNamesManager.TempPlayerName);
        bool isEmpty = target.Equals(new XivCommon.Functions.FriendList.FriendListEntry());
        ulong targetID = isEmpty ? Service.PlayersNamesManager.TempPlayerID : target.ContentId;
        if (Service.Configuration.playersNameList.TryGetValue(targetID, out var tar1) && tar1.currentName == Service.PlayersNamesManager.TempPlayerName)
        {
            var nickName = Service.Configuration.playersNameList[targetID].nickName;
            // var nickName = target.nickName;
            if (ImGui.InputText("##CurrentNickName", ref nickName, 250))
            {
                Service.Configuration.playersNameList[targetID].nickName = nickName;
                Service.Configuration.StoreNames();
            }
        }
        else
        {
            ImGui.Text(String.Format(Service.Loc.Localize("NO PLAYER FOUND. Please makesure {0} is your friend.\nThen, try update FriendList"), Service.PlayersNamesManager.TempPlayerName));
            ImGui.Spacing();
            if (ImGui.Button(Service.Loc.Localize("Update FriendList")))
            {
                Service.GameDataManager.GetDataFromXivCommon();
            }
        }
    }
}