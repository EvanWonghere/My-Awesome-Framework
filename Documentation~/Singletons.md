# Singletons

This framework provides base classes for creating Singletons, which are classes that are designed to have only one instance accessible globally.

## 1. `Singleton<T>` (for MonoBehaviours)

Use `MAF.Singleton.Singleton<T>` for creating `MonoBehaviour` classes that should act as singletons.

### Features
- Ensures only one instance of the `MonoBehaviour` exists.
- If no instance exists in the scene, one is automatically created with `DontDestroyOnLoad`.
- If multiple instances are accidentally created (e.g., by duplicating a GameObject), extras are destroyed.
- Handles application quitting gracefully.

### Usage
Create your `MonoBehaviour` class and inherit from `Singleton<T>`:
```csharp
using MAF.Singleton;

public class GameManager : Singleton<GameManager>
{
    // Optional: Override Awake if you need specific initialization logic
    // Remember to call base.Awake() if you do.
    protected override void Awake()
    {
        base.Awake(); // Ensures Singleton logic runs
        // Your GameManager specific Awake logic here
        Debug.Log("GameManager initialized!");
    }

    public void StartNewGame()
    {
        Debug.Log("Starting a new game...");
    }
}
```
Access the instance globally:
```csharp
GameManager.Instance.StartNewGame();
```

## 2. `LazySingleton<T>` (for non-MonoBehaviours)

Use `MAF.Singleton.LazySingleton<T>` for regular C# classes (not deriving from `MonoBehaviour`) that need to be singletons.

### Features
- **Lazy Initialization:** The instance is only created when it's first accessed.
- **Thread-Safe:** Uses `System.Lazy<T>` for thread-safe instance creation.

### Usage
Create your class and inherit from `LazySingleton<T>`. The class `T` must have a parameterless constructor.
```csharp
using MAF.Singleton;

public class GameSettings : LazySingleton<GameSettings>
{
    public float MasterVolume { get; set; }

    // Constructor can be public, protected, or private.
    // If private, it further enforces the singleton pattern.
    // LazySingleton<T> requires `new()` constraint, so it must be accessible.
    public GameSettings() 
    {
        MasterVolume = 0.75f; // Default value
    }

    public void LoadSettings() { /* ... */ }
}
```
Access the instance globally:
```csharp
float volume = GameSettings.Instance.MasterVolume;
GameSettings.Instance.LoadSettings();
```
