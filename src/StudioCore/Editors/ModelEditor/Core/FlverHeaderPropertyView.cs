﻿using ImGuiNET;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class FlverHeaderPropertyView
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelContextMenu ContextMenu;
    private ModelPropertyDecorator Decorator;

    public FlverHeaderPropertyView(ModelEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
        Decorator = screen.Decorator;
    }

    public void Display()
    {
        var entry = Screen.ResManager.GetCurrentFLVER().Header;

        ImGui.Separator();
        ImGui.Text("Header");
        ImGui.Separator();

        // Variables
        var bigEndian = entry.BigEndian;
        var version = entry.Version;
        var bbMin = entry.BoundingBoxMin;
        var bbMax = entry.BoundingBoxMax;
        var unicode = entry.Unicode;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Big Endian");
        UIHelper.ShowHoverTooltip("If true FLVER will be written big-endian, if false little-endian.");
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Version");
        UIHelper.ShowHoverTooltip("Version of the format indicating presence of various features.");
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Bounding Box: Minimum");
        UIHelper.ShowHoverTooltip("Minimum extent of the entire model.");
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Bounding Box: Maximum");
        UIHelper.ShowHoverTooltip("Maximum extent of the entire model.");
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Unicode");
        UIHelper.ShowHoverTooltip("If true strings are UTF-16, if false Shift-JIS.");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox("##BigEndian", ref bigEndian);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.BigEndian != bigEndian)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERHeader_BigEndian(entry, entry.BigEndian, bigEndian));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##Version", ref version);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Version != version)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERHeader_Version(entry, entry.Version, version));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3("##BoundingBoxMin", ref bbMin);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.BoundingBoxMin != bbMin)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERHeader_BoundingBoxMin(entry, entry.BoundingBoxMin, bbMin));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3("##BoundingBoxMax", ref bbMax);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.BoundingBoxMax != bbMax)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERHeader_BoundingBoxMax(entry, entry.BoundingBoxMax, bbMax));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox("##Unicode", ref unicode);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Unicode != unicode)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERHeader_Unicode(entry, entry.Unicode, unicode));
        }

        ImGui.Columns(1);
    }
}