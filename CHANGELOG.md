# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- (List new features or improvements currently in development for the next release)

### Changed
- (List changes in existing functionality)

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