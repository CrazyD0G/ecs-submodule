﻿
namespace ME.ECS.Collections {

    using System.Collections.Generic;
    
    public struct IntrusiveSortedListGenericNode<T> : IStructComponent {

        public Entity next;
        public Entity prev;
        public T data;

    }

    public interface IIntrusiveSortedListGeneric<T> where T : struct, System.IEquatable<T>, System.IComparable<T> {

        int Count { get; }
        
        void Add(in T entityData);
        bool Remove(in T entityData);
        int RemoveAll(in T entityData);
        void Clear();
        bool RemoveAt(int index);
        int RemoveRange(int from, int to);
        T GetValue(int index);
        bool Contains(in T entityData);
        T GetFirst();
        T GetLast();
        bool RemoveLast();
        bool RemoveFirst();

        IEnumerator<T> GetRange(int from, int to);
        BufferArray<T> ToArray();
        IntrusiveSortedListGeneric<T>.Enumerator GetEnumerator();

    }
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public struct IntrusiveSortedListGeneric<T> : IIntrusiveSortedListGeneric<T> where T : struct, System.IEquatable<T>, System.IComparable<T> {

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public struct Enumerator : System.Collections.Generic.IEnumerator<T> {

            private readonly Entity root;
            private Entity head;
            public T Current { get; private set; }

            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            public Enumerator(IntrusiveSortedListGeneric<T> list) {

                this.root = list.root;
                this.head = list.root;
                this.Current = default;
                
            }

            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() {

                if (this.head.IsAlive() == false) return false;

                this.Current = this.head.GetData<IntrusiveSortedListGenericNode<T>>().data;
                
                this.head = this.head.GetData<IntrusiveSortedListGenericNode<T>>().next;
                return true;

            }

            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            public void Reset() {
                
                this.head = this.root;
                this.Current = default;
                
            }

            object System.Collections.IEnumerator.Current {
                get {
                    throw new AllocationException();
                }
            }

            public void Dispose() {

            }

        }

        private Entity root;
        private Entity head;
        private int count;
        private bool descending;

        public int Count => this.count;

        public IntrusiveSortedListGeneric(bool descending) {

            this.descending = descending;
            this.root = default;
            this.head = default;
            this.count = 0;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {

            IntrusiveSortedListGeneric<T>.InitializeComponents();

            return new Enumerator(this);

        }

        /// <summary>
        /// Put entity data into array.
        /// </summary>
        /// <returns>Buffer array from pool</returns>
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public BufferArray<T> ToArray() {

            IntrusiveSortedListGeneric<T>.InitializeComponents();

            var arr = PoolArray<T>.Spawn(this.count);
            var i = 0;
            foreach (var entity in this) {
                
                arr.arr[i++] = entity;
                
            }

            return arr;

        }

        /// <summary>
        /// Find an element.
        /// </summary>
        /// <param name="entityData"></param>
        /// <returns>Returns TRUE if data was found</returns>
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T entityData) {
            
            if (this.count == 0) return false;

            IntrusiveSortedListGeneric<T>.InitializeComponents();

            var node = this.FindNode(in entityData);
            if (node.IsAlive() == true) {

                return true;

            }
            
            return false;

        }
        
        /// <summary>
        /// Clear the list.
        /// </summary>
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Clear() {

            IntrusiveSortedListGeneric<T>.InitializeComponents();

            while (this.root.IsAlive() == true) {

                var node = this.root;
                this.root = this.root.GetData<IntrusiveSortedListGenericNode<T>>().next;
                node.Destroy();
                
            }

            this.root = Entity.Empty;
            this.head = Entity.Empty;
            this.count = 0;

        }

        /// <summary>
        /// Returns enumeration of nodes in range [from..to)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetRange(int from, int to) {

            IntrusiveSortedListGeneric<T>.InitializeComponents();

            while (from < to) {

                var node = this.FindNode(from);
                if (node.IsAlive() == true) {

                    yield return node.GetData<IntrusiveSortedListGenericNode<T>>().data;

                } else {

                    ++from;

                }

            }

        }

        /// <summary>
        /// Remove node at index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Returns TRUE if data was found</returns>
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool RemoveAt(int index) {

            if (this.count == 0) return false;

            IntrusiveSortedListGeneric<T>.InitializeComponents();

            var node = this.FindNode(index);
            if (node.IsAlive() == true) {

                this.RemoveNode(in node);
                return true;

            }

            return false;

        }

        /// <summary>
        /// Remove nodes in range [from..to)
        /// </summary>
        /// <param name="from">Must be exists in list, could not be out of list range</param>
        /// <param name="to">May be out of list range, but greater than from</param>
        /// <returns>Returns count of removed elements</returns>
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int RemoveRange(int from, int to) {

            if (this.count == 0) return 0;

            IntrusiveSortedListGeneric<T>.InitializeComponents();

            var count = 0;
            var node = this.FindNode(from);
            if (node.IsAlive() == true) {

                while (from < to) {

                    if (node.IsAlive() == true) {
                        
                        var next = node.GetData<IntrusiveSortedListGenericNode<T>>().next;
                        this.RemoveNode(in node);
                        node = next;
                        ++count;
                        ++from;
                        
                    } else {

                        break;

                    }

                }

            }
            
            return count;

        }

        /// <summary>
        /// Get value by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Entity data. Entity.Empty if not found.</returns>
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public T GetValue(int index) {
            
            if (this.count == 0) return default;

            IntrusiveSortedListGeneric<T>.InitializeComponents();

            var node = this.FindNode(index);
            if (node.IsAlive() == true) {
                
                return node.GetData<IntrusiveSortedListGenericNode<T>>().data;
                
            }
            
            return default;
            
        }
        
        /// <summary>
        /// Remove data from list.
        /// </summary>
        /// <param name="entityData"></param>
        /// <returns>Returns TRUE if data was found</returns>
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T entityData) {

            if (this.count == 0) return false;

            IntrusiveSortedListGeneric<T>.InitializeComponents();

            var node = this.FindNode(in entityData);
            if (node.IsAlive() == true) {

                this.RemoveNode(in node);
                return true;

            }

            return false;
            
        }

        /// <summary>
        /// Remove all nodes data from list.
        /// </summary>
        /// <param name="entityData"></param>
        /// <returns>Returns count of removed elements</returns>
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int RemoveAll(in T entityData) {

            if (this.count == 0) return 0;

            IntrusiveSortedListGeneric<T>.InitializeComponents();

            var root = this.root;
            var count = 0;
            do {

                var nextLink = root.GetData<IntrusiveSortedListGenericNode<T>>();
                if (entityData.Equals(nextLink.data) == true) {
                    
                    this.RemoveNode(root);
                    ++count;

                }

                root = nextLink.next;

            } while (root.IsAlive() == true);
            
            return count;
            
        }

        /// <summary>
        /// Add new data at the end of the list.
        /// </summary>
        /// <param name="entityData"></param>
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Add(in T entityData) {

            IntrusiveSortedListGeneric<T>.InitializeComponents();
            
            var node = IntrusiveSortedListGeneric<T>.CreateNode(in entityData);
            if (this.count == 0) {

                this.root = node;
                this.head = node;

            } else {

                ref var nodeLink = ref node.GetData<IntrusiveSortedListGenericNode<T>>();
                var nodeToAddBefore = this.FindNodeToAddBefore(in entityData);
                if (nodeToAddBefore.IsAlive() == true) {

                    ref var nodeToAddBeforeLink = ref nodeToAddBefore.GetData<IntrusiveSortedListGenericNode<T>>();
                    if (nodeToAddBeforeLink.prev.IsAlive() == false) {
                        
                        // Prev node is null - add current node before all nodes
                        ref var headLink = ref nodeToAddBefore.GetData<IntrusiveSortedListGenericNode<T>>();
                        nodeLink.next = nodeToAddBefore;
                        headLink.prev = node;
                        
                        this.root = node;

                    } else {
                    
                        // Connect node to prev node
                        ref var headLink = ref nodeToAddBeforeLink.prev.GetData<IntrusiveSortedListGenericNode<T>>();
                        headLink.next = node;
                        nodeLink.prev = nodeToAddBeforeLink.prev;
                        nodeLink.next = nodeToAddBefore;
                        nodeToAddBeforeLink.prev = node;

                    }
                    
                } else {

                    // Add node to the end
                    ref var headLink = ref this.head.GetData<IntrusiveSortedListGenericNode<T>>();
                    headLink.next = node;
                    nodeLink.prev = this.head;

                    this.head = node;

                }

            }

            ++this.count;

        }

        /// <summary>
        /// Returns first element.
        /// </summary>
        /// <returns>Returns instance, default if not found</returns>
        public T GetFirst() {

            if (this.root.IsAlive() == false) return default;
            
            return this.root.GetData<IntrusiveSortedListGenericNode<T>>().data;

        }

        /// <summary>
        /// Returns last element.
        /// </summary>
        /// <returns>Returns instance, default if not found</returns>
        public T GetLast() {

            if (this.head.IsAlive() == false) return default;
            
            return this.head.GetData<IntrusiveSortedListGenericNode<T>>().data;

        }

        /// <summary>
        /// Returns last element.
        /// </summary>
        /// <returns>Returns TRUE on success</returns>
        public bool RemoveLast() {

            if (this.head.IsAlive() == false) return false;
            
            this.RemoveNode(in this.head);
            return true;

        }

        /// <summary>
        /// Returns last element.
        /// </summary>
        /// <returns>Returns TRUE on success</returns>
        public bool RemoveFirst() {

            if (this.head.IsAlive() == false) return false;
            
            this.RemoveNode(in this.root);
            return true;

        }

        #region Helpers
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private Entity FindNodeToAddBefore(in T entityData) {
            
            if (this.count == 0) return Entity.Empty;
            
            var node = this.root;
            do {

                var nodeLink = node.GetData<IntrusiveSortedListGenericNode<T>>();
                var comparer = entityData.CompareTo(nodeLink.data);
                if ((this.descending == false && comparer > 0) || (this.descending == true && comparer < 0)) {

                    node = nodeLink.next;
                    
                } else {

                    return node;

                }

            } while (node.IsAlive() == true);
            
            return Entity.Empty;
            
        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private Entity FindNode(in T entityData) {
            
            if (this.count == 0) return Entity.Empty;
            
            var node = this.root;
            do {

                var nodeEntity = node;
                var nodeLink = node.GetData<IntrusiveSortedListGenericNode<T>>();
                var nodeData = nodeLink.data;
                node = nodeLink.next;
                if (entityData.Equals(nodeData) == true) {
                
                    return nodeEntity;

                }

            } while (node.IsAlive() == true);
            
            return Entity.Empty;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private Entity FindNode(int index) {
            
            var idx = 0;
            var node = this.root;
            do {

                var nodeEntity = node;
                var nodeLink = node.GetData<IntrusiveSortedListGenericNode<T>>();
                node = nodeLink.next;
                if (idx == index) {

                    return nodeEntity;

                }

                ++idx;
                if (idx >= this.count) break;

            } while (node.IsAlive() == true);

            return Entity.Empty;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void RemoveNode(in Entity node) {
            
            var link = node.GetData<IntrusiveSortedListGenericNode<T>>();
            if (link.prev.IsAlive() == true) {
                ref var prev = ref link.prev.GetData<IntrusiveSortedListGenericNode<T>>();
                prev.next = link.next;
            }

            if (link.next.IsAlive() == true) {
                ref var next = ref link.next.GetData<IntrusiveSortedListGenericNode<T>>();
                next.prev = link.prev;
            }

            if (node == this.root) this.root = link.next;
            if (node == this.head) this.head = link.prev;
            if (this.head == this.root && this.root == node) {
                    
                this.root = Entity.Empty;
                this.head = Entity.Empty;
                    
            }

            --this.count;
            node.Destroy();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static Entity CreateNode(in T data) {
            
            var node = new Entity("IntrusiveSortedListGenericNode<T>");
            ref var nodeLink = ref node.GetData<IntrusiveSortedListGenericNode<T>>();
            nodeLink.data = data;

            return node;

        }

        private static void InitializeComponents() {

            WorldUtilities.InitComponentTypeId<IntrusiveSortedListGenericNode<T>>();
            ComponentInitializer.Init(ref Worlds.currentWorld.GetStructComponents());

        }
        
        private static class ComponentInitializer {
    
            public static void Init(ref ME.ECS.StructComponentsContainer structComponentsContainer) {
    
                structComponentsContainer.Validate<IntrusiveSortedListGenericNode<T>>(false);
                
            }

        }
        #endregion

    }

}
