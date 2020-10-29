﻿using System.Linq;

namespace ME.ECSEditor {

    using UnityEngine;
    using UnityEditor;
    using ME.ECS;

    [UnityEditor.CustomEditor(typeof(ME.ECS.DataConfigs.DataConfig), true)]
    [CanEditMultipleObjects]
    public class DataConfigEditor : Editor {

        public struct Registry {

            public IStructComponent data;
            public int index;

        }

        public struct RegistryComponent {

            public IComponent data;
            public int index;

        }

        private static readonly WorldsViewerEditor.WorldEditor multipleWorldEditor = new WorldsViewerEditor.WorldEditor();
        private static readonly System.Collections.Generic.Dictionary<Object, WorldsViewerEditor.WorldEditor> worldEditors = new System.Collections.Generic.Dictionary<Object, WorldsViewerEditor.WorldEditor>();

        public override void OnInspectorGUI() {

            var style = new GUIStyle(EditorStyles.toolbar);
            style.fixedHeight = 0f;
            style.stretchHeight = true;

            var backStyle = new GUIStyle(EditorStyles.label);
            backStyle.normal.background = Texture2D.whiteTexture;

            var slice = new ME.ECS.DataConfigs.DataConfigSlice();
            var isMultiple = false;
            if (this.targets.Length > 1) {

                slice = ME.ECS.DataConfigs.DataConfigSlice.Distinct(this.targets.Cast<ME.ECS.DataConfigs.DataConfig>().ToArray());
                isMultiple = true;

            } else {

                var config = (ME.ECS.DataConfigs.DataConfig)this.target;
                slice = new ME.ECS.DataConfigs.DataConfigSlice() {
                    configs = new [] {
                        config
                    },
                    structComponentsDataTypeIds = config.structComponentsDataTypeIds,
                    componentsTypeIds = config.componentsTypeIds
                };
                
            }

            var usedComponentsAll = new System.Collections.Generic.HashSet<System.Type>();
            foreach (var cfg in slice.configs) {

                var componentTypes = cfg.GetStructComponentTypes();
                foreach (var cType in componentTypes) {
                    
                    if (usedComponentsAll.Contains(cType) == false) usedComponentsAll.Add(cType);
                    
                }
                
                if (DataConfigEditor.worldEditors.TryGetValue(cfg, out var worldEditor) == false) {

                    worldEditor = new WorldsViewerEditor.WorldEditor();
                    DataConfigEditor.worldEditors.Add(cfg, worldEditor);

                }
                
            }

            if (isMultiple == true) {

                GUILayoutExt.DrawHeader("The same components:");

                GUILayoutExt.Padding(8f, () => {

                    var kz = 0;
                    for (int i = 0; i < slice.structComponentsDataTypeIds.Length; ++i) {

                        var typeId = slice.structComponentsDataTypeIds[i];
                        var component = slice.configs[0].GetByTypeId(typeId);
                        var components = slice.configs.Select(x => x.GetByTypeId(typeId)).ToArray();

                        var backColor = GUI.backgroundColor;
                        GUI.backgroundColor = new Color(1f, 1f, 1f, kz++ % 2 == 0 ? 0f : 0.05f);

                        GUILayout.BeginVertical(backStyle);
                        {
                            GUI.backgroundColor = backColor;
                            var editor = WorldsViewerEditor.GetEditor(components);
                            if (editor != null) {

                                EditorGUI.BeginChangeCheck();
                                editor.OnDrawGUI();
                                if (EditorGUI.EndChangeCheck() == true) {

                                    //component = editor.GetTarget<IStructComponent>();
                                    //dataConfig.structComponents[registry.index] = component;
                                    //this.Save(dataConfig);
                                    slice.Set(typeId, components);
                                    this.Save(slice.configs);

                                }

                            } else {

                                var componentName = GUILayoutExt.GetStringCamelCaseSpace(component.GetType().Name);
                                var fieldsCount = GUILayoutExt.GetFieldsCount(component);
                                if (fieldsCount == 0) {

                                    EditorGUI.BeginDisabledGroup(true);
                                    EditorGUILayout.Toggle(componentName, true);
                                    EditorGUI.EndDisabledGroup();

                                } else if (fieldsCount == 1) {

                                    var changed = GUILayoutExt.DrawFields(DataConfigEditor.multipleWorldEditor, components, componentName);
                                    if (changed == true) {

                                        slice.Set(typeId, components);
                                        this.Save(slice.configs);

                                    }

                                } else {

                                    GUILayout.BeginHorizontal();
                                    {
                                        GUILayout.Space(18f);
                                        GUILayout.BeginVertical();
                                        {

                                            var key = "ME.ECS.WorldsViewerEditor.FoldoutTypes." + component.GetType().FullName;
                                            var foldout = EditorPrefs.GetBool(key, true);
                                            GUILayoutExt.FoldOut(ref foldout, componentName, () => {

                                                var changed = GUILayoutExt.DrawFields(DataConfigEditor.multipleWorldEditor, components);
                                                if (changed == true) {

                                                    slice.Set(typeId, components);
                                                    this.Save(slice.configs);

                                                }

                                            });
                                            EditorPrefs.SetBool(key, foldout);

                                        }
                                        GUILayout.EndVertical();
                                    }
                                    GUILayout.EndHorizontal();

                                }

                                GUILayoutExt.DrawComponentHelp(component.GetType());

                            }
                        }
                        GUILayout.EndVertical();

                        GUILayoutExt.Separator();

                    }

                });

                GUILayoutExt.DrawAddComponentMenu(usedComponentsAll, (addType, isUsed) => {

                    foreach (var dataConfigInner in slice.configs) {

                        if (isUsed == true) {

                            usedComponentsAll.Remove(addType);
                            for (int i = 0; i < dataConfigInner.structComponents.Length; ++i) {

                                if (dataConfigInner.structComponents[i].GetType() == addType) {

                                    var list = dataConfigInner.structComponents.ToList();
                                    list.RemoveAt(i);
                                    dataConfigInner.structComponents = list.ToArray();
                                    dataConfigInner.OnScriptLoad();
                                    this.Save(dataConfigInner);
                                    break;

                                }

                            }

                        } else {

                            usedComponentsAll.Add(addType);
                            System.Array.Resize(ref dataConfigInner.structComponents, dataConfigInner.structComponents.Length + 1);
                            dataConfigInner.structComponents[dataConfigInner.structComponents.Length - 1] = (IStructComponent)System.Activator.CreateInstance(addType);
                            dataConfigInner.OnScriptLoad();
                            this.Save(dataConfigInner);

                        }

                    }

                });
                
                return;

            }
            
            GUILayoutExt.Separator(6f);
            GUILayoutExt.DrawHeader("Add Struct Components:");
            GUILayoutExt.Separator();

            var dataConfig = (ME.ECS.DataConfigs.DataConfig)this.target;
            GUILayoutExt.Padding(8f, () => {

                var usedComponents = new System.Collections.Generic.HashSet<System.Type>();

                var kz = 0;
                var registries = dataConfig.structComponents;
                var sortedRegistries = new System.Collections.Generic.SortedDictionary<int, Registry>(new WorldsViewerEditor.DuplicateKeyComparer<int>());
                for (int i = 0; i < registries.Length; ++i) {

                    var registry = registries[i];
                    if (registry == null) {
                        continue;
                    }

                    var component = registry;
                    usedComponents.Add(component.GetType());

                    var editor = WorldsViewerEditor.GetEditor(component, out var order);
                    if (editor != null) {

                        sortedRegistries.Add(order, new Registry() {
                            index = i,
                            data = component
                        });

                    } else {

                        sortedRegistries.Add(0, new Registry() {
                            index = i,
                            data = component
                        });

                    }

                }

                foreach (var registryKv in sortedRegistries) {

                    var registry = registryKv.Value;
                    var component = registry.data;

                    var backColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(1f, 1f, 1f, kz++ % 2 == 0 ? 0f : 0.05f);

                    GUILayout.BeginVertical(backStyle);
                    {
                        GUI.backgroundColor = backColor;
                        var editor = WorldsViewerEditor.GetEditor(component);
                        if (editor != null) {

                            EditorGUI.BeginChangeCheck();
                            editor.OnDrawGUI();
                            if (EditorGUI.EndChangeCheck() == true) {

                                component = editor.GetTarget<IStructComponent>();
                                dataConfig.structComponents[registry.index] = component;
                                this.Save(dataConfig);

                            }

                        } else {

                            var componentName = GUILayoutExt.GetStringCamelCaseSpace(component.GetType().Name);
                            var fieldsCount = GUILayoutExt.GetFieldsCount(component);
                            if (fieldsCount == 0) {

                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.Toggle(componentName, true);
                                EditorGUI.EndDisabledGroup();

                                GUILayoutExt.DrawComponentHelp(component.GetType());

                            } else if (fieldsCount == 1) {

                                var changed = GUILayoutExt.DrawFields(DataConfigEditor.multipleWorldEditor, component, componentName);
                                if (changed == true) {

                                    dataConfig.structComponents[registry.index] = component;
                                    this.Save(dataConfig);

                                }

                            } else {

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Space(18f);
                                    GUILayout.BeginVertical();
                                    {

                                        var key = "ME.ECS.WorldsViewerEditor.FoldoutTypes." + component.GetType().FullName;
                                        var foldout = EditorPrefs.GetBool(key, true);
                                        GUILayoutExt.FoldOut(ref foldout, componentName, () => {

                                            var changed = GUILayoutExt.DrawFields(DataConfigEditor.multipleWorldEditor, component);
                                            if (changed == true) {

                                                dataConfig.structComponents[registry.index] = component;
                                                this.Save(dataConfig);

                                            }

                                        });
                                        EditorPrefs.SetBool(key, foldout);

                                    }
                                    GUILayout.EndVertical();
                                }
                                GUILayout.EndHorizontal();

                            }

                        }
                    }
                    GUILayout.EndVertical();

                    GUILayoutExt.Separator();

                }

                GUILayoutExt.DrawAddComponentMenu(usedComponents, (addType, isUsed) => {

                    if (isUsed == true) {

                        usedComponents.Remove(addType);
                        for (int i = 0; i < dataConfig.structComponents.Length; ++i) {

                            if (dataConfig.structComponents[i].GetType() == addType) {

                                var list = dataConfig.structComponents.ToList();
                                list.RemoveAt(i);
                                dataConfig.structComponents = list.ToArray();
                                dataConfig.OnScriptLoad();
                                this.Save(dataConfig);
                                break;

                            }

                        }

                    } else {

                        usedComponents.Add(addType);
                        System.Array.Resize(ref dataConfig.structComponents, dataConfig.structComponents.Length + 1);
                        dataConfig.structComponents[dataConfig.structComponents.Length - 1] = (IStructComponent)System.Activator.CreateInstance(addType);
                        dataConfig.OnScriptLoad();
                        this.Save(dataConfig);

                    }

                });

            });

            GUILayoutExt.Separator(6f);
            GUILayoutExt.DrawHeader("Add Managed Components:");
            GUILayoutExt.Separator();

            GUILayoutExt.Padding(8f, () => {

                var usedComponents = new System.Collections.Generic.HashSet<System.Type>();

                var kz = 0;
                var registries = dataConfig.components;
                var sortedRegistries = new System.Collections.Generic.SortedDictionary<int, RegistryComponent>(new WorldsViewerEditor.DuplicateKeyComparer<int>());
                for (int i = 0; i < registries.Length; ++i) {

                    var registry = registries[i];
                    if (registry == null) {
                        continue;
                    }

                    var component = registry;
                    usedComponents.Add(component.GetType());

                    var editor = WorldsViewerEditor.GetEditor(component, out var order);
                    if (editor != null) {

                        sortedRegistries.Add(order, new RegistryComponent() {
                            index = i,
                            data = component
                        });

                    } else {

                        sortedRegistries.Add(0, new RegistryComponent() {
                            index = i,
                            data = component
                        });

                    }

                }

                foreach (var registryKv in sortedRegistries) {

                    var registry = registryKv.Value;
                    var component = registry.data;

                    var backColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(1f, 1f, 1f, kz++ % 2 == 0 ? 0f : 0.05f);

                    GUILayout.BeginVertical(backStyle);
                    {
                        GUI.backgroundColor = backColor;
                        var editor = WorldsViewerEditor.GetEditor(component);
                        if (editor != null) {

                            EditorGUI.BeginChangeCheck();
                            editor.OnDrawGUI();
                            if (EditorGUI.EndChangeCheck() == true) {

                                component = editor.GetTarget<IComponent>();
                                dataConfig.components[registry.index] = component;
                                this.Save(dataConfig);

                            }

                        } else {

                            var componentName = component.GetType().Name;
                            var fieldsCount = GUILayoutExt.GetFieldsCount(component);
                            if (fieldsCount == 0) {

                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.Toggle(componentName, true);
                                EditorGUI.EndDisabledGroup();

                            } else if (fieldsCount == 1) {

                                var changed = GUILayoutExt.DrawFields(DataConfigEditor.multipleWorldEditor, component, componentName);
                                if (changed == true) {

                                    dataConfig.components[registry.index] = component;
                                    this.Save(dataConfig);

                                }

                            } else {

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Space(18f);
                                    GUILayout.BeginVertical();
                                    {

                                        var key = "ME.ECS.WorldsViewerEditor.FoldoutTypes." + component.GetType().FullName;
                                        var foldout = EditorPrefs.GetBool(key, true);
                                        GUILayoutExt.FoldOut(ref foldout, componentName, () => {

                                            var changed = GUILayoutExt.DrawFields(DataConfigEditor.multipleWorldEditor, component);
                                            if (changed == true) {

                                                dataConfig.components[registry.index] = component;
                                                this.Save(dataConfig);

                                            }

                                        });
                                        EditorPrefs.SetBool(key, foldout);

                                    }
                                    GUILayout.EndVertical();
                                }
                                GUILayout.EndHorizontal();

                            }

                        }
                    }
                    GUILayout.EndVertical();

                    GUILayoutExt.Separator();

                }

                GUILayoutExt.DrawAddComponentMenu(usedComponents, (addType, isUsed) => {

                    if (isUsed == true) {

                        usedComponents.Remove(addType);
                        for (int i = 0; i < dataConfig.components.Length; ++i) {

                            if (dataConfig.components[i].GetType() == addType) {

                                var list = dataConfig.components.ToList();
                                list.RemoveAt(i);
                                dataConfig.components = list.ToArray();
                                dataConfig.OnScriptLoad();
                                this.Save(dataConfig);
                                break;

                            }

                        }

                    } else {

                        usedComponents.Add(addType);
                        System.Array.Resize(ref dataConfig.components, dataConfig.components.Length + 1);
                        dataConfig.components[dataConfig.components.Length - 1] = (IComponent)System.Activator.CreateInstance(addType);
                        dataConfig.OnScriptLoad();
                        this.Save(dataConfig);

                    }

                }, drawRefComponents: true);

            });

            GUILayoutExt.Separator(6f);
            GUILayoutExt.DrawHeader("Remove Struct Components:");
            GUILayoutExt.Separator();

            // Remove struct components
            GUILayoutExt.Padding(8f, () => {
             
                var usedComponents = new System.Collections.Generic.HashSet<System.Type>();

                var kz = 0;
                var registries = dataConfig.removeStructComponentsDataTypeIds;
                for (int i = 0; i < registries.Length; ++i) {

                    var registry = registries[i];
                    var type = ComponentTypesRegistry.allTypeId.FirstOrDefault(x => x.Value == registry).Key;
                    
                    if (type == null) {
                        continue;
                    }

                    usedComponents.Add(type);

                    var backColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(1f, 1f, 1f, kz++ % 2 == 0 ? 0f : 0.05f);

                    GUILayout.BeginVertical(backStyle);
                    {
                        GUI.backgroundColor = backColor;
                        var componentName = GUILayoutExt.GetStringCamelCaseSpace(type.Name);
                        
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.Toggle(componentName, true);
                        EditorGUI.EndDisabledGroup();

                        GUILayoutExt.DrawComponentHelp(type);

                    }
                    GUILayout.EndVertical();

                    GUILayoutExt.Separator();

                }

                GUILayoutExt.DrawAddComponentMenu(usedComponents, (addType, isUsed) => {

                    if (isUsed == true) {

                        usedComponents.Remove(addType);
                        for (int i = 0; i < dataConfig.removeStructComponents.Length; ++i) {

                            if (dataConfig.removeStructComponents[i].GetType() == addType) {

                                var list = dataConfig.removeStructComponents.ToList();
                                list.RemoveAt(i);
                                dataConfig.removeStructComponents = list.ToArray();
                                dataConfig.OnScriptLoad();
                                this.Save(dataConfig);
                                break;

                            }

                        }

                    } else {

                        usedComponents.Add(addType);
                        System.Array.Resize(ref dataConfig.removeStructComponents, dataConfig.removeStructComponents.Length + 1);
                        dataConfig.removeStructComponents[dataConfig.removeStructComponents.Length - 1] = (IStructComponent)System.Activator.CreateInstance(addType);
                        dataConfig.OnScriptLoad();
                        this.Save(dataConfig);

                    }

                });

            });

        }

        private void Save(ME.ECS.DataConfigs.DataConfig dataConfig) {
            
            EditorUtility.SetDirty(dataConfig);
            
        }

        private void Save(ME.ECS.DataConfigs.DataConfig[] dataConfigs) {
            
            foreach (var dataConfig in dataConfigs) EditorUtility.SetDirty(dataConfig);
            
        }

    }

}