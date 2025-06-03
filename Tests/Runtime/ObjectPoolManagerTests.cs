using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using MAF.ObjectPool; // Your namespace
using MAF.Singleton;   // Required by ObjectPoolManager

namespace MAF.Tests.Runtime
{
    public class ObjectPoolManagerTests
    {
        private GameObject _testPrefab;
        private ObjectPoolManager _managerInstance;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Ensure a clean ObjectPoolManager instance for each test run
            // If an instance persists from a previous test, destroy it.
            var existingManager = ObjectPoolManager.Instance; // This will create one if none exists
            if (existingManager != null && existingManager.gameObject.scene.IsValid()) // Check if it's a scene object
            {
                // We need a way to reset the manager or ensure it's fresh.
                // Destroying and waiting is one way for MonoBehaviours.
                // A proper Reset method on the Singleton would be cleaner.
            }

            _testPrefab = new GameObject("TestPrefabForPool");
            // Add a component to make it a valid prefab for some tests if needed, e.g., Rigidbody
            _testPrefab.AddComponent<BoxCollider>(); 

            // The ObjectPoolManager is a Singleton, it will create itself if not present.
            // Ensure it's ready before tests run.
            _managerInstance = ObjectPoolManager.Instance; 
            yield return null; // Wait a frame for Awake to run if it's newly created.

            // Clean up any pools from previous tests
            _managerInstance.ClearAllPools(true);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_managerInstance != null)
            {
                 _managerInstance.ClearAllPools(true); // Clear pools and destroy their gameobjects
            }
            if (_testPrefab != null)
            {
                Object.Destroy(_testPrefab);
            }
            // Potentially destroy the manager instance if it's not DontDestroyOnLoad or if you want full cleanup
            // However, Singletons are often designed to persist. If your Singleton.cs DontDestroyOnLoad is active,
            // destroying it here might conflict with its design. For testing, you might need a specific test setup.
            // For now, we rely on ClearAllPools.
            yield return null;
        }

        [UnityTest]
        public IEnumerator ObjectPoolManager_CreatesPool()
        {
            Assert.IsNotNull(_managerInstance, "ObjectPoolManager instance should exist.");
            _managerInstance.CreatePool(_testPrefab, 5, true);
            // Verification could be internal state or checking for the pool's parent GameObject
            var poolParent = _managerInstance.transform.Find("ObjectPoolCollection")?.Find($"{_testPrefab.name}_PoolInstance");
            Assert.IsNotNull(poolParent, "Pool parent GameObject was not created.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator ObjectPoolManager_GetObjectFromPool()
        {
            _managerInstance.CreatePool(_testPrefab, 1, true);
            GameObject obj = _managerInstance.GetObject(_testPrefab);

            Assert.IsNotNull(obj, "GetObject returned null.");
            Assert.IsTrue(obj.activeSelf, "Retrieved object should be active.");
            Assert.AreEqual(_testPrefab.name, obj.name, "Retrieved object name doesn't match prefab name."); // PoolInstance renames it
            Assert.IsNotNull(obj.GetComponent<PooledObjectInfo>(), "Retrieved object missing PooledObjectInfo.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator ObjectPoolManager_ReturnObjectToPool()
        {
            _managerInstance.CreatePool(_testPrefab, 1, true);
            GameObject obj = _managerInstance.GetObject(_testPrefab);
            Assert.IsNotNull(obj, "Failed to get object for return test.");

            _managerInstance.ReturnObject(obj);
            yield return null; // Give a frame for SetActive(false) to apply

            Assert.IsFalse(obj.activeSelf, "Returned object should be inactive.");
            // To further verify, try to get an object again - it should be the same instance.
            GameObject sameObj = _managerInstance.GetObject(_testPrefab);
            Assert.AreSame(obj, sameObj, "Did not get the same object instance after returning and getting again.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator ObjectPoolManager_PoolCanGrow()
        {
            _managerInstance.CreatePool(_testPrefab, 1, true); // Initial size 1, can grow
            GameObject obj1 = _managerInstance.GetObject(_testPrefab);
            GameObject obj2 = _managerInstance.GetObject(_testPrefab); // Should grow

            Assert.IsNotNull(obj1, "First object was null.");
            Assert.IsNotNull(obj2, "Second object (from grown pool) was null.");
            Assert.AreNotSame(obj1, obj2, "Grown pool should provide a new instance.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator ObjectPoolManager_PoolCannotGrow_ReturnsNullWhenEmpty()
        {
            _managerInstance.CreatePool(_testPrefab, 1, false); // Initial size 1, cannot grow
            GameObject obj1 = _managerInstance.GetObject(_testPrefab);
            Assert.IsNotNull(obj1);

            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, $"[ObjectPoolInstance] Pool for '{_testPrefab.name}' is empty and configured not to grow. Cannot provide an object.");
            GameObject obj2 = _managerInstance.GetObject(_testPrefab); // Should return null

            Assert.IsNull(obj2, "Pool should return null when empty and cannot grow.");
            yield return null;
        }
    }
}