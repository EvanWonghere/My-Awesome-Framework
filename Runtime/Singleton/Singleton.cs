// File: Singleton.cs
using UnityEngine;

namespace MAF.Singleton
{
    /// <summary>
    /// A generic base class for creating MonoBehaviour singletons.
    /// Ensures that only one instance of the Singleton exists.
    /// If an instance does not exist, it will be created automatically.
    /// </summary>
    /// <typeparam name="T">The type of the MonoBehaviour to be a singleton.</typeparam>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object Lock = new object(); // For thread safety during instance creation, though Unity API calls are main-thread only.
        private static bool _isApplicationQuitting = false;

        /// <summary>
        /// Gets the singleton instance of this MonoBehaviour.
        /// If an instance does not exist, it tries to find one or creates a new one.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_isApplicationQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                lock (Lock) // Ensure only one thread can enter this block at a time
                {
                    if (_instance == null)
                    {
                        // Try to find an existing instance in the scene
                        _instance = FindFirstObjectByType<T>();

                        if (_instance == null)
                        {
                            // No instance found, create a new GameObject and add the component
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"{typeof(T).Name} (Singleton)";

                            // Make instance persistent across scene loads
                            DontDestroyOnLoad(singletonObject);
                            Debug.Log($"[Singleton] An instance of {typeof(T)} is needed in the scene, so '{singletonObject.name}' was created with DontDestroyOnLoad.");
                        }
                        else
                        {
                            // An instance was found in the scene. Ensure it persists.
                            // This is important if the instance was placed in the scene manually.
                            Debug.Log($"[Singleton] Using instance already created: {_instance.gameObject.name}");
                            DontDestroyOnLoad(_instance.gameObject); 
                        }
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Unity's Awake method. Called when the script instance is being loaded.
        /// This method handles instance uniqueness and persistence.
        /// It should be 'protected virtual' to allow derived classes to override 
        /// and call base.Awake() if needed.
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                // If no instance is yet set, assign this instance.
                _instance = this as T;
                DontDestroyOnLoad(gameObject); // Ensure this instance persists.
            }
            else if (_instance != this)
            {
                // If an instance already exists and it's not this one, destroy this GameObject.
                Debug.LogWarning($"[Singleton] Another instance of {typeof(T)} already exists. Destroying duplicate: {gameObject.name}");
                Destroy(gameObject);
            }
            // If _instance == this, it means this is the one true instance, already handled by Instance getter or a previous Awake.
        }

        /// <summary>
        /// Unity's OnApplicationQuit method. Called when the application is about to quit.
        /// Sets a flag to prevent instance recreation during the quitting process.
        /// </summary>
        private void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }

        /// <summary>
        /// Protected default constructor to prevent creating an instance with 'new'.
        /// Singletons should be accessed via the Instance property.
        /// </summary>
        protected Singleton() { }
    }
}