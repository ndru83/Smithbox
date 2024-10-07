﻿using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions.Dummy;

public class ReplaceDummyList : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private List<FLVER.Dummy> OldDummies;
    private List<FLVER.Dummy> NewDummies;

    public ReplaceDummyList(ModelEditorScreen screen, List<FLVER.Dummy> dummies)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = screen.ResManager.GetCurrentFLVER();

        OldDummies = [.. CurrentFLVER.Dummies];
        NewDummies = dummies;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Dummies = NewDummies;
        ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Dummies = OldDummies;
        ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
}