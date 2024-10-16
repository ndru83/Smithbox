﻿using HKLib.hk2018.hkAsyncThreadPool;
using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class DeleteFmgEntry : EditorAction
{
    private FMG Fmg;
    private FMG.Entry Entry;
    private FMG.Entry OldEntry;
    private int InsertionIndex;

    private TextContainerInfo Info;

    public DeleteFmgEntry(TextContainerInfo info, FMG currentFmg, FMG.Entry entry)
    {
        Info = info;
        Fmg = currentFmg;
        Entry = entry;
        OldEntry = entry.Clone();
        InsertionIndex = Fmg.Entries.Count;

        for (int i = 0; i < Fmg.Entries.Count; i++)
        {
            var curEntry = Fmg.Entries[i];

            if (curEntry.ID == entry.ID)
            {
                InsertionIndex = i;
            }
        }
    }

    public override ActionEvent Execute()
    {
        Fmg.Entries.RemoveAt(InsertionIndex);
        Info.IsModified = true;

        Smithbox.EditorHandler.TextEditor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Fmg.Entries.Insert(InsertionIndex, OldEntry);
        Info.IsModified = false;

        Smithbox.EditorHandler.TextEditor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }
}