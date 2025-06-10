# MAF Framework (My Awesome Framework)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE)
A collection of reusable scripts and systems designed to accelerate game development in Unity. MAF provides common utilities and architectural patterns to help you build more organized and efficient projects.

## Features

MAF Framework currently includes the following modules:

* **Event System (`EventCenter`):** A centralized, type-safe event management system for decoupled communication between different parts of your game.
* **Finite State Machine (FSM):** A robust FSM implementation to manage complex behaviors for characters, UI, or other game entities.
* **Object Pooling (`ObjectPoolManager`):** An efficient system for reusing GameObjects, reducing instantiation overhead and improving performance.
* **Singletons:**
    * `Singleton<T>`: A generic base class for creating MonoBehaviour singletons that persist across scenes.
    * `LazySingleton<T>`: A generic base class for creating thread-safe, lazy-initialized singletons for non-MonoBehaviour classes.
* **Audio Management (`AudioManager`):** A robust manager for BGM and SFX using the Addressables system. Supports on-demand loading for general sounds and preloading for critical sounds.
* **Scene Management (`SceneLoader`):** An asynchronous scene loader with progress reporting, cancellation support, and a type-safe `Scenes` enum generator for robust scene transitions.

## Requirements

* Unity `2021.3.x` (LTS) or higher is recommended. Please check the `package.json` for the specific minimum version.
* **Addressables** package is required for the `AudioManager`.

## Installation

This framework is distributed as a Unity Package Manager (UPM) package. To install it in your project:

1.  Open your Unity project.
2.  Go to **Window > Package Manager**.
3.  Click the **"+"** button in the top-left corner of the Package Manager window.
4.  Select **"Add package from git URL..."**
5.  Enter the following URL:
    ```
    https://github.com/EvanWonghere/My-Awesome-Framework.git
    ```
6.  You can also specify a particular version or branch by appending `#` followed by the tag/branch name (e.g., `#v1.0.0` or `#main`).
7.  Click **Add**.

Unity will download and install the package into your project.

## How to Use

Each module is designed to be intuitive and easy to integrate.

* **Event System:** Access via `EventCenter.Instance.Register<MyEvent>(handler);` and `EventCenter.Instance.Trigger(new MyEvent());`.
* **FSM:** Create an instance of `FSM`, add your `IState` implementations, set an initial state, and call `ExecuteState()` in your `Update` loop.
* **Object Pooling:** Use `ObjectPoolManager.Instance.CreatePool(prefab);` to set up, `ObjectPoolManager.Instance.GetObject(prefab);` to retrieve, and `ObjectPoolManager.Instance.ReturnObject(instance);` to return objects.
* **Singletons:** Inherit your MonoBehaviours from `Singleton<MyClass>` or your regular C# classes from `LazySingleton<MyOtherClass>` and access them via `MyClass.Instance`.
* **Audio Manager:** Use `AudioManager.Instance.PlayBGM("YourMusicAddress");` and `AudioManager.Instance.PlaySFX("YourSFXAddress");`. Preload critical sounds with `AudioManager.Instance.PreloadAudioClip("CriticalSoundAddress");`.
* **Scene Loader:** Generate your scenes enum via `Tools/MAF/Generate Scene Enum`, then load scenes with `SceneLoader.Instance.LoadScene(Scenes.MyLevel);`.

### Samples & Detailed Documentation

* **Samples:** Example scenes and scripts demonstrating how to use each module are available in the `Samples~` folder of this package. You can import them into your project via the Package Manager window (select "MAF Framework" under "In Project", then find the "Samples" dropdown).
* **Detailed Documentation:** For more in-depth information on each module's API and usage, please refer to the documentation files located in the `Documentation~` folder of this package or [view them on GitHub here](./Documentation~/Index.md).

## Contributing

Currently, this project is maintained by EvanWong. If you'd like to contribute, please feel free to fork the repository, make your changes, and submit a pull request. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the **Apache License Version 2.0**. See the [LICENSE](LICENSE) file for details.
