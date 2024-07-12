﻿using HKLib.hk2018.hkHashMapDetail;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editors.MapEditor;
using StudioCore.Gui;
using StudioCore.Locators;
using StudioCore.MsbEditor;
using StudioCore.Resource;
using StudioCore.Scene;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.ModelEditor
{
    public class ModelViewportHandler
    {
        public ModelEditorScreen Screen;
        public IViewport Viewport;

        public MeshRenderableProxy _renderMesh;
        public ResourceHandle<FlverResource> _flverhandle;

        public string ContainerID;

        public ModelViewportHandler(ModelEditorScreen screen, IViewport viewport)
        {
            Screen = screen;
            Viewport = viewport;
            ContainerID = "";
        }

        public void OnResourceLoaded(IResourceHandle handle, int tag)
        {
            _flverhandle = (ResourceHandle<FlverResource>)handle;
            _flverhandle.Acquire();

            if (_renderMesh != null)
            {
                BoundingBox box = _renderMesh.GetBounds();
                Viewport.FrameBox(box);

                Vector3 dim = box.GetDimensions();
                var mindim = Math.Min(dim.X, Math.Min(dim.Y, dim.Z));
                var maxdim = Math.Max(dim.X, Math.Max(dim.Y, dim.Z));

                var minSpeed = 1.0f;
                var basespeed = Math.Max(minSpeed, (float)Math.Sqrt(mindim / 3.0f));
                Viewport.WorldView.CameraMoveSpeed_Normal = basespeed;
                Viewport.WorldView.CameraMoveSpeed_Slow = basespeed / 10.0f;
                Viewport.WorldView.CameraMoveSpeed_Fast = basespeed * 10.0f;

                Viewport.NearClip = Math.Max(0.001f, maxdim / 10000.0f);
            }

            if (_flverhandle.IsLoaded && _flverhandle.Get() != null)
            {
                FlverResource r = _flverhandle.Get();
                if (r.Flver != null)
                {
                    Screen._universe.UnloadModels(true);
                    Screen._universe.LoadFlverInModelEditor(r.Flver, _renderMesh, Screen.ResourceHandler.CurrentFLVERInfo.ModelName);
                    ContainerID = Screen.ResourceHandler.CurrentFLVERInfo.ModelName;
                }
            }

            if (CFG.Current.Viewport_Enable_Texturing)
            {
                Screen._universe.ScheduleTextureRefresh();
            }
        }

        public void OnResourceUnloaded(IResourceHandle handle, int tag)
        {
            _flverhandle = null;
        }

        /// <summary>
        /// Updated the viewport FLVER model render mesh
        /// </summary>
        public void UpdateRenderMesh(ResourceDescriptor modelAsset, bool skipModel = false)
        {
            if (Universe.IsRendering)
            {
                // Ignore this if we are only loading textures
                if (!skipModel)
                {
                    if (_renderMesh != null)
                    {
                        _renderMesh.Dispose();
                    }

                    _renderMesh = MeshRenderableProxy.MeshRenderableFromFlverResource(Screen.RenderScene, modelAsset.AssetVirtualPath, ModelMarkerType.None, null);
                    _renderMesh.World = Matrix4x4.Identity;
                }
            }
        }

        public void UpdateRepresentativeDummy(int index, Vector3 position)
        {
            if (_flverhandle.Get().Flver.Dummies.Count < index)
                return;

            var container = Screen._universe.LoadedModelContainers[ContainerID];

            // This relies on the index of the lists to align
            for (int i = 0; i < container.DummyPoly_RootNode.Children.Count; i++)
            {
                var curNode = container.DummyPoly_RootNode.Children[i];

                if (i == index)
                {
                    DummyPositionChange act = new(curNode, position);
                    Screen.EditorActionManager.ExecuteAction(act);

                    break;
                }
            }
        }

        public void UpdateRepresentativeNode(int index, Vector3 position)
        {
            if (_flverhandle.Get().Flver.Bones.Count < index)
                return;

            var container = Screen._universe.LoadedModelContainers[ContainerID];

            // This relies on the index of the lists to align
            for (int i = 0; i < container.Bone_RootNode.Children.Count; i++)
            {
                var curNode = container.Bone_RootNode.Children[i];

                if (i == index)
                {
                    BonePositionChange act = new(curNode, position);
                    Screen.EditorActionManager.ExecuteAction(act);

                    break;
                }
            }
        }

        public void SelectRepresentativeDummy(int index)
        {
            if (_flverhandle.Get().Flver.Dummies.Count < index)
                return;

            var container = Screen._universe.LoadedModelContainers[ContainerID];

            // This relies on the index of the lists to align
            for (int i = 0; i < container.DummyPoly_RootNode.Children.Count; i++)
            {
                var curNode = container.DummyPoly_RootNode.Children[i];

                if(i == index)
                {
                    Screen._selection.ClearSelection();
                    Screen._selection.AddSelection(curNode);
                    break;
                }
            }
        }

        public void SelectRepresentativeNode(int index)
        {
            if (_flverhandle.Get().Flver.Bones.Count < index)
                return;

            var container = Screen._universe.LoadedModelContainers[ContainerID];

            // This relies on the index of the lists to align
            for (int i = 0; i < container.Bone_RootNode.Children.Count; i++)
            {
                var curNode = container.Bone_RootNode.Children[i];

                if (i == index)
                {
                    Screen._selection.ClearSelection();
                    Screen._selection.AddSelection(curNode);
                    break;
                }
            }
        }

        public void DisplayRepresentativeDummyState(int index)
        {
            if (_flverhandle.Get().Flver.Dummies.Count < index)
                return;

            var container = Screen._universe.LoadedModelContainers[ContainerID];

            Entity curEntity = null;

            // This relies on the index of the lists to align
            for (int i = 0; i < container.DummyPoly_RootNode.Children.Count; i++)
            {
                var curNode = container.DummyPoly_RootNode.Children[i];

                if (i == index)
                {
                    curEntity = curNode;
                }
            }

            if (curEntity != null)
            {
                ImGui.SetItemAllowOverlap();
                var isVisible = curEntity.EditorVisible;
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - 18.0f * Smithbox.GetUIScale());
                ImGui.PushStyleColor(ImGuiCol.Text, isVisible
                    ? new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                    : new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
                ImGui.TextWrapped(isVisible ? ForkAwesome.Eye : ForkAwesome.EyeSlash);
                ImGui.PopStyleColor();

                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    // Quick-tool all if this key is down
                    if (InputTracker.GetKey(KeyBindings.Current.ModelEditor_ToggleVisibilitySection))
                    {
                        for (int i = 0; i < container.DummyPoly_RootNode.Children.Count; i++)
                        {
                            Screen.ViewportHandler.ToggleRepresentativeDummy(i);
                        }
                    }
                    // Otherwise just toggle this row
                    else
                    {
                        Screen.ViewportHandler.ToggleRepresentativeDummy(index);
                    }
                }
            }
        }

        public void ToggleRepresentativeDummy(int index)
        {
            if (_flverhandle.Get().Flver.Dummies.Count < index)
                return;

            var container = Screen._universe.LoadedModelContainers[ContainerID];

            // This relies on the index of the lists to align
            for (int i = 0; i < container.DummyPoly_RootNode.Children.Count; i++)
            {
                var curNode = container.DummyPoly_RootNode.Children[i];

                if (i == index)
                {
                    curNode.EditorVisible = !curNode.EditorVisible;
                }
            }
        }

        public void DisplayRepresentativeNodeState(int index)
        {
            if (_flverhandle.Get().Flver.Bones.Count < index)
                return;

            var container = Screen._universe.LoadedModelContainers[ContainerID];

            Entity curEntity = null;

            // This relies on the index of the lists to align
            for (int i = 0; i < container.Bone_RootNode.Children.Count; i++)
            {
                var curNode = container.Bone_RootNode.Children[i];

                if (i == index)
                {
                    curEntity = curNode;
                }
            }

            if (curEntity != null)
            {
                ImGui.SetItemAllowOverlap();
                var isVisible = curEntity.EditorVisible;
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - 18.0f * Smithbox.GetUIScale());
                ImGui.PushStyleColor(ImGuiCol.Text, isVisible
                    ? new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                    : new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
                ImGui.TextWrapped(isVisible ? ForkAwesome.Eye : ForkAwesome.EyeSlash);
                ImGui.PopStyleColor();

                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    // Quick-tool all if this key is down
                    if (InputTracker.GetKey(KeyBindings.Current.ModelEditor_ToggleVisibilitySection))
                    {
                        for (int i = 0; i < container.Bone_RootNode.Children.Count; i++)
                        {
                            Screen.ViewportHandler.ToggleRepresentativeNode(i);
                        }
                    }
                    // Otherwise just toggle this row
                    else
                    {
                        Screen.ViewportHandler.ToggleRepresentativeNode(index);
                    }
                }
            }
        }

        public void ToggleRepresentativeNode(int index)
        {
            if (_flverhandle.Get().Flver.Bones.Count < index)
                return;

            var container = Screen._universe.LoadedModelContainers[ContainerID];

            // This relies on the index of the lists to align
            for (int i = 0; i < container.Bone_RootNode.Children.Count; i++)
            {
                var curNode = container.Bone_RootNode.Children[i];

                if (i == index)
                {
                    curNode.EditorVisible = !curNode.EditorVisible;
                }
            }
        }

        public void DisplayRepresentativeMeshState(int index)
        {
            if (_flverhandle.Get().Flver.Meshes.Count < index)
                return;

            var container = Screen._universe.LoadedModelContainers[ContainerID];

            Entity curEntity = null;

            // This relies on the index of the lists to align
            for (int i = 0; i < container.Mesh_RootNode.Children.Count; i++)
            {
                var curNode = container.Mesh_RootNode.Children[i];

                if (i == index)
                {
                    curEntity = curNode;
                }
            }

            if(curEntity != null)
            {
                ImGui.SetItemAllowOverlap();
                var isVisible = curEntity.EditorVisible;
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - 18.0f * Smithbox.GetUIScale());
                ImGui.PushStyleColor(ImGuiCol.Text, isVisible
                    ? new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                    : new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
                ImGui.TextWrapped(isVisible ? ForkAwesome.Eye : ForkAwesome.EyeSlash);
                ImGui.PopStyleColor();

                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    // Quick-tool all if this key is down
                    if (InputTracker.GetKey(KeyBindings.Current.ModelEditor_ToggleVisibilitySection))
                    {
                        for (int i = 0; i < container.Mesh_RootNode.Children.Count; i++)
                        {
                            Screen.ViewportHandler.ToggleRepresentativeMesh(i);
                        }
                    }
                    // Otherwise just toggle this row
                    else
                    {
                        Screen.ViewportHandler.ToggleRepresentativeMesh(index);
                    }
                }
            }
        }

        public void ToggleRepresentativeMesh(int index)
        {
            if (_flverhandle.Get().Flver.Meshes.Count < index)
                return;

            var container = Screen._universe.LoadedModelContainers[ContainerID];

            // This relies on the index of the lists to align
            for (int i = 0; i < container.Mesh_RootNode.Children.Count; i++)
            {
                var curNode = container.Mesh_RootNode.Children[i];

                if (i == index)
                {
                    curNode.EditorVisible = !curNode.EditorVisible;
                }
            }
        }

        public void OnRepresentativeEntitySelected(Entity ent)
        {
            if (!IsTransformableNode(ent))
                return;

            TransformableNamedEntity transformEnt = (TransformableNamedEntity)ent;

            // Dummies
            if (transformEnt.WrappedObject is FLVER.Dummy)
            {
                Screen.ModelHierarchy._lastSelectedEntry = ModelEntrySelectionType.Dummy;
                Screen.ModelHierarchy._selectedDummy = transformEnt.Index;
            }
            // Bones
            if (transformEnt.WrappedObject is FLVER.Bone)
            {
                Screen.ModelHierarchy._lastSelectedEntry = ModelEntrySelectionType.Node;
                Screen.ModelHierarchy._selectedNode = transformEnt.Index;
            }
        }

        public void OnRepresentativeEntityDeselected(Entity ent)
        {
            if (!IsTransformableNode(ent))
                return;

            TransformableNamedEntity transformEnt = (TransformableNamedEntity)ent;
        }

        public void OnRepresentativeEntityUpdate(Entity ent)
        {
            if (!IsTransformableNode(ent))
                return;

            TransformableNamedEntity transformEnt = (TransformableNamedEntity)ent;

            // Dummies
            if (transformEnt.WrappedObject is FLVER.Dummy)
            {
                UpdateStoredDummyPosition(transformEnt);
            }
            // Bones
            if (transformEnt.WrappedObject is FLVER.Bone)
            {
                UpdateStoredNodePosition(transformEnt);
            }
        }

        private void UpdateStoredDummyPosition(TransformableNamedEntity transformEnt)
        {
            if (Screen.ModelHierarchy._selectedDummy == -1)
                return;
            
            var dummy = Screen.ResourceHandler.CurrentFLVER.Dummies[Screen.ModelHierarchy._selectedDummy];
            var entDummy = (FLVER.Dummy)transformEnt.WrappedObject;

            if(dummy.Position != entDummy.Position)
            {
                dummy.Position = entDummy.Position;
            }
        }

        private void UpdateStoredNodePosition(TransformableNamedEntity transformEnt)
        {
            if (Screen.ModelHierarchy._selectedNode == -1)
                return;

            var bone = Screen.ResourceHandler.CurrentFLVER.Nodes[Screen.ModelHierarchy._selectedNode];
            var entBone = (FLVER.Bone)transformEnt.WrappedObject;

            if (bone.Translation != entBone.Position)
            {
                bone.Translation = entBone.Position;
            }
        }

        public bool IsTransformableNode(Entity ent)
        {
            if(ent is TransformableNamedEntity)
            {
                return true;
            }

            return false;
        }
    }
}
