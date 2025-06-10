using UnityEngine;
using UnityEngine.UI;
using MAF.SceneManagement;

namespace MAF.Samples
{
    public class SceneLoaderDemo : MonoBehaviour
    {
        [Header("Scene To Load")]
        [Tooltip("Select the scene you want to load from the dropdown.")]
        [SerializeField] private Scenes targetScene; // Using the enum now!

        [Header("UI Elements")]
        [SerializeField] private GameObject loadingScreenPanel;
        [SerializeField] private Slider progressBar;

        void Awake()
        {
            SceneLoader.Instance.OnLoadStarted += HandleLoadStarted;
            SceneLoader.Instance.OnLoadProgress += HandleLoadProgress;
            SceneLoader.Instance.OnLoadComplete += HandleLoadComplete;
        }

        void OnDestroy()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.OnLoadStarted -= HandleLoadStarted;
                SceneLoader.Instance.OnLoadProgress -= HandleLoadProgress;
                SceneLoader.Instance.OnLoadComplete -= HandleLoadComplete;
            }
        }

        /// <summary>
        /// This public method can be called by a UI Button's OnClick event.
        /// </summary>
        public void TriggerSceneLoad()
        {
            Debug.Log($"Requesting to load scene: {targetScene}");
            // Use the new enum-based LoadScene method
            SceneLoader.Instance.LoadScene(targetScene);
        }

        private void HandleLoadStarted()
        {
            if (loadingScreenPanel != null) loadingScreenPanel.SetActive(true);
            if (progressBar != null) progressBar.value = 0;
        }

        private void HandleLoadProgress(float progress)
        {
            if (progressBar != null) progressBar.value = progress;
        }

        private void HandleLoadComplete()
        {
            if (loadingScreenPanel != null) loadingScreenPanel.SetActive(false);
        }
    }
}