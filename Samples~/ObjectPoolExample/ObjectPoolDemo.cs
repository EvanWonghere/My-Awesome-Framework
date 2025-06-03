using UnityEngine;
using MAF.ObjectPool; // Your framework's namespace
using System.Collections.Generic;

namespace MAF.Samples
{
    public class ObjectPoolDemo : MonoBehaviour
    {
        public GameObject projectilePrefab; // Assign a simple prefab (e.g., a Sphere) in Inspector
        public float spawnInterval = 0.5f;
        public float projectileLifetime = 3f;

        private float _timer;
        private List<GameObject> _activeProjectiles = new List<GameObject>();

        void Start()
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("ObjectPoolDemo: Projectile Prefab is not assigned!");
                this.enabled = false;
                return;
            }

            // Pre-warm the pool for the projectile
            ObjectPoolManager.Instance.CreatePool(projectilePrefab, 10, true);
            Debug.Log("ObjectPoolDemo: Projectile pool created.");
        }

        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= spawnInterval)
            {
                _timer = 0f;
                SpawnProjectile();
            }
        }

        void SpawnProjectile()
        {
            if (projectilePrefab == null) return;

            GameObject projectile = ObjectPoolManager.Instance.GetObject(
                projectilePrefab, 
                transform.position + transform.forward, // Spawn in front of this spawner
                Quaternion.identity
            );

            if (projectile != null)
            {
                _activeProjectiles.Add(projectile);
                // Add a simple script to the projectile prefab to handle its own despawn
                // or manage lifetime here. For this demo, we'll manage it here.
                StartCoroutine(DespawnAfterTime(projectile, projectileLifetime));
                // Give it some velocity if it has a Rigidbody
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = transform.forward * 10f;
                }
                 Debug.Log($"ObjectPoolDemo: Spawned projectile {projectile.name}. Active projectiles: {_activeProjectiles.Count}");
            }
            else
            {
                Debug.LogWarning("ObjectPoolDemo: Could not get projectile from pool.");
            }
        }

        System.Collections.IEnumerator DespawnAfterTime(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (obj != null && obj.activeSelf) // Check if it wasn't already returned
            {
                ObjectPoolManager.Instance.ReturnObject(obj);
                _activeProjectiles.Remove(obj);
                Debug.Log($"ObjectPoolDemo: Returned projectile {obj.name} after lifetime. Active projectiles: {_activeProjectiles.Count}");
            }
        }
    }
}