﻿using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Editors.MapEditor.Actions;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Tools;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.MapEditor.Actions.ActionHandler;

namespace StudioCore.Editors.MapEditor.Tools;

public class ToolSubMenu
{
    private MapEditorScreen Screen;
    private ActionHandler Handler;

    private bool PatrolsVisualised = false;

    public ToolSubMenu(MapEditorScreen screen, ActionHandler handler)
    {
        Screen = screen;
        Handler = handler;
    }

    public void Shortcuts()
    {
        /// Toggle Patrol Route Visualisation
        if (Smithbox.ProjectType != ProjectType.DS2S && Smithbox.ProjectType != ProjectType.DS2)
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_TogglePatrolRouteRendering))
            {
                if (!PatrolsVisualised)
                {
                    PatrolsVisualised = true;
                    PatrolDrawManager.Generate(Screen.Universe);
                }
                else
                {
                    PatrolDrawManager.Clear();
                    PatrolsVisualised = false;
                }
            }
        }

        RotationIncrement.Shortcuts();
        KeyboardMovement.Shortcuts();

        //Selection Groups
        Screen.SelectionGroupEditor.SelectionGroupShortcuts();
    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            ///--------------------
            /// Color Picker
            ///--------------------
            if (ImGui.Button("Color Picker", UI.MenuButtonWideSize))
            {
                ColorPicker.ShowColorPicker = !ColorPicker.ShowColorPicker;
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.TEXTURE_ExportTexture.HintText}");

            ImGui.Separator();

            ///--------------------
            /// Toggle Editor Visibility by Tag
            ///--------------------
            if (ImGui.BeginMenu("Toggle Editor Visibility by Tag"))
            {
                ImGui.InputText("##targetTag", ref CFG.Current.Toolbar_Tag_Visibility_Target, 255);
                UIHelper.ShowHoverTooltip("Specific which tag the map objects will be filtered by.");

                if (ImGui.Button("Enable Visibility", UI.MenuButtonWideSize))
                {
                    CFG.Current.Toolbar_Tag_Visibility_State_Enabled = true;
                    CFG.Current.Toolbar_Tag_Visibility_State_Disabled = false;

                    Handler.ApplyEditorVisibilityChangeByTag();
                }
                if (ImGui.Button("Disable Visibility", UI.MenuButtonWideSize))
                {
                    CFG.Current.Toolbar_Tag_Visibility_State_Enabled = false;
                    CFG.Current.Toolbar_Tag_Visibility_State_Disabled = true;

                    Handler.ApplyEditorVisibilityChangeByTag();
                }

                ImGui.EndMenu();
            }

            ///--------------------
            /// Patrol Route Visualisation
            ///--------------------
            if (Smithbox.ProjectType != ProjectType.DS2S && Smithbox.ProjectType != ProjectType.DS2)
            {
                if (ImGui.BeginMenu("Patrol Route Visualisation"))
                {
                    if (ImGui.Button("Display", UI.MenuButtonWideSize))
                    {
                        PatrolDrawManager.Generate(Screen.Universe);
                    }
                    if (ImGui.Button("Clear", UI.MenuButtonWideSize))
                    {
                        PatrolDrawManager.Clear();
                    }

                    ImGui.EndMenu();
                }
            }

            ///--------------------
            /// Generate Navigation Data
            ///--------------------
            if (Smithbox.ProjectType is ProjectType.DES || Smithbox.ProjectType is ProjectType.DS1 || Smithbox.ProjectType is ProjectType.DS1R)
            {
                if (ImGui.BeginMenu("Navigation Data"))
                {
                    if (ImGui.Button("Generate", UI.MenuButtonWideSize))
                    {
                        Handler.GenerateNavigationData();
                    }

                    ImGui.EndMenu();
                }
            }

            ///--------------------
            /// Entity ID Checker
            ///--------------------
            if (Smithbox.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
            {
                if (ImGui.BeginMenu("Entity ID Checker"))
                {
                    if (Screen.Universe.LoadedObjectContainers != null && Screen.Universe.LoadedObjectContainers.Any())
                    {
                        if (ImGui.BeginCombo("##Targeted Map", Handler._targetMap.Item1))
                        {
                            foreach (var obj in Screen.Universe.LoadedObjectContainers)
                            {
                                if (obj.Value != null)
                                {
                                    if (ImGui.Selectable(obj.Key))
                                    {
                                        Handler._targetMap = (obj.Key, obj.Value);
                                        break;
                                    }
                                }
                            }
                            ImGui.EndCombo();
                        }

                        if (ImGui.Button("Check", UI.MenuButtonWideSize))
                        {
                            Handler.ApplyEntityChecker();
                        }
                    }

                    ImGui.EndMenu();
                }
            }

            ///--------------------
            /// Name Map Objects
            ///--------------------
            // Tool for AC6 since its maps come with unnamed Regions and Events
            if (Smithbox.ProjectType is ProjectType.AC6)
            {
                if (ImGui.BeginMenu("Rename Map Objects"))
                {
                    if (Screen.Universe.LoadedObjectContainers != null && Screen.Universe.LoadedObjectContainers.Any())
                    {
                        if (ImGui.BeginCombo("##Targeted Map", Handler._targetMap.Item1))
                        {
                            foreach (var obj in Screen.Universe.LoadedObjectContainers)
                            {
                                if (obj.Value != null)
                                {
                                    if (ImGui.Selectable(obj.Key))
                                    {
                                        Handler._targetMap = (obj.Key, obj.Value);
                                        break;
                                    }
                                }
                            }
                            ImGui.EndCombo();
                        }

                        if (ImGui.Button("Apply Names", UI.MenuButtonWideSize))
                        {
                            Handler.ApplyMapObjectNames();
                        }
                    }

                    ImGui.EndMenu();
                }
                UIHelper.ShowHoverTooltip("Applies descriptive name (based on the map object class) to map objects with blank names by default.");
            }

            ImGui.EndMenu();
        }
    }
}

