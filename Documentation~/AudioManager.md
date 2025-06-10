# Audio Management System

The Audio Management system provides a robust, centralized solution for handling all sound in your game. It is built upon the **Addressables** system for efficient, asynchronous asset loading and memory management.

The manager handles Background Music (BGM) and Sound Effects (SFX) separately and supports two primary playback methods:
1.  **On-Demand Loading:** For non-critical sounds that can tolerate a tiny loading delay.
2.  **Preloading:** For critical, low-latency sounds (like gunshots or user feedback) that must play instantly.

## Core Class
`MAF.AudioManager.AudioManager`

## Setup

1.  **Install Addressables:** This module requires the "Addressables" package. Install it via the Unity Package Manager if you haven't already.
2.  **Mark Audio as Addressable:** In your Unity project, find your audio clips (`.mp3`, `.wav`, etc.). In the Inspector, check the "Addressable" box for each clip you want the `AudioManager` to use.
3.  **Use Addressable Keys:** All methods in the `AudioManager` identify audio clips by their **Addressable Key** (a string), which you can see and modify in the "Addressables Groups" window (`Window > Asset Management > Addressables > Groups`).

## Usage

The `AudioManager` is a `Singleton` and can be accessed globally via `AudioManager.Instance`.

### 1. Playing Background Music (BGM)

BGM is treated as a single, long-running track. Playing a new BGM will automatically stop and release the previous one.

```csharp
// The addressable key for your music clip
string musicAddress = "MainMenuTheme";

// Play the BGM. It will load asynchronously.
AudioManager.Instance.PlayBGM(musicAddress);

// You can also control playback
AudioManager.Instance.PauseBGM();
AudioManager.Instance.UnPauseBGM();
AudioManager.Instance.StopBGM(); // Stops and releases the clip from memory
```

### 2. Playing Sound Effects (SFX)

You have two ways to play SFX, depending on your performance needs.

#### A) On-Demand Playback (For Non-Critical Sounds)

This is the simplest method. Use it for sounds where a tiny, potential delay is acceptable (e.g., UI clicks, ambient effects). The manager handles loading and memory cleanup automatically.

```csharp
string clickSoundAddress = "UIClick";

// Play a 2D sound
AudioManager.Instance.PlaySFX(clickSoundAddress);

// Play a 3D sound at a specific location
AudioManager.Instance.PlaySFXAtPoint(clickSoundAddress, transform.position);
```

#### B) Preloading (For Critical, Lag-Free Sounds)

This is the recommended method for sounds that must be perfectly synchronized with gameplay (e.g., gunshots, impacts, jump sounds). It involves three steps:

**Step 1: Preload the Sounds**
During a loading screen, when a character spawns, or when a weapon is equipped, preload the necessary audio clips.

```csharp
void Start()
{
    // Preload sounds that need to be played instantly later
    AudioManager.Instance.PreloadAudioClip("PlayerGunshot");
    AudioManager.Instance.PreloadAudioClip("Footstep_Grass");
}
```

**Step 2: Play the Sounds**
Call `PlaySFX` as usual. The manager will detect that the clip is already in memory and play it instantly with no loading delay.

```csharp
void Shoot()
{
    // This will play INSTANTLY because it was preloaded.
    AudioManager.Instance.PlaySFX("PlayerGunshot");
}
```

**Step 3: Release the Sounds**
When the sounds are no longer needed (e.g., on level unload, or when the player's weapon is unequipped), release them to free up memory.

```csharp
void OnDestroy()
{
    // Clean up the preloaded assets
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.ReleasePreloadedAudioClip("PlayerGunshot");
        AudioManager.Instance.ReleasePreloadedAudioClip("Footstep_Grass");
    }
}
```

### 3. Volume Control

You can control BGM and SFX volume globally. The values should be between `0.0` and `1.0`.

```csharp
// Set BGM volume to 50%
AudioManager.Instance.BGMVolume = 0.5f;

// Set SFX volume to 100%
AudioManager.Instance.SFXVolume = 1.0f;
```

## Key `AudioManager` Methods

### BGM
- `PlayBGM(string clipAddress, bool loop = true)`: Asynchronously loads and plays the background music.
- `StopBGM()`: Stops the current BGM and releases its memory.
- `PauseBGM()`: Pauses the BGM playback.
- `UnPauseBGM()`: Resumes the paused BGM.

### SFX & Preloading
- `PreloadAudioClip(string clipAddress)`: Loads an audio clip into memory for later use.
- `ReleasePreloadedAudioClip(string clipAddress)`: Releases a preloaded clip from memory.
- `PlaySFX(string clipAddress)`: Plays a 2D sound. Plays instantly if preloaded, otherwise loads on-demand.
- `PlaySFXAtPoint(string clipAddress, Vector3 position)`: Plays a 3D sound at a world position.

### Volume
- `BGMVolume` (property): Get or set the volume for background music.
- `SFXVolume` (property): Get or set the volume for all sound effects.

## Important Notes
- **Main Thread:** All methods should be called from Unity's main thread.
- **Memory Management:** For on-demand SFX, the `AudioManager` automatically releases the audio clip's memory after it finishes playing. For preloaded clips, you are responsible for calling `ReleasePreloadedAudioClip` when you are done with them.
