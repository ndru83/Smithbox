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

public class FlverBaseSkeletonPropertyView
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelContextMenu ContextMenu;
    private ModelPropertyDecorator Decorator;

    public FlverBaseSkeletonPropertyView(ModelEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
        Decorator = screen.Decorator;
    }

    public void Display()
    {
        var index = Selection._selectedBaseSkeletonBone;

        if (index == -1)
            return;

        if (Screen.ResManager.GetCurrentFLVER().Skeletons.BaseSkeleton.Count < index)
            return;

        if (Screen.ResManager.GetCurrentFLVER().Skeletons.BaseSkeleton == null)
            return;

        if (Selection.BaseSkeletonMultiselect.StoredIndices.Count > 1)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Multiple Skeleton Bones are selected.\nProperties cannot be edited whilst in this state.");
            ImGui.Separator();
            return;
        }

        ImGui.Separator();
        ImGui.Text("Standard Skeleton Hierarchy");
        ImGui.Separator();
        UIHelper.ShowHoverTooltip("Contains the standard skeleton hierarchy, which corresponds to the node hierarchy.");

        var entry = Screen.ResManager.GetCurrentFLVER().Skeletons.BaseSkeleton[index];

        int parentIndex = entry.ParentIndex;
        int firstChildIndex = entry.FirstChildIndex;
        int nextSiblingIndex = entry.NextSiblingIndex;
        int previousSiblingIndex = entry.PreviousSiblingIndex;
        int nodeIndex = entry.NodeIndex;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Parent Index");
        UIHelper.ShowHoverTooltip("Index of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("First Child Index");
        UIHelper.ShowHoverTooltip("Index of this node's first child, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Next Sibling Index");
        UIHelper.ShowHoverTooltip("Index of the next child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Previous Sibling Index");
        UIHelper.ShowHoverTooltip("Index of the previous child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Node Index");
        UIHelper.ShowHoverTooltip("Index of the node in the Node list.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##ParentIndex", ref parentIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.ParentIndex != parentIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERSkeleton_Bone_ParentIndex(entry, entry.ParentIndex, parentIndex));
        }

        Decorator.NodeIndexDecorator(parentIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##FirstChildIndex", ref firstChildIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.FirstChildIndex != firstChildIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERSkeleton_Bone_FirstChildIndex(entry, entry.FirstChildIndex, firstChildIndex));
        }

        Decorator.NodeIndexDecorator(firstChildIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NextSiblingIndex", ref nextSiblingIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.NextSiblingIndex != nextSiblingIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERSkeleton_Bone_NextSiblingIndex(entry, entry.NextSiblingIndex, nextSiblingIndex));
        }

        Decorator.NodeIndexDecorator(nextSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##PreviousSiblingIndex", ref previousSiblingIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.PreviousSiblingIndex != previousSiblingIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERSkeleton_Bone_PreviousSiblingIndex(entry, entry.PreviousSiblingIndex, previousSiblingIndex));
        }

        Decorator.NodeIndexDecorator(previousSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NodeIndex", ref nodeIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.NodeIndex != nodeIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERSkeleton_Bone_NodeIndex(entry, entry.NodeIndex, nodeIndex));
        }

        Decorator.NodeIndexDecorator(nodeIndex);

        ImGui.Columns(1);
    }
}

