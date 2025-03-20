using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

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

    // Editing Window is opened by source which could provide 100% accurate ContentId
    public bool TrustOpen = false;

    public override void Draw()
    {
        ImGui.Text(Service.PlayersNamesManager.TempPlayerName + "'s current nick name:".Loc());

        ulong targetID = Service.PlayersNamesManager.TempPlayerID;
        if (Service.Configuration.playersNameList.TryGetValue(targetID, out var tar1) && tar1.currentName == Service.PlayersNamesManager.TempPlayerName)
        {
            if (!TrustOpen)
            {
                // if not trust open, warning user that this is not 100% accurate
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "WARNING: There may be other players with the same name\nPlease verify target before editing".Loc());
            }
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
            ImGui.Text(String.Format("NO PLAYER FOUND. Please makesure {0} is your friend.\nThen, try update FriendList".Loc(), Service.PlayersNamesManager.TempPlayerName));
            ImGui.Spacing();
            if (ImGui.Button("Update FriendList".Loc()))
            {
                //Service.GameDataManager.UpdateDataFromXivCommon();
                Service.Chat.Print("The current function is not available");
            }
        }
    }
}