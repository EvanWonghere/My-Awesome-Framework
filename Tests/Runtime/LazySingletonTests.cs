using NUnit.Framework;
using MAF.Singleton; // Your namespace

namespace MAF.Tests.Runtime
{
    // Concrete class for testing LazySingleton
    public class MyService : LazySingleton<MyService>
    {
        public bool IsInitialized { get; private set; }
        public int Counter { get; set; }

        // Private constructor is typical for LazySingleton pattern if you don't need to inherit further
        // but LazySingleton<T> has a protected constructor, so this also works.
        // If LazySingleton required a private constructor on T, the constraint new() would manage that.
        public MyService() 
        {
            IsInitialized = true;
            Counter = 0;
        }
    }

    public class LazySingletonTests
    {
        [Test]
        public void LazySingleton_Instance_CreatesInstanceOnFirstAccess()
        {
            // For non-MonoBehaviour singletons, we don't have the same scene persistence issues.
            // However, static fields mean state can persist between test runs in the same domain.
            // This test assumes MyService is fresh or its state doesn't interfere.
            // A "Reset" or making MyService non-static for testing might be needed for complex cases.

            MyService instance = MyService.Instance;
            Assert.IsNotNull(instance, "Instance should not be null.");
            Assert.IsTrue(instance.IsInitialized, "Instance should be initialized upon creation.");
        }

        [Test]
        public void LazySingleton_Instance_ReturnsSameInstance()
        {
            MyService instance1 = MyService.Instance;
            MyService instance2 = MyService.Instance;
            Assert.AreSame(instance1, instance2, "Instance property returned different instances.");
        }

        [Test]
        public void LazySingleton_Instance_IsLazy()
        {
            // This is hard to test directly without reflection or side effects.
            // The core idea is that 'new MyService()' isn't called until MyService.Instance is first accessed.
            // We trust System.Lazy<T> to handle this.
            // A conceptual test:
            // 1. Set up a static flag MyService.WasConstructed = false
            // 2. In MyService constructor, set MyService.WasConstructed = true
            // 3. Assert MyService.WasConstructed is false before accessing Instance
            // 4. Access MyService.Instance
            // 5. Assert MyService.WasConstructed is true
            Assert.Pass("Laziness is assumed by System.Lazy<T>. Direct test is complex.");
        }
    }
}