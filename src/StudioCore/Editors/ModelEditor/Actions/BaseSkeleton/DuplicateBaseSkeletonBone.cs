﻿using SoulsFormats;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.BaseSkeleton;

public class DuplicateBaseSkeletonBone : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.SkeletonSet.Bone DupedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public DuplicateBaseSkeletonBone(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedBaseSkeletonBone;

        CurrentFLVER = flver;
        DupedObject = CurrentFLVER.Skeletons.BaseSkeleton[index].Clone();
        Index = flver.Skeletons.BaseSkeleton.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Skeletons.BaseSkeleton.Insert(Index, DupedObject);
        Selection._selectedBaseSkeletonBone = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Selection._selectedBaseSkeletonBone = PreviousSelectionIndex;
        CurrentFLVER.Skeletons.BaseSkeleton.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
}