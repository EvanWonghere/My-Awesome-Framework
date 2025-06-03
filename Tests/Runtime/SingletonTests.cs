using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using MAF.Singleton; // Your namespace

namespace MAF.Tests.Runtime
{
    // A concrete Singleton for testing purposes
    public class TestSingleton : Singleton<TestSingleton>
    {
        public bool IsInitialized { get; private set; }
        protected override void Awake() 
        {
            base.Awake(); // Important to call the base Singleton's Awake
            IsInitialized = true;
            // Debug.Log("TestSingleton Awake called, IsInitialized: " + IsInitialized);
        }
        public void DoSomething() { }
    }

    // Another concrete Singleton for uniqueness test
    public class AnotherTestSingleton : Singleton<AnotherTestSingleton> { }


    public class SingletonTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Try to clean up any existing singletons from previous tests
            // This is tricky because they might be DontDestroyOnLoad
            TestSingleton existingTestSingleton = Object.FindFirstObjectByType<TestSingleton>();
            if (existingTestSingleton != null) Object.DestroyImmediate(existingTestSingleton.gameObject);

            AnotherTestSingleton existingAnotherSingleton = Object.FindFirstObjectByType<AnotherTestSingleton>();
            if (existingAnotherSingleton != null) Object.DestroyImmediate(existingAnotherSingleton.gameObject);

            yield return null; // Wait for destruction to occur
        }

        [UnityTest]
        public IEnumerator Singleton_Instance_CreatesInstanceIfNotExists()
        {
            Assert.IsNull(Object.FindFirstObjectByType<TestSingleton>(), "Pre-condition: No TestSingleton should exist.");

            TestSingleton instance = TestSingleton.Instance;
            yield return null; // Allow a frame for Awake to run

            Assert.IsNotNull(instance, "Instance should not be null after access.");
            Assert.IsTrue(instance.IsInitialized, "Singleton instance was not initialized (Awake not run properly).");
            Assert.AreEqual(1, Object.FindObjectsByType<TestSingleton>(FindObjectsSortMode.None).Length, "More than one instance was created.");
        }

        [UnityTest]
        public IEnumerator Singleton_Instance_ReturnsSameInstance()
        {
            TestSingleton instance1 = TestSingleton.Instance;
            yield return null; 
            TestSingleton instance2 = TestSingleton.Instance;
            yield return null;

            Assert.AreSame(instance1, instance2, "Instance property returned different instances.");
        }

        [UnityTest]
        public IEnumerator Singleton_MultipleSingletonTypes_AreIndependent()
        {
            TestSingleton testInstance = TestSingleton.Instance;
            yield return null;
            AnotherTestSingleton anotherInstance = AnotherTestSingleton.Instance;
            yield return null;

            Assert.IsNotNull(testInstance, "TestSingleton instance should exist.");
            Assert.IsNotNull(anotherInstance, "AnotherTestSingleton instance should exist.");
            Assert.AreNotSame(testInstance, anotherInstance as MonoBehaviour, "Instances of different Singleton types should not be the same.");
        }

        // DontDestroyOnLoad is harder to test in isolation without scene loading,
        // but the Singleton<T> implementation itself calls DontDestroyOnLoad.
        // Manual verification or integration tests involving scene loads would confirm DontDestroyOnLoad.
    }
}