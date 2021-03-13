namespace ME.ECS {

    [System.Serializable]
    public sealed class FeaturesListCategories {

        public System.Collections.Generic.List<FeaturesListCategory> items = new System.Collections.Generic.List<FeaturesListCategory>();

        public void Initialize(World world) {

            for (int i = 0; i < this.items.Count; ++i) {
                
                this.items[i].features.Initialize(world);
                
            }
            
            for (int i = 0; i < this.items.Count; ++i) {
                
                this.items[i].features.InitializePost(world);
                
            }
            
        }

        public void DeInitialize(World world) {

            for (int i = 0; i < this.items.Count; ++i) {
                
                this.items[i].features.DeInitialize(world);
                
            }

        }

    }

    [System.Serializable]
    public sealed class FeaturesListCategory {

        public string folderCaption;
        public FeaturesList features = new FeaturesList();

    }
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    [System.Serializable]
    public sealed class FeaturesList {

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Serializable]
        public sealed class FeatureData {

            public bool enabled;
            public FeatureBase feature;
            public FeatureBase featureInstance;
            
        }

        public System.Collections.Generic.List<FeatureData> features = new System.Collections.Generic.List<FeatureData>();

        internal void Initialize(World world) {

            for (int i = 0; i < this.features.Count; ++i) {

                var item = this.features[i];
                if (item.enabled == true) {

                    var instance = (world.settings.createInstanceForFeatures == true ? UnityEngine.Object.Instantiate(item.feature) : item.feature);
                    if (world.settings.createInstanceForFeatures == true) instance.name = item.feature.name;
                    world.AddFeature(item.featureInstance = instance, doConstruct: false);

                }

            }

        }
        
        public void InitializePost(World world) {

            for (int i = 0; i < this.features.Count; ++i) {
                
                var item = this.features[i];
                if (item.enabled == true) {
                    
                    item.featureInstance.DoConstruct();
                    
                }
                
            }

        }

        internal void DeInitialize(World world) {
            
            for (int i = 0; i < this.features.Count; ++i) {
                
                var item = this.features[i];
                if (item.enabled == true) {
                    
                    world.RemoveFeature(item.featureInstance);
                    if (world.settings.createInstanceForFeatures == true) UnityEngine.Object.DestroyImmediate(item.featureInstance);
                    item.featureInstance = null;

                }
                
            }

        }

    }

    public abstract class FeatureBase : UnityEngine.ScriptableObject, IFeatureBase {

        [UnityEngine.TextAreaAttribute]
        public string editorComment;

        public World world { get; set; }
        protected SystemGroup systemGroup;

        internal void DoConstruct() {
            
            this.systemGroup = new SystemGroup(this.world, this.name);
            Filter.RegisterInject(this.InjectFilter);
            this.OnConstruct();
            Filter.UnregisterInject(this.InjectFilter);
            
        }

        internal void DoDeconstruct() {
            
            this.OnDeconstruct();
            
        }

        protected virtual void InjectFilter(Filter filter) {}
        
        protected abstract void OnConstruct();
        protected abstract void OnDeconstruct();

        protected bool AddSystem<TSystem>() where TSystem : class, ISystemBase, new() {

            if (this.systemGroup.HasSystem<TSystem>() == false) {

                return this.systemGroup.AddSystem<TSystem>();
                
            }

            return false;

        }

        protected bool AddModule<TModule>() where TModule : class, IModuleBase, new() {

            if (this.world.HasModule<TModule>() == false) {
                
                return this.world.AddModule<TModule>();
                
            }

            return false;

        }

    }

    public abstract class Feature : FeatureBase, IFeature {

    }

}