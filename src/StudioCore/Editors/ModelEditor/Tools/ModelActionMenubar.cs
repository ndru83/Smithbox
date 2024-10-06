﻿using HKLib.hk2018.hkHashMapDetail;
using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Tools;

public class ModelActionMenubar
{
    private ModelEditorScreen Screen;
    private ModelActionHandler ActionHandler;

    public ModelActionMenubar(ModelEditorScreen screen)
    {
        Screen = screen;
        ActionHandler = screen.ActionHandler;
    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Actions"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Create", KeyBindings.Current.CORE_CreateNewEntry.HintText))
            {
                ActionHandler.CreateHandler();
            }
            UIHelper.ShowHoverTooltip("Adds new entry based on current selection in Model Hierarchy.");

            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Duplicate", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
            {
                ActionHandler.DuplicateHandler();
            }
            UIHelper.ShowHoverTooltip("Duplicates current selection in Model Hierarchy.");

            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Delete", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                ActionHandler.DeleteHandler();
            }
            UIHelper.ShowHoverTooltip("Deletes current selection in Model Hierarchy.");

            ImGui.EndMenu();
        }
    }

}
