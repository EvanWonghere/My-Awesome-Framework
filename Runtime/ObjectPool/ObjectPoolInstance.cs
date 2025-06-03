// File: ObjectPoolInstance.cs
using System.Collections.Generic;
using UnityEngine;

namespace MAF.ObjectPool
{
    /// <summary>
    /// Manages a pool of GameObjects for a specific prefab.
    /// Handles instantiation, retrieval, and return of objects.
    /// </summary>
    public class ObjectPoolInstance
    {
        private readonly GameObject _prefab;
        private readonly int _initialPoolSize;
        private readonly bool _canGrow;
        private readonly Transform _poolParent; // Parent for inactive pooled objects

        private readonly Queue<GameObject> _pooledObjects = new Queue<GameObject>();

        /// <summary>
        /// Gets the name of the prefab this pool is managing.
        /// Returns "NullPrefab" if the prefab is null.
        /// </summary>
        public string PrefabName => _prefab != null ? _prefab.name : "NullPrefab_PoolInstance";

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPoolInstance"/> class.
        /// </summary>
        /// <param name="prefab">The prefab to pool. Must not be null.</param>
        /// <param name="initialSize">The initial number of objects to pre-instantiate. Must be non-negative.</param>
        /// <param name="canGrow">Whether the pool can create new objects if it runs out.</param>
        /// <param name="parentTransform">An optional parent transform to hold inactive pooled objects. Helps keep the scene hierarchy organized.</param>
        public ObjectPoolInstance(GameObject prefab, int initialSize, bool canGrow, Transform parentTransform = null)
        {
            if (prefab == null)
            {
                Debug.LogError("[ObjectPoolInstance] Prefab cannot be null. Pool not created.");
                return;
            }
            if (initialSize < 0)
            {
                 Debug.LogWarning($"[ObjectPoolInstance] Initial size for prefab '{prefab.name}' was negative ({initialSize}). Setting to 0.");
                 initialSize = 0;
            }

            _prefab = prefab;
            _initialPoolSize = initialSize;
            _canGrow = canGrow;
            _poolParent = parentTransform;

            InitializePool();
        }

        /// <summary>
        /// Pre-instantiates the initial number of objects for the pool.
        /// </summary>
        private void InitializePool()
        {
            for (int i = 0; i < _initialPoolSize; i++)
            {
                AddObjectToPoolInternal(false); // Add initially inactive objects
            }
        }

        /// <summary>
        /// Internal method to create a new GameObject from the prefab and add it to the pool structure.
        /// </summary>
        /// <param name="setActiveOnCreate">If true, the new object will be active upon creation. Otherwise, it will be inactive.</param>
        /// <returns>The newly created GameObject, or null if the prefab is null.</returns>
        private GameObject AddObjectToPoolInternal(bool setActiveOnCreate)
        {
            if (_prefab == null) return null; // Should have been caught by constructor but as a safeguard

            var newObj = GameObject.Instantiate(_prefab);
            newObj.name = _prefab.name; // Remove "(Clone)" suffix for cleaner naming

            if (_poolParent != null)
            {
                newObj.transform.SetParent(_poolParent);
            }

            // Add PooledObjectInfo and link it back to this pool instance
            var pooledInfo = newObj.GetComponent<PooledObjectInfo>();
            if (pooledInfo == null)
            {
                pooledInfo = newObj.AddComponent<PooledObjectInfo>();
            }
            pooledInfo.OwningPool = this;

            newObj.SetActive(setActiveOnCreate);
            if (!setActiveOnCreate)
            {
                _pooledObjects.Enqueue(newObj); // Only enqueue if it's being added as inactive (i.e., to the pool reserve)
            }
            return newObj;
        }

        /// <summary>
        /// Retrieves an object from the pool. If the pool is empty and can grow, a new object is created.
        /// The retrieved object will be active.
        /// </summary>
        /// <returns>An active GameObject from the pool, or null if the pool is empty and cannot grow or if an error occurred.</returns>
        public GameObject GetObject()
        {
            if (_pooledObjects.Count > 0)
            {
                var obj = _pooledObjects.Dequeue();
                // Detach from the pool's parent transform so it can be moved freely in the scene.
                // User is responsible for re-parenting if needed.
                obj.transform.SetParent(null); 
                obj.SetActive(true);
                return obj;
            }

            if (_canGrow)
            {
                // Debug.LogWarning($"[ObjectPoolInstance] Pool for '{PrefabName}' ran out, growing pool by one.");
                return AddObjectToPoolInternal(true); // Create a new one and return it active
            }

            Debug.LogWarning($"[ObjectPoolInstance] Pool for '{PrefabName}' is empty and configured not to grow. Cannot provide an object.");
            return null;
        }

        /// <summary>
        /// Returns a GameObject to the pool. The object will be deactivated and re-parented (if a pool parent exists).
        /// </summary>
        /// <param name="obj">The GameObject to return. If null, the method does nothing.</param>
        public void ReturnObject(GameObject obj)
        {
            if (obj == null)
            {
                // Debug.LogWarning("[ObjectPoolInstance] Attempted to return a null object to the pool.");
                return;
            }
            
            // Ensure the object is indeed from this pool if possible (sanity check, PooledObjectInfo helps with this at manager level)
            // var poi = obj.GetComponent<PooledObjectInfo>();
            // if (poi == null || poi.OwningPool != this) {
            //     Debug.LogWarning($"[ObjectPoolInstance] Object '{obj.name}' being returned to pool for '{PrefabName}' does not seem to belong to this pool. This might indicate an issue.");
            //     // Optionally destroy it or just proceed. For now, we assume manager level check is sufficient.
            // }

            obj.SetActive(false);
            if (_poolParent != null)
            {
                obj.transform.SetParent(_poolParent);
            }
            else
            {
                // If no pool parent, the object remains at the root or its last parent.
                // This is generally fine, but a pool parent is recommended for organization.
            }
            _pooledObjects.Enqueue(obj);
        }

        /// <summary>
        /// Clears the pool by destroying all currently pooled GameObjects and the pool's parent GameObject.
        /// After calling this, the pool instance should generally not be used further unless re-initialized.
        /// </summary>
        public void Clear()
        {
            Debug.Log($"[ObjectPoolInstance] Clearing pool for '{PrefabName}'. Destroying {_pooledObjects.Count} inactive objects.");
            while (_pooledObjects.Count > 0)
            {
                var obj = _pooledObjects.Dequeue();
                GameObject.Destroy(obj);
            }

            if (_poolParent != null)
            {
                Debug.Log($"[ObjectPoolInstance] Destroying pool parent for '{PrefabName}'.");
                GameObject.Destroy(_poolParent.gameObject);
            }
            // Note: Active objects taken from the pool are not tracked by this instance anymore,
            // so they won't be destroyed by this Clear method. They need to be returned or destroyed manually.
        }
    }
}