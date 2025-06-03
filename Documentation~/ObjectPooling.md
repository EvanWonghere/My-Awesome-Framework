# Object Pooling System

The Object Pooling system helps manage frequently created and destroyed GameObjects (like bullets, effects, or enemies) to improve performance by reusing objects instead of instantiating and destroying them repeatedly.

## Core Classes
- `MAF.ObjectPool.ObjectPoolManager`: A `Singleton` that manages all object pools.
- `MAF.ObjectPool.ObjectPoolInstance`: Manages a pool for a single specific prefab (internal).
- `MAF.ObjectPool.PooledObjectInfo`: A component automatically added to pooled objects to track their origin.

## Usage

**1. Creating/Pre-warming a Pool:**

It's good practice to create pools for your prefabs during an initialization phase (e.g., game loading or level start).
```csharp
public GameObject enemyPrefab; // Assign in Inspector

void Start()
{
    // Get the ObjectPoolManager instance and create a pool
    ObjectPoolManager.Instance.CreatePool(enemyPrefab, 20, true); 
    // Args: prefab, initial size, can the pool grow?
}
```

**2. Getting an Object from the Pool:**

When you need a new instance of a pooled object:
```csharp
// Get an enemy at a specific position and rotation
GameObject newEnemy = ObjectPoolManager.Instance.GetObject(
    enemyPrefab, 
    new Vector3(0, 1, 10), 
    Quaternion.identity
);

if (newEnemy != null)
{
    // Initialize the enemy, set its stats, etc.
    // The object is already active.
}
```
If a pool doesn't exist for the requested prefab when `GetObject` is called, the `ObjectPoolManager` will attempt to create one with default settings.

**3. Returning an Object to the Pool:**

When an object is no longer needed (e.g., enemy defeated, bullet impact):

```csharp
// In your enemy script, for example:
// void OnDeath()
// {
//     ObjectPoolManager.Instance.ReturnObject(this.gameObject);
// }
```
The `ObjectPoolManager` uses the `PooledObjectInfo` component (automatically added when `GetObject` is called) to determine which pool the object belongs to. The object will be deactivated and placed back into its pool.

## `ObjectPoolManager` Key Methods:
- `CreatePool(GameObject prefab, int initialSize = 10, bool canGrow = true)`: Pre-warms a pool.
- `GetObject(GameObject prefab, Vector3? position = null, Quaternion? rotation = null)`: Retrieves an object.
- `ReturnObject(GameObject obj)`: Returns an object to its pool.
- `ClearPool(string prefabName, bool destroyPoolInstance = false)`: Clears inactive objects from a specific pool and optionally destroys the pool itself.
- `ClearAllPools(bool destroyPoolInstances = false)`: Clears all managed pools.

## Important Notes
- **Hierarchy:** The `ObjectPoolManager` creates a root GameObject ("ObjectPoolCollection") and child GameObjects for each pool instance to keep inactive pooled objects organized in the scene hierarchy.
- `PooledObjectInfo`: Do not remove this component from pooled objects.
- **Main Thread Only:** All `ObjectPoolManager` methods that interact with GameObjects (instantiation, activation) must be called from the main Unity thread.

