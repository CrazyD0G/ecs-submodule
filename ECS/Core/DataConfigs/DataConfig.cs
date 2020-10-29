﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ME.ECS.DataConfigs {

    [CreateAssetMenu(menuName = "ME.ECS/Data Config")]
    public class DataConfig : ScriptableObject {

        [SerializeReference]
        public IStructComponent[] structComponents = new IStructComponent[0];
        [SerializeReference]
        public IComponent[] components = new IComponent[0];

        public int[] structComponentsDataTypeIds = new int[0];
        public int[] componentsTypeIds = new int[0];
        
        [SerializeReference]
        public IStructComponent[] removeStructComponents = new IStructComponent[0];
        public int[] removeStructComponentsDataTypeIds = new int[0];
        
        public void Apply(in Entity entity) {

            var world = Worlds.currentWorld;
            for (int i = 0; i < this.removeStructComponentsDataTypeIds.Length; ++i) {

                world.RemoveData(in entity, in this.removeStructComponentsDataTypeIds[i], -1);

            }

            for (int i = 0; i < this.structComponents.Length; ++i) {

                world.SetData(in entity, in this.structComponents[i], in this.structComponentsDataTypeIds[i], -1);

            }
            
            for (int i = 0; i < this.components.Length; ++i) {

                world.AddComponent(entity, this.components[i], this.componentsTypeIds[i]);

            }
            
            // Update filters
            {
                world.UpdateFilters(in entity);
            }

        }

        public System.Type[] GetStructComponentTypes() {
            
            var types = new System.Type[this.structComponents.Length];
            for (int i = 0; i < this.structComponents.Length; ++i) {

                types[i] = this.structComponents[i].GetType();

            }
            
            return types;

        }
        
        public void SetByTypeId(int typeId, IStructComponent component) {
            
            var idx = System.Array.IndexOf(this.structComponentsDataTypeIds, typeId);
            if (idx >= 0) {

                this.structComponents[idx] = component;

            }

        }
        
        public IStructComponent GetByTypeId(int typeId) {
            
            var idx = System.Array.IndexOf(this.structComponentsDataTypeIds, typeId);
            if (idx >= 0) {

                return this.structComponents[idx];

            }

            return null;

        }

        public bool Has<T>() where T : struct, IStructComponent {

            var idx = System.Array.IndexOf(this.structComponentsDataTypeIds, AllComponentTypes<T>.typeId);
            if (idx >= 0) {

                return true;

            }

            return false;

        }

        public T Get<T>() where T : struct, IStructComponent {

            var idx = System.Array.IndexOf(this.structComponentsDataTypeIds, AllComponentTypes<T>.typeId);
            if (idx >= 0) {

                return (T)this.structComponents[idx];

            }

            return default;

        }
        
        public void OnValidate() {

            if (Application.isPlaying == true) return;
            
            this.OnScriptLoad();
            
        }

        public void OnScriptLoad() {

            if (Application.isPlaying == true) return;

            var changed = false;
            var allAsms = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in allAsms) {

                var asmType = asm.GetType("ME.ECS.ComponentsInitializer");
                if (asmType != null) {

                    var m = asmType.GetMethod("InitTypeId", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    if (m == null) continue;
                    
                    m.Invoke(null, null);
                    
                    {

                        if (this.removeStructComponentsDataTypeIds == null || this.removeStructComponentsDataTypeIds.Length != this.removeStructComponents.Length) {
                            
                            this.removeStructComponentsDataTypeIds = new int[this.removeStructComponents.Length];
                            changed = true;

                        }
                        
                        for (int i = 0; i < this.removeStructComponents.Length; ++i) {

                            var obj = this.removeStructComponents[i];
                            if (obj == null) {

                                if (this.removeStructComponentsDataTypeIds[i] != -1) {
                                    
                                    this.removeStructComponentsDataTypeIds[i] = -1;
                                    changed = true;
                                    
                                }
                                continue;
                                
                            }
                            
                            var type = obj.GetType();
                            if (ComponentTypesRegistry.allTypeId.ContainsKey(type) == false) {
                                
                                Debug.LogWarning("Type was not found: " + type + " on config " + this, this);
                                continue;
                                
                            }
                            var allId = ComponentTypesRegistry.allTypeId[type];
                            if (this.removeStructComponentsDataTypeIds[i] != allId) {
                                
                                this.removeStructComponentsDataTypeIds[i] = allId;
                                changed = true;

                            }

                        }
                        
                    }
                    
                    {

                        if (this.structComponentsDataTypeIds == null || this.structComponentsDataTypeIds.Length != this.structComponents.Length) {
                            
                            this.structComponentsDataTypeIds = new int[this.structComponents.Length];
                            changed = true;

                        }
                        
                        for (int i = 0; i < this.structComponents.Length; ++i) {

                            var obj = this.structComponents[i];
                            if (obj == null) {

                                if (this.structComponentsDataTypeIds[i] != -1) {
                                    
                                    this.structComponentsDataTypeIds[i] = -1;
                                    changed = true;
                                    
                                }
                                continue;
                                
                            }
                            
                            var type = obj.GetType();
                            if (ComponentTypesRegistry.allTypeId.ContainsKey(type) == false) {
                                
                                Debug.LogWarning("Type was not found: " + type + " on config " + this, this);
                                continue;
                                
                            }
                            var allId = ComponentTypesRegistry.allTypeId[type];
                            if (this.structComponentsDataTypeIds[i] != allId) {
                                
                                this.structComponentsDataTypeIds[i] = allId;
                                changed = true;

                            }

                        }
                        
                    }
                    
                    {

                        if (this.componentsTypeIds == null || this.componentsTypeIds.Length != this.components.Length) {
                            
                            this.componentsTypeIds = new int[this.components.Length];
                            changed = true;

                        }

                        for (int i = 0; i < this.components.Length; ++i) {

                            var obj = this.components[i];
                            if (obj == null) {

                                if (this.componentsTypeIds[i] != -1) {
                                    
                                    this.componentsTypeIds[i] = -1;
                                    changed = true;

                                }
                                continue;
                                
                            }
                            
                            var type = obj.GetType();
                            if (ComponentTypesRegistry.typeId.TryGetValue(type, out var componentIndex) == true) {

                                if (this.componentsTypeIds[i] != componentIndex) {
                                    
                                    this.componentsTypeIds[i] = componentIndex;
                                    changed = true;

                                }

                            } else {

                                if (this.componentsTypeIds[i] != -1) {
                                    
                                    this.componentsTypeIds[i] = -1;
                                    changed = true;

                                }

                            }

                        }
                        
                    }
                    break;

                }

            }

            if (changed == true) {

                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                #endif

                Debug.Log("DataConfig " + this + " reloaded");

            }

        }
        
        #if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
        public static void OnScriptsReloaded() {

            var configs = Resources.FindObjectsOfTypeAll<DataConfig>();
            foreach (var config in configs) config.OnScriptLoad();

        }
        #endif

    }

    public struct DataConfigSlice {

        public DataConfig[] configs;
        public int[] structComponentsDataTypeIds;
        public int[] componentsTypeIds;

        public void Set(int typeId, IStructComponent[] components) {

            for (int i = 0; i < this.configs.Length; ++i) {

                this.configs[i].SetByTypeId(typeId, components[i]);

            }
            
        }
        
        public static DataConfigSlice Distinct(DataConfig[] configs) {
            
            var slice = new DataConfigSlice();
            slice.configs = configs;
            
            {

                var listIdx = new Dictionary<int, int>();
                for (int i = 0; i < configs.Length; ++i) {

                    var config = configs[i];
                    for (int j = 0; j < config.structComponentsDataTypeIds.Length; ++j) {

                        var idx = config.structComponentsDataTypeIds[j];
                        if (listIdx.TryGetValue(idx, out var count) == true) {

                            listIdx[idx] = count + 1;

                        } else {

                            listIdx.Add(idx, 1);

                        }

                    }

                }

                var list = new List<int>();
                foreach (var kv in listIdx) {

                    if (kv.Value == configs.Length) {

                        list.Add(kv.Key);

                    }

                }

                slice.structComponentsDataTypeIds = list.ToArray();

            }
            
            {

                var listIdx = new Dictionary<int, int>();
                for (int i = 0; i < configs.Length; ++i) {

                    var config = configs[i];
                    for (int j = 0; j < config.componentsTypeIds.Length; ++j) {

                        var idx = config.componentsTypeIds[j];
                        if (listIdx.TryGetValue(idx, out var count) == true) {

                            listIdx[idx] = count + 1;

                        } else {

                            listIdx.Add(idx, 1);

                        }

                    }

                }

                var list = new List<int>();
                foreach (var kv in listIdx) {

                    if (kv.Value > 1) {

                        list.Add(kv.Key);

                    }

                }

                slice.componentsTypeIds = list.ToArray();

            }

            return slice;

        }
        
    }
    
}
