using UnityEngine;
using UnityEngine.UI;
using MAF.SceneManagement;

namespace MAF.Samples
{
    /// <summary>
    /// Demonstrates how to use the refactored SceneLoader, including the type-safe Scenes enum
    /// and the operation cancellation feature.
    /// </summary>
    public class SceneLoaderDemo : MonoBehaviour
    {
        [Header("Scene To Load")]
        [Tooltip("Select the scene you want to load from the dropdown. This list is generated from your Build Settings.")]
        [SerializeField] private Scenes targetScene; // IMPROVEMENT: Using the enum now for type-safety!

        [Header("UI Elements")]
        [Tooltip("The parent GameObject for the entire loading screen UI.")]
        [SerializeField] private GameObject loadingScreenPanel;
        [Tooltip("A UI Slider to display the loading progress.")]
        [SerializeField] private Slider progressBar;
        [Tooltip("A button that becomes visible during loading to allow cancellation.")]
        [SerializeField] private Button cancelButton;

        void Awake()
        {
            // Subscribe to all SceneLoader events to drive the UI
            SceneLoader.Instance.OnOperationStarted += HandleOperationStarted;
            SceneLoader.Instance.OnLoadProgress += HandleLoadProgress;
            SceneLoader.Instance.OnOperationComplete += HandleOperationComplete;
            SceneLoader.Instance.OnOperationCancelled += HandleOperationCancelled;

            // Also add a listener to our UI cancel button
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(TriggerCancelLoad);
            }
        }

        void OnDestroy()
        {
            // Always unsubscribe from events when the object is destroyed to prevent memory leaks
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.OnOperationStarted -= HandleOperationStarted;
                SceneLoader.Instance.OnLoadProgress -= HandleLoadProgress;
                SceneLoader.Instance.OnOperationComplete -= HandleOperationComplete;
                SceneLoader.Instance.OnOperationCancelled -= HandleOperationCancelled;
            }
             if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(TriggerCancelLoad);
            }
        }

        /// <summary>
        /// This public method can be called by a "Load Scene" UI Button's OnClick event.
        /// </summary>
        public void TriggerSceneLoad()
        {
            Debug.Log($"Requesting to load scene: {targetScene}");
            // Use the new enum-based LoadScene method
            SceneLoader.Instance.LoadScene(targetScene);
        }

        /// <summary>
        /// This public method is called by our "Cancel" UI Button.
        /// </summary>
        public void TriggerCancelLoad()
        {
            Debug.Log("Requesting to cancel scene load operation...");
            SceneLoader.Instance.CancelOperation();
        }

        private void HandleOperationStarted()
        {
            Debug.Log("SceneLoader reported: Operation Started. Activating loading screen.");
            if (loadingScreenPanel != null) loadingScreenPanel.SetActive(true);
            if (progressBar != null) progressBar.value = 0;
            if (cancelButton != null) cancelButton.gameObject.SetActive(true);
        }

        private void HandleLoadProgress(float progress)
        {
            if (progressBar != null) progressBar.value = progress;
        }

        private void HandleOperationComplete()
        {
            Debug.Log("SceneLoader reported: Operation Complete. Deactivating loading screen.");
            // This object will be destroyed when the new scene loads,
            // but this event is useful if your loading UI is on a DontDestroyOnLoad object.
            if (loadingScreenPanel != null) loadingScreenPanel.SetActive(false);
            if (cancelButton != null) cancelButton.gameObject.SetActive(false);
        }
        
        private void HandleOperationCancelled()
        {
            Debug.Log("SceneLoader reported: Operation Cancelled. Deactivating loading screen.");
            if (loadingScreenPanel != null) loadingScreenPanel.SetActive(false);
            if (cancelButton != null) cancelButton.gameObject.SetActive(false);
        }
    }
}