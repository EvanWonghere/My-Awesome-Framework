// File: ObjectPoolManager.cs
using System.Collections.Generic;
using UnityEngine;
using MAF.Singleton; // Assuming your Singleton<T> is in this namespace

namespace MAF.ObjectPool
{
    /// <summary>
    /// Manages multiple <see cref="ObjectPoolInstance"/>s, each for a different prefab.
    /// Provides a centralized Singleton interface to create pools, get, and return pooled GameObjects.
    /// </summary>
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        // Dictionary to store pools, keyed by the prefab's name.
        private readonly Dictionary<string, ObjectPoolInstance> _pools = new Dictionary<string, ObjectPoolInstance>();
        // Parent transform in the scene to hold all GameObjects related to specific pools (e.g., parents of inactive objects).
        private Transform _poolCollectionRoot;

        /// <summary>
        /// Unity's Awake method. Initializes the ObjectPoolManager.
        /// Sets up a root GameObject to organize pool-related GameObjects in the scene.
        /// </summary>
        protected override void Awake()
        {
            // It's crucial that Singleton<T>.Awake is `protected virtual` for base.Awake() to be callable
            // and for the singleton pattern to work correctly with inheritance.
            base.Awake(); // Initialize Singleton base logic (handles DontDestroyOnLoad and instance uniqueness)

            if (Instance == this) // Ensure this is the true singleton instance before proceeding
            {
                // Create a parent GameObject in the scene to hold all individual pool parents for better organization.
                // This helps keep the scene hierarchy clean.
                _poolCollectionRoot = new GameObject("ObjectPoolCollection").transform;
                // Parent it to this manager so if the manager is DontDestroyOnLoad, the collection root also persists.
                _poolCollectionRoot.SetParent(this.transform);
            }
        }

        /// <summary>
        /// Creates (pre-warms) an object pool for a specific prefab.
        /// If a pool for this prefab name already exists, this method logs a warning and does nothing.
        /// </summary>
        /// <param name="prefab">The prefab to create a pool for. Must not be null.</param>
        /// <param name="initialSize">The initial number of objects to allocate in the pool. Defaults to 10.</param>
        /// <param name="canGrow">Whether the pool is allowed to create new objects if it runs out. Defaults to true.</param>
        public void CreatePool(GameObject prefab, int initialSize = 10, bool canGrow = true)
        {
            if (prefab == null)
            {
                Debug.LogError("[ObjectPoolManager] Cannot create pool for a null prefab.");
                return;
            }

            string prefabKey = prefab.name; // Using prefab name as key. Consider InstanceID if names can clash.

            if (!_pools.ContainsKey(prefabKey))
            {
                // Create a dedicated parent GameObject for this specific pool's inactive objects.
                // This helps in organizing objects belonging to different pools under the _poolCollectionRoot.
                Transform specificPoolParent = new GameObject($"{prefabKey}_PoolInstance").transform;
                specificPoolParent.SetParent(_poolCollectionRoot);

                var newPool = new ObjectPoolInstance(prefab, initialSize, canGrow, specificPoolParent);
                _pools.Add(prefabKey, newPool);
                // Debug.Log($"[ObjectPoolManager] Pool created for '{prefabKey}' with initial size {initialSize}. Can grow: {canGrow}.");
            }
            else
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool for '{prefabKey}' already exists. Not creating a new one.");
            }
        }

        /// <summary>
        /// Retrieves a GameObject from the pool for the specified prefab.
        /// If a pool for this prefab does not exist, it will be created automatically with default settings
        /// (initial size 10, can grow true).
        /// </summary>
        /// <param name="prefab">The prefab of the object to retrieve. Must not be null.</param>
        /// <param name="position">Optional position to set for the retrieved object.</param>
        /// <param name="rotation">Optional rotation to set for the retrieved object.</param>
        /// <returns>An active GameObject instance from the pool, or null if an error occurs or the pool cannot provide an object.</returns>
        public GameObject GetObject(GameObject prefab, Vector3? position = null, Quaternion? rotation = null)
        {
            if (prefab == null)
            {
                Debug.LogError("[ObjectPoolManager] Cannot get object for a null prefab.");
                return null;
            }

            string prefabKey = prefab.name;

            if (!_pools.TryGetValue(prefabKey, out var pool))
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool for '{prefabKey}' not found. Creating with default settings (Initial Size: 10, Can Grow: true).");
                CreatePool(prefab, 10, true); // Create with default size and canGrow
                // Try to get the newly created pool.
                if (!_pools.TryGetValue(prefabKey, out pool))
                {
                     Debug.LogError($"[ObjectPoolManager] Critical error: Failed to create or retrieve pool for '{prefabKey}' after attempting creation.");
                     return null; // Critical failure if pool still not found after creation attempt
                }
            }
            
            GameObject obj = pool.GetObject();
            if (obj != null)
            {
                // Set position and rotation if provided.
                // The object is already active and detached from the pool parent by pool.GetObject().
                if (position.HasValue) obj.transform.position = position.Value;
                if (rotation.HasValue) obj.transform.rotation = rotation.Value;
            }
            else
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool for '{prefabKey}' could not provide an object (it might be empty and non-growable).");
            }
            return obj;
        }

        /// <summary>
        /// Returns a GameObject to its appropriate pool.
        /// The object must have a <see cref="PooledObjectInfo"/> component that correctly references its <see cref="ObjectPoolInstance"/>.
        /// If the object is null or does not have valid pool information, it will be logged and potentially destroyed.
        /// </summary>
        /// <param name="obj">The GameObject to return to the pool.</param>
        public void ReturnObject(GameObject obj)
        {
            if (obj == null)
            {
                // Debug.LogWarning("[ObjectPoolManager] Attempted to return a null object.");
                return;
            }

            var pooledInfo = obj.GetComponent<PooledObjectInfo>();
            if (pooledInfo != null && pooledInfo.OwningPool != null)
            {
                pooledInfo.OwningPool.ReturnObject(obj);
            }
            else
            {
                Debug.LogWarning($"[ObjectPoolManager] Object '{obj.name}' is being returned but does not have valid PooledObjectInfo or its OwningPool is not set. The object will be destroyed to prevent issues.", obj);
                GameObject.Destroy(obj);
            }
        }

        /// <summary>
        /// Clears all inactive GameObjects from a specific pool and, if requested, destroys the pool instance itself
        /// (including its parent GameObject for inactive objects).
        /// Active objects retrieved from this pool are not affected and must be managed separately.
        /// </summary>
        /// <param name="prefabName">The name of the prefab whose pool is to be cleared (case-sensitive).</param>
        /// <param name="destroyPoolInstance">If true, removes the pool from management and destroys its associated GameObjects after clearing inactive objects.</param>
        public void ClearPool(string prefabName, bool destroyPoolInstance = false)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                Debug.LogWarning("[ObjectPoolManager] Cannot clear pool for a null or empty prefab name.");
                return;
            }

            if (_pools.TryGetValue(prefabName, out var pool))
            {
                pool.Clear(); // This will destroy inactive objects and the pool's parent transform

                if (destroyPoolInstance)
                {
                    _pools.Remove(prefabName);
                    Debug.Log($"[ObjectPoolManager] Pool instance for '{prefabName}' has been destroyed and removed from management.");
                }
                else
                {
                     Debug.Log($"[ObjectPoolManager] Inactive objects in pool for '{prefabName}' have been cleared. Pool instance remains.");
                }
            }
            else
            {
                Debug.LogWarning($"[ObjectPoolManager] Cannot clear pool for '{prefabName}', as it does not exist or was already cleared.");
            }
        }

        /// <summary>
        /// Clears all inactive objects from all pools managed by this manager.
        /// Optionally, also destroys all pool instances and removes them from management.
        /// </summary>
        /// <param name="destroyPoolInstances">If true, all pool instances will be destroyed after their inactive objects are cleared.</param>
        public void ClearAllPools(bool destroyPoolInstances = false)
        {
            Debug.Log($"[ObjectPoolManager] Clearing all pools. Destroy pool instances: {destroyPoolInstances}");
            // Iterate over a copy of keys since the dictionary might be modified if destroyPoolInstances is true
            List<string> poolKeys = new List<string>(_pools.Keys);
            foreach (string key in poolKeys)
            {
                ClearPool(key, destroyPoolInstances);
            }

            if (destroyPoolInstances && _pools.Count > 0) // Should be 0 if all ClearPool calls were successful
            {
                 Debug.LogWarning("[ObjectPoolManager] Some pools may not have been cleared correctly from the dictionary during ClearAllPools. Clearing dictionary now.");
                _pools.Clear(); // Final sweep
            }
             Debug.Log("[ObjectPoolManager] Finished ClearAllPools operation.");
        }

        /// <summary>
        /// Unity's OnDestroy method.
        /// Ensures that if the manager GameObject is destroyed, all pools are also cleared.
        /// </summary>
        protected virtual void OnDestroy()
        {
            // If this Singleton is destroyed, it's good practice to clean up managed resources.
            // This prevents orphaned GameObjects if the pools created their own parent GameObjects.
            ClearAllPools(true); // Destroy all pool instances and their GameObjects
            if (_poolCollectionRoot != null)
            {
                Destroy(_poolCollectionRoot.gameObject);
            }
        }
    }
}