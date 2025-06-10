# Scene Management System

The Scene Management system provides a robust, centralized solution for loading and unloading scenes asynchronously. It is designed to prevent your game from freezing during transitions and includes advanced features like progress reporting, cancellation, and type-safe scene selection to create a smooth user experience and a safe development workflow.

## Core Class
`MAF.SceneManagement.SceneLoader`

## Setup

There are two crucial steps to set up this module:

### 1. Add Scenes to Build Settings

For the `SceneLoader` to find and load your scenes, they **must** be added to Unity's Build Settings.

1.  Go to **File > Build Settings...**.
2.  Drag and drop all scenes you intend to load from your Project window into the "Scenes In Build" list. The order of scenes here determines their build index.

### 2. Generate the `Scenes` Enum

To prevent errors from typos in scene names and to get a convenient dropdown in the Inspector, you should generate a `Scenes` enum.

1.  After updating your scenes in the Build Settings, go to the new menu item **Tools > MAF > Generate Scene Enum**.
2.  This will create or overwrite the `ScenesEnum.cs` file in your framework, containing all scenes from your build list.
3.  Run this generator whenever you add, remove, or reorder scenes in the Build Settings.

## Usage

The `SceneLoader` is a `Singleton` accessible globally via `SceneLoader.Instance`.

### 1. Loading a Scene

You can load a scene using the generated `Scenes` enum (recommended), its string name, or its integer build index. The `LoadScene` methods will unload all currently open scenes.

```csharp
// Method 1: Using the Scenes enum (Recommended, type-safe)
SceneLoader.Instance.LoadScene(Scenes.Level_01);

// Method 2: Using the scene name string
SceneLoader.Instance.LoadSceneByName("Level_01");

// Method 3: Using the scene's build index
SceneLoader.Instance.LoadSceneByIndex(1);
```

### 2. Creating a Loading Screen

The `SceneLoader` provides a full suite of events to manage a loading screen UI.

* `OnOperationStarted`: Fired when the load begins. Use this to show your loading UI.
* `OnLoadProgress`: Fired every frame during loading. The `float` parameter is the progress from 0.0 to 1.0. Use this to update a progress bar.
* `OnOperationComplete`: Fired after the new scene is fully loaded and activated. Use this to hide your loading UI.
* `OnOperationCancelled`: Fired if the load is cancelled via `CancelOperation()`. Use this to hide the loading UI.

**Example UI Controller:**
```csharp
public class LoadingScreenController : MonoBehaviour
{
    public GameObject loadingPanel;
    public Slider progressBar;

    void Awake()
    {
        // Subscribe to events
        SceneLoader.Instance.OnOperationStarted += OnLoadStarted;
        SceneLoader.Instance.OnLoadProgress += OnProgressUpdated;
        SceneLoader.Instance.OnOperationComplete += OnLoadFinished;
        SceneLoader.Instance.OnOperationCancelled += OnLoadFinished; // Can often use the same cleanup logic
    }

    void OnDestroy()
    {
        // IMPORTANT: Always unsubscribe
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnOperationStarted -= OnLoadStarted;
            SceneLoader.Instance.OnLoadProgress -= OnProgressUpdated;
            SceneLoader.Instance.OnOperationComplete -= OnLoadFinished;
            SceneLoader.Instance.OnOperationCancelled -= OnLoadFinished;
        }
    }

    void OnLoadStarted() => loadingPanel.SetActive(true);
    void OnProgressUpdated(float progress) => progressBar.value = progress;
    void OnLoadFinished() => loadingPanel.SetActive(false);
}
```

### 3. Cancelling a Scene Load

You can abort a scene transition while it's in progress. This is useful for letting the user back out of a loading screen.

```csharp
// This could be called from a "Cancel" button on your loading screen UI
public void CancelLoad()
{
    SceneLoader.Instance.CancelOperation();
}
```

### 4. Additive Loading and Unloading

For more complex scenarios, like managing multiple scenes at once (e.g., a persistent UI scene plus a swappable level scene), you can use the unload methods.

* `UnloadScene(string sceneName)`: Unloads a single additively-loaded scene.
* `UnloadSceneThenLoadScene(string sceneToUnload, string sceneToLoad)`: A convenient sequence for swapping scenes, like going from Level 1 to Level 2.

```csharp
// Example: Swapping from Level_01 to Level_02
SceneLoader.Instance.UnloadSceneThenLoadScene("Level_01", "Level_02");

// The unload-specific events OnUnloadStarted and OnUnloadComplete will be fired.
```

## Key `SceneLoader` Members

### Properties
- `IsBusy` (`bool`): Returns `true` if any scene operation (load or unload) is currently in progress.

### Methods
- `LoadScene(Scenes scene)`: Loads a scene using the enum.
- `LoadSceneByName(string sceneName)`: Loads a scene by its name.
- `LoadSceneByIndex(int sceneBuildIndex)`: Loads a scene by its build index.
- `UnloadScene(string sceneName)`: Unloads an additive scene.
- `UnloadSceneThenLoadScene(string sceneToUnload, string sceneToLoad)`: Performs an unload-then-load sequence.
- `CancelOperation()`: Aborts the current operation.

### Events
- `OnOperationStarted`: `Action`
- `OnLoadProgress`: `Action<float>`
- `OnOperationComplete`: `Action`
- `OnUnloadStarted`: `Action<string>`
- `OnUnloadComplete`: `Action<string>`
- `OnOperationCancelled`: `Action`
