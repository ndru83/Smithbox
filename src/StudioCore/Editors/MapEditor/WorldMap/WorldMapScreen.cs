﻿using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Formats;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Editors.MapEditor.WorldMap;

public class WorldMapScreen : IResourceEventListener
{
    private Task _loadingTask;
    private bool LoadedWorldMapTexture { get; set; }
    private bool WorldMapOpen { get; set; }

    private WorldMapLayout WorldMapLayout_Vanilla = null;
    private WorldMapLayout WorldMapLayout_SOTE = null;

    private Vector2 zoomFactor;

    private float zoomFactorStep = 0.1f;

    private Vector2 TextureViewWindowPosition = new Vector2(0, 0);
    private Vector2 TextureViewScrollPosition = new Vector2(0, 0);

    private Vector2 trueSize = new Vector2();
    private Vector2 size = new Vector2();
    private Vector2 relativePos = new Vector2();
    private Vector2 relativePosWindowPosition = new Vector2();

    List<string> currentHoverMaps = new List<string>();

    private bool IsViewingSOTEMap = false;

    public WorldMapScreen()
    {
        WorldMapOpen = false;
        LoadedWorldMapTexture = false;
        zoomFactor = GetDefaultZoomLevel();
        Smithbox.UIScaleChanged += (_, _) =>
        {
            zoomFactor = GetDefaultZoomLevel();
        };
    }

    public void OnProjectChanged()
    {
        if (Smithbox.ProjectType is ProjectType.ER)
        {
            LoadWorldMapTexture();
            GenerateWorldMapLayout_Vanilla();
            GenerateWorldMapLayout_SOTE();
        }
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.Map_WorldMap_Vanilla))
        {
            WorldMapOpen = !WorldMapOpen;
            if (IsViewingSOTEMap)
            {
                IsViewingSOTEMap = false;
                WorldMapOpen = true;
            };
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.Map_WorldMap_SOTE))
        {
            WorldMapOpen = !WorldMapOpen;
            if (!IsViewingSOTEMap)
            {
                IsViewingSOTEMap = true;
                WorldMapOpen = true;
            };
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.Map_WorldMap_ClearSelection))
        {
            Smithbox.EditorHandler.MapEditor.WorldMap_ClickedMapZone = null;
        }

        if (InputTracker.GetKey(Key.LControl))
        {
            HandleZoom();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.TextureViewer_ZoomReset))
        {
            zoomFactor = GetDefaultZoomLevel();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.Map_WorldMap_DragMap))
        {
            AdjustScrollNextFrame = true;
        }
    }

    public void DisplayWorldMapButton()
    {
        if (Smithbox.ProjectType != ProjectType.ER)
            return;

        var scale = Smithbox.GetUIScale();

        var windowHeight = ImGui.GetWindowHeight();
        var windowWidth = ImGui.GetWindowWidth();
        var widthUnit = windowWidth / 100;

        if (LoadedWorldMapTexture && CFG.Current.MapEditor_ShowWorldMapButtons)
        {
            if (ImGui.Button("Lands Between", new Vector2(widthUnit * 40, 20 * scale)))
            {
                WorldMapOpen = !WorldMapOpen;
                if(IsViewingSOTEMap)
                {
                    IsViewingSOTEMap = false;
                    WorldMapOpen = true;
                };
            }
            ImguiUtils.ShowHoverTooltip($"Open the Lands Between world map for Elden Ring.\nAllows you to easily select open-world tiles.\nShortcut: {KeyBindings.Current.Map_WorldMap_Vanilla.HintText}");

            ImGui.SameLine();
            if (ImGui.Button("Shadow of the Erdtree", new Vector2(widthUnit * 40, 20 * scale)))
            {
                WorldMapOpen = !WorldMapOpen;
                if (!IsViewingSOTEMap)
                {
                    IsViewingSOTEMap = true;
                    WorldMapOpen = true;
                };
            }
            ImguiUtils.ShowHoverTooltip($"Open the Shadow of the Erdtree world map for Elden Ring.\nAllows you to easily select open-world tiles.\nShortcut: {KeyBindings.Current.Map_WorldMap_SOTE.HintText}");

            ImGui.SameLine();
            if (ImGui.Button("Clear", new Vector2(widthUnit * 15, 20 * scale)))
            {
                Smithbox.EditorHandler.MapEditor.WorldMap_ClickedMapZone = null;
            }
            ImguiUtils.ShowHoverTooltip($"Clear the current world map selection (if any).\nShortcut: {KeyBindings.Current.Map_WorldMap_ClearSelection.HintText}");
        }
    }

    private float WorldMapScrollX = 0;
    private float WorldMapScrollY = 0;
    private float WorldMapScrollXMax = 0;
    private float WorldMapScrollYMax = 0;

    private bool AdjustScrollNextFrame = false;
    private float NextFrameAdjustmentX = 0;
    private float NextFrameAdjustmentY = 0;

    private Vector2 MouseDelta = new Vector2(0, 0);

    public void DisplayWorldMap()
    {
        if (Smithbox.ProjectType != ProjectType.ER)
            return;

        if (!WorldMapOpen)
            return;

        ImGui.Begin("World Map##WorldMapImage", ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);

        var windowHeight = ImGui.GetWindowHeight();
        var windowWidth = ImGui.GetWindowWidth();
        var mousePos = ImGui.GetMousePos();

        // Map Drag
        /*
        WorldMapScrollX = ImGui.GetScrollX();
        WorldMapScrollXMax = ImGui.GetScrollMaxX();
        WorldMapScrollY = ImGui.GetScrollY();
        WorldMapScrollYMax = ImGui.GetScrollMaxY();
        MouseDelta = InputTracker.MouseDelta;

        if (AdjustScrollNextFrame)
        {
            AdjustScrollNextFrame = false;
            ImGui.SetScrollFromPosX(NextFrameAdjustmentX);
        }
        */

        // Map
        TextureViewWindowPosition = ImGui.GetWindowPos();
        TextureViewScrollPosition = new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());

        ResourceHandle<TextureResource> resHandle = GetImageTextureHandle("smithbox/worldmap/world_map_vanilla");

        if(IsViewingSOTEMap)
        {
            resHandle = GetImageTextureHandle("smithbox/worldmap/world_map_sote");
        }

        if (resHandle != null)
        {
            TextureResource texRes = resHandle.Get();

            if (texRes != null)
            {
                trueSize = GetImageSize(texRes, false);
                size = GetImageSize(texRes, true);
                relativePos = GetRelativePosition(TextureViewWindowPosition, TextureViewScrollPosition);
                relativePosWindowPosition = GetRelativePositionWindowOnly(TextureViewWindowPosition);

                IntPtr handle = (nint)texRes.GPUTexture.TexHandle;

                ImGui.Image(handle, size);
            }
        }

        ImGui.End();

        // Properties
        ImGui.Begin("Properties##WorldMapProperties");

        ImguiUtils.WrappedText($"Press Left Mouse button to select an area of the map to filter the map object list by.");
        ImguiUtils.WrappedText($"");
        ImguiUtils.WrappedText($"Hold Left-Control and scroll the mouse wheel to zoom in and out.");
        ImguiUtils.WrappedText($"Press {KeyBindings.Current.TextureViewer_ZoomReset.HintText} to reset zoom level to 100%.");
        ImguiUtils.WrappedText($"");

        ImGui.Text($"Relative Position: {relativePos}");
        //ImGui.Text($"Relative (Sans Scroll) Position: {relativePosWindowPosition}");
        //ImGui.Text($"mousePos: {mousePos}");
        //ImGui.Text($"windowHeight: {windowHeight}");
        //ImGui.Text($"windowWidth: {windowWidth}");
        /*
        ImGui.Text($"MouseDelta: {InputTracker.MouseDelta}");
        ImGui.Text($"scrollPosX: {WorldMapScrollX}");
        ImGui.Text($"scrollPosXMax: {WorldMapScrollXMax}");
        ImGui.Text($"scrollPosY: {WorldMapScrollY}");
        ImGui.Text($"scrollPosYMax: {WorldMapScrollYMax}");
        */

        currentHoverMaps = GetMatchingMaps(relativePos);

        ImGui.Separator();
        ImGui.Text($"Maps in Tile:");
        ImguiUtils.ShowHoverTooltip("These are the maps that are within the tile you are currently hovering over within the world map.");
        ImGui.Separator();

        // Hover Maps
        if (currentHoverMaps != null && currentHoverMaps.Count > 0)
        {
            foreach(var match in currentHoverMaps)
            {
                ImGui.Text($"{match}");
                AliasUtils.DisplayAlias(Smithbox.NameCacheHandler.MapNameCache.GetMapName(match));
            }
        }

        ImGui.Separator();
        ImGui.Text($"Selection:");
        ImguiUtils.ShowHoverTooltip("These are the maps that the map object list will be filtered to.");
        ImGui.Separator();

        // Stored Click Maps
        if (Smithbox.EditorHandler.MapEditor.WorldMap_ClickedMapZone != null && Smithbox.EditorHandler.MapEditor.WorldMap_ClickedMapZone.Count > 0)
        {
            foreach (var match in Smithbox.EditorHandler.MapEditor.WorldMap_ClickedMapZone)
            {
                ImGui.Text($"{match}");
                AliasUtils.DisplayAlias(Smithbox.NameCacheHandler.MapNameCache.GetMapName(match));
            }
        }

        ImGui.End();

        if (InputTracker.GetMouseButtonDown(MouseButton.Left))
        {
            if (relativePosWindowPosition.X > 0 && relativePosWindowPosition.X < windowWidth && relativePosWindowPosition.Y > 0 && relativePosWindowPosition.Y < windowHeight)
            {
                if (currentHoverMaps != null && currentHoverMaps.Count > 0)
                {
                    Smithbox.EditorHandler.MapEditor.WorldMap_ClickedMapZone = currentHoverMaps;
                }
            }
        }
    }

    private void LoadWorldMapTexture() 
    {
        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob($@"Loading World Map textures");
        ResourceDescriptor ad = new ResourceDescriptor();
        ad.AssetVirtualPath = "smithbox/worldmap";

        if (!ResourceManager.IsResourceLoadedOrInFlight(ad.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
        {
            if (ad.AssetVirtualPath != null)
            {
                job.AddLoadFileTask(ad.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly, true);
            }

            _loadingTask = job.Complete();
        }

        ResourceManager.AddResourceListener<TextureResource>(ad.AssetVirtualPath, this, AccessLevel.AccessGPUOptimizedOnly);

        LoadedWorldMapTexture = true;
    }

    private void GenerateWorldMapLayout_Vanilla()
    {
        WorldMapLayout_Vanilla = new WorldMapLayout("60", 480, 55);
        WorldMapLayout_Vanilla.ConstructSmallTiles();
        WorldMapLayout_Vanilla.ConstructMediumTiles();
        WorldMapLayout_Vanilla.ConstructLargeTiles();
    }

    private void GenerateWorldMapLayout_SOTE()
    {
        WorldMapLayout_SOTE = new WorldMapLayout("61", 480, 55);
        WorldMapLayout_Vanilla.ConstructSmallTiles();
        WorldMapLayout_Vanilla.ConstructMediumTiles();
        WorldMapLayout_Vanilla.ConstructLargeTiles();
    }

    private List<string> GetMatchingMaps(Vector2 pos)
    {
        List<string> matches =  new List<string>();

        var tiles = WorldMapLayout_Vanilla.Tiles;

        if (IsViewingSOTEMap)
        {
            tiles = WorldMapLayout_SOTE.Tiles;
        }

        foreach(var tile in tiles)
        {
            var tileName = "";
            var match = false;

            (tileName, match) = MatchMousePosToIcon(tile, pos);

            if(match && !matches.Contains(tileName))
            {
                matches.Add(tileName);
            }
        }

        return matches;
    }

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
    }

    private Vector2 GetRelativePositionWindowOnly(Vector2 windowPos)
    {
        var scale = Smithbox.GetUIScale();

        Vector2 relativePos = new Vector2(0, 0);

        var fixedX = 3 * scale;
        var fixedY = 24 * scale;
        var cursorPos = ImGui.GetMousePos();

        // Account for window position
        relativePos.X = cursorPos.X - ((windowPos.X + fixedX));
        relativePos.Y = cursorPos.Y - ((windowPos.Y + fixedY));

        return relativePos;
    }

    private (string, bool) MatchMousePosToIcon(WorldMapTile tile, Vector2 relativePos)
    {
        var cursorPos = relativePos;

        var Name = tile.Name;

        var success = false;

        float Xmin = tile.X;
        float Xmax = Xmin + tile.Width;
        float Ymin = tile.Y;
        float Ymax = Ymin + tile.Height;

        if (cursorPos.X > Xmin && cursorPos.X < Xmax && cursorPos.Y > Ymin && cursorPos.Y < Ymax)
        {
            success = true;
        }

        return (Name, success);
    }

    private Vector2 GetImageSize(TextureResource texRes, bool includeZoomFactor)
    {
        Vector2 size = new Vector2(0, 0);

        if (texRes.GPUTexture != null)
        {
            var Width = texRes.GPUTexture.Width;
            var Height = texRes.GPUTexture.Height;

            if (Height != 0 && Width != 0)
            {
                if (includeZoomFactor)
                {
                    size = new Vector2((Width * zoomFactor.X), (Height * zoomFactor.Y));
                }
                else
                {
                    size = new Vector2(Width, Height);
                }
            }
        }

        return size;
    }

    public ResourceHandle<TextureResource> GetImageTextureHandle(string path)
    {
        var virtName = $@"{path}".ToLower();

        var resources = ResourceManager.GetResourceDatabase();

        if (resources.ContainsKey(virtName))
        {
            return (ResourceHandle<TextureResource>)resources[virtName];
        }

        return null;
    }

    private void HandleZoom()
    {
        var delta = InputTracker.GetMouseWheelDelta();

        if (delta > 0)
        {
            ZoomIn();
        }
        if (delta < 0)
        {
            ZoomOut();
        }
    }

    private void ZoomIn()
    {
        zoomFactor.X = zoomFactor.X + zoomFactorStep;
        zoomFactor.Y = zoomFactor.Y + zoomFactorStep;

        if (zoomFactor.X > 10.0f)
        {
            zoomFactor.X = 10.0f;
        }
        if (zoomFactor.Y > 10.0f)
        {
            zoomFactor.Y = 10.0f;
        }
    }
    private void ZoomOut()
    {
        zoomFactor.X = zoomFactor.X - zoomFactorStep;
        zoomFactor.Y = zoomFactor.Y - zoomFactorStep;

        if (zoomFactor.X < 0.1f)
        {
            zoomFactor.X = 0.1f;
        }
        if (zoomFactor.Y < 0.1f)
        {
            zoomFactor.Y = 0.1f;
        }
    }
    private Vector2 GetDefaultZoomLevel()
    {
        var scale = Smithbox.GetUIScale();
        return new Vector2(float.Round(0.2f * scale, 1), float.Round(0.2f * scale, 1));
    }

    private Vector2 GetRelativePosition(Vector2 windowPos, Vector2 scrollPos)
    {
        var scale = Smithbox.GetUIScale();

        Vector2 relativePos = new Vector2(0, 0);

        // Offsets to account for imgui spacing between window and image texture
        var fixedX = 8 * scale;
        var fixedY = 29 * scale;
        var cursorPos = ImGui.GetMousePos();

        // Account for window position and scroll
        relativePos.X = cursorPos.X - ((windowPos.X + fixedX) - scrollPos.X);
        relativePos.Y = cursorPos.Y - ((windowPos.Y + fixedY) - scrollPos.Y);

        // Account for zoom
        relativePos.X = relativePos.X / zoomFactor.X;
        relativePos.Y = relativePos.Y / zoomFactor.Y;

        return relativePos;
    }
}
