# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- (List new features or improvements currently in development for the next release)
- **Audio Management Module:**
  - Added `AudioManager` singleton for handling BGM and SFX.
  - Integrated with the Addressables system for asynchronous, on-demand audio loading.
  - Implemented a preloading system (`PreloadAudioClip` and `ReleasePreloadedAudioClip`) for critical, low-latency sounds.
  - Added separate global volume controls for BGM and SFX.
- **Scene Management Module:**
  - Added `SceneLoader` singleton for robust asynchronous scene loading.
  - Implemented progress, completion, cancellation, and unload events to manage loading screens and game state.
  - Added `UnloadSceneThenLoadScene` for advanced scene management sequences.
  - Created a `SceneEnumGenerator` editor tool (`Tools/MAF/Generate Scene Enum`) for type-safe scene selection in code and the Inspector.
- Added new Samples, Tests, and Documentation for the Audio and Scene Management modules.

### Changed
- (List changes in existing functionality)
- Refactored `SceneLoader` to be more robust with pre-load validation, cancellation support, and better lifecycle management.

### Deprecated
- (List features soon to be removed)

### Removed
- (List features removed in this cycle)

### Fixed
- (List any bug fixes)

### Security
- (List vulnerabilities patched)

## [0.1.0] - 2025-06-03
### Added
- **Event System:** Initial implementation of `EventCenter` for global event management.
- **Finite State Machine (FSM):** Core `FSM` class and `IState` interface for state management.
- **Object Pooling:** `ObjectPoolManager` for managing reusable GameObjects, `ObjectPoolInstance` for individual pool logic, and `PooledObjectInfo` for tracking pooled objects.
- **Singletons:**
    - `Singleton<T>`: Generic MonoBehaviour Singleton base class.
    - `LazySingleton<T>`: Generic non-MonoBehaviour lazy-initialized Singleton base class.
- **Initial Tests:** Basic unit and integration tests for core modules.
- **Initial Samples:** Example scenes and scripts for EventSystem, FSM, and ObjectPooling.
- **Initial Documentation:** Markdown documentation for all core modules.
- **Project Setup:** Configured as a UPM package with `package.json` and assembly definitions.