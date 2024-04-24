﻿using ImGuiNET;
using Microsoft.Extensions.Logging;
using StudioCore.Banks;
using StudioCore.Banks.AliasBank;
using StudioCore.BanksMain;
using StudioCore.Editor;
using StudioCore.Help;
using StudioCore.Interface.Tabs;
using StudioCore.Platform;
using StudioCore.Settings;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Interface.Windows;

public class ProjectWindow
{
    private bool MenuOpenState;

    public ProjectSettings ProjSettings = null;

    private ParticleTab ParticleAliasTab;
    private EventFlagTab EventFlagAliasTab;
    private MapNameTab MapAliasTab;
    private MapGroupTab MapGroupTab;
    private GparamNameTab GparamAliasTab;
    private SoundTab SoundAliasTab;

    public ProjectWindow()
    {
        ParticleAliasTab = new ParticleTab();
        EventFlagAliasTab = new EventFlagTab();
        MapAliasTab = new MapNameTab();
        MapGroupTab = new MapGroupTab();
        GparamAliasTab = new GparamNameTab();
        SoundAliasTab = new SoundTab();
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    public void Display()
    {
        var scale = Smithbox.GetUIScale();

        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(600.0f, 600.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Project##ProjectManagementWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("##ProjectTabs");

            DisplayProjectTab();
            DisplayMapGroupTab();
            DisplayMapNameTab();
            DisplayGparamNameTab();
            DisplayEventFlagTab();
            DisplayParticleTab();
            DisplaySoundTab();

            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    public void DisplayProjectTab()
    {
        if(ImGui.BeginTabItem("General"))
        {
            if (ProjSettings == null || ProjSettings.ProjectName == null)
            {
                ImGui.Text("No project loaded");
                ImguiUtils.ShowHoverTooltip("No project has been loaded yet.");
            }
            else if (TaskManager.AnyActiveTasks())
            {
                ImGui.Text("Waiting for program tasks to finish...");
                ImguiUtils.ShowHoverTooltip("Smithbox must finished all program tasks before it can load a project.");
            }
            else
            {
                ImGui.Text($"Project Name: {ProjSettings.ProjectName}");
                ImGui.Text($"Project Type: {Project.Type}");
                ImGui.Text($"Project Root Directory: {Project.GameRootDirectory}");
                ImGui.Text($"Project Mod Directory: {Project.GameModDirectory}");

                ImGui.Separator();

                if (ImGui.MenuItem("Open project settings file"))
                {
                    var projectPath = CFG.Current.LastProjectFile;
                    Process.Start("explorer.exe", projectPath);
                }

                ImGui.Separator();

                var useLoose = ProjSettings.UseLooseParams;
                if (ProjSettings.GameType is ProjectType.DS2S or ProjectType.DS3)
                {
                    if (ImGui.Checkbox("Use loose params", ref useLoose))
                        ProjSettings.UseLooseParams = useLoose;
                    ImguiUtils.ShowHoverTooltip("Loose params means the .PARAM files will be saved outside of the regulation.bin file.\n\nFor Dark Souls II: Scholar of the First Sin, it is recommended that you enable this if add any additional rows.");
                }

                var usepartial = ProjSettings.PartialParams;
                if (FeatureFlags.EnablePartialParam || usepartial)
                {
                    if (ProjSettings.GameType == ProjectType.ER &&
                    ImGui.Checkbox("Partial params", ref usepartial))
                        ProjSettings.PartialParams = usepartial;
                    ImguiUtils.ShowHoverTooltip("Partial params.");
                }
            }

            ImGui.EndTabItem();
        }
    }
    public void DisplayMapGroupTab()
    {
        if (ImGui.BeginTabItem("Map Groups"))
        {
            MapGroupTab.Display();

            ImGui.EndTabItem();
        }
    }

    public void DisplayEventFlagTab()
    {
        if (ImGui.BeginTabItem("Event Flags"))
        {
            EventFlagAliasTab.Display();

            ImGui.EndTabItem();
        }
    }

    public void DisplayMapNameTab()
    {
        if (ImGui.BeginTabItem("Map Names"))
        {
            MapAliasTab.Display();

            ImGui.EndTabItem();
        }
    }

    public void DisplayGparamNameTab()
    {
        if (ImGui.BeginTabItem("Gparam Names"))
        {
            GparamAliasTab.Display();

            ImGui.EndTabItem();
        }
    }

    public void DisplayParticleTab()
    {
        if (ImGui.BeginTabItem("Particles"))
        {
            ParticleAliasTab.Display();

            ImGui.EndTabItem();
        }
    }

    public void DisplaySoundTab()
    {
        if (ImGui.BeginTabItem("Sounds"))
        {
            SoundAliasTab.Display();

            ImGui.EndTabItem();
        }
    }
}
