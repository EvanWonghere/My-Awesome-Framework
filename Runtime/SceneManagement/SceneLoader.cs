using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using MAF.Singleton;

namespace MAF.SceneManagement
{
    /// <summary>
    /// An optimized singleton class to manage asynchronous scene loading and unloading.
    /// Features progress reporting, cancellation, and validation for improved robustness.
    /// Scenes must be added to the Build Settings to be loaded or unloaded by name/index.
    /// </summary>
    public class SceneLoader : Singleton<SceneLoader>
    {
        #region Events
        /// <summary>
        /// Fired when a scene load or unload operation begins. Ideal for activating a loading screen.
        /// </summary>
        public event Action OnOperationStarted;

        /// <summary>
        /// Fired every frame during a scene load operation. The float value represents the loading progress from 0.0 to 1.0.
        /// </summary>
        public event Action<float> OnLoadProgress;

        /// <summary>
        /// Fired when the scene has finished loading and is fully activated.
        /// </summary>
        public event Action OnOperationComplete;

        /// <summary>
        /// Fired when a scene unload operation begins. The string is the name of the scene being unloaded.
        /// </summary>
        public event Action<string> OnUnloadStarted;

        /// <summary>
        /// Fired when a scene has finished unloading. The string is the name of the scene that was unloaded.
        /// </summary>
        public event Action<string> OnUnloadComplete;
        
        /// <summary>
        /// IMPROVEMENT: Fired when a loading operation is cancelled by the user.
        /// </summary>
        public event Action OnOperationCancelled;
        #endregion

        /// <summary>
        /// Gets whether a scene is currently being loaded or unloaded.
        /// </summary>
        public bool IsBusy { get; private set; }

        private Coroutine _currentOperation;

        #region Public API
        
        /// <summary>
        /// Loads a scene by its enum value. This will unload all other scenes.
        /// </summary>
        public void LoadScene(Scenes scene)
        {
            LoadSceneByIndex((int)scene);
        }

        /// <summary>
        /// Loads a scene by its name. This will unload all other scenes.
        /// </summary>
        public void LoadSceneByName(string sceneName)
        {
            // IMPROVEMENT: Validate that the scene can be loaded before starting.
            if (!ValidateCanStartOperation(sceneName)) return;
            
            _currentOperation = StartCoroutine(LoadSceneRoutine(sceneName, LoadSceneMode.Single));
        }

        /// <summary>
        /// Loads a scene by its build index. This will unload all other scenes.
        /// </summary>
        public void LoadSceneByIndex(int sceneBuildIndex)
        {
            // IMPROVEMENT: Validate that the scene can be loaded before starting.
            if (!ValidateCanStartOperation(sceneBuildIndex)) return;

            _currentOperation = StartCoroutine(LoadSceneRoutine(sceneBuildIndex, LoadSceneMode.Single));
        }

        /// <summary>
        /// Unloads an additively loaded scene by name.
        /// </summary>
        public void UnloadScene(string sceneName)
        {
            if (!ValidateCanStartOperation(sceneName)) return;
            _currentOperation = StartCoroutine(UnloadSceneRoutine(sceneName));
        }
        
        /// <summary>
        /// Unloads one scene and then loads another additively.
        /// </summary>
        public void UnloadSceneThenLoadScene(string sceneToUnload, string sceneToLoad)
        {
            if (!ValidateCanStartOperation(sceneToLoad)) return;
            _currentOperation = StartCoroutine(UnloadThenLoadRoutine(sceneToUnload, sceneToLoad));
        }

        /// <summary>
        /// IMPROVEMENT: Cancels the currently active scene loading or unloading operation.
        /// </summary>
        public void CancelOperation()
        {
            if (!IsBusy)
            {
                Debug.LogWarning("[SceneLoader] No operation is currently in progress to cancel.");
                return;
            }
            if (_currentOperation != null)
            {
                StopCoroutine(_currentOperation);
                _currentOperation = null;
            }

            IsBusy = false;
            OnOperationCancelled?.Invoke();
            Debug.Log("[SceneLoader] Scene operation cancelled by user.");
        }
        #endregion

        #region Private Helpers and Coroutines

        /// <summary>
        /// Central validation check before starting any operation.
        /// </summary>
        private bool ValidateCanStartOperation(object sceneIdentifier = null)
        {
            if (IsBusy)
            {
                Debug.LogWarning("[SceneLoader] An operation is already in progress. New request ignored.");
                return false;
            }

            if (sceneIdentifier == null) return true;

            bool canBeLoaded = sceneIdentifier switch
            {
                string sceneName => Application.CanStreamedLevelBeLoaded(sceneName),
                int sceneBuildIndex => Application.CanStreamedLevelBeLoaded(sceneBuildIndex),
                _ => false
            };
            
            if (!canBeLoaded)
            {
                 Debug.LogError($"[SceneLoader] Scene '{sceneIdentifier}' cannot be loaded. Ensure it is added to the Build Settings and enabled.");
                 return false;
            }

            return true;
        }

        // IMPROVEMENT: This single, generic routine avoids code duplication while public methods provide type safety.
        private IEnumerator LoadSceneRoutine(object sceneIdentifier, LoadSceneMode mode)
        {
            IsBusy = true;
            OnOperationStarted?.Invoke();
            yield return null; 

            AsyncOperation operation = sceneIdentifier switch
            {
                string sceneName => SceneManager.LoadSceneAsync(sceneName, mode),
                int sceneBuildIndex => SceneManager.LoadSceneAsync(sceneBuildIndex, mode),
                _ => null
            };

            if (operation == null)
            {
                Debug.LogError($"[SceneLoader] Invalid scene identifier type: {sceneIdentifier.GetType().Name}");
                IsBusy = false;
                yield break;
            }

            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                OnLoadProgress?.Invoke(Mathf.Clamp01(operation.progress / 0.9f));
                yield return null;
            }
            
            OnLoadProgress?.Invoke(1.0f);
            
            // Allow a brief moment for UI transitions before activating the new scene
            yield return new WaitForSeconds(0.5f);

            operation.allowSceneActivation = true;

            while (!operation.isDone)
                yield return null;

            OnOperationComplete?.Invoke();
            IsBusy = false;
            _currentOperation = null;
        }

        private IEnumerator UnloadSceneRoutine(string sceneName)
        {
            IsBusy = true;
            OnUnloadStarted?.Invoke(sceneName);
            yield return SceneManager.UnloadSceneAsync(sceneName);
            OnUnloadComplete?.Invoke(sceneName);
            IsBusy = false;
            _currentOperation = null;
        }

        private IEnumerator UnloadThenLoadRoutine(string sceneToUnload, string sceneToLoad)
        {
            IsBusy = true;
            
            // Unload the old scene
            OnUnloadStarted?.Invoke(sceneToUnload);
            yield return SceneManager.UnloadSceneAsync(sceneToUnload);
            OnUnloadComplete?.Invoke(sceneToUnload);
            
            // Load the new one additively
            yield return StartCoroutine(LoadSceneRoutine(sceneToLoad, LoadSceneMode.Additive));

            IsBusy = false;
            _currentOperation = null;
        }
        #endregion

        /// <summary>
        /// IMPROVEMENT: Clean up event subscribers when the singleton is destroyed to prevent memory leaks.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnOperationStarted = null;
            OnLoadProgress = null;
            OnOperationComplete = null;
            OnUnloadStarted = null;
            OnUnloadComplete = null;
            OnOperationCancelled = null;
        }
    }
}