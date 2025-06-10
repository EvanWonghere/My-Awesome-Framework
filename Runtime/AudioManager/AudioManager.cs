using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MAF.Singleton;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MAF.AudioManager
{
    /// <summary>
    /// Manages all audio playback using the Addressables system. 
    /// Supports on-demand loading for general sounds and preloading for critical, low-latency sounds.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        // Audio sources for BGM and SFX
        private AudioSource _bgmSource;
        private readonly List<AudioSource> _sfxSources = new List<AudioSource>();
        private int _sfxSourceIndex = 0;
        private const int INITIAL_SFX_SOURCES = 5; // Initial number of AudioSources for SFX

        // Handle to the currently loaded BGM clip to manage its memory
        private AsyncOperationHandle<AudioClip> _bgmLoadHandle;

        // Dictionary to hold preloaded clips for instant playback
        private readonly Dictionary<string, AsyncOperationHandle<AudioClip>> _preloadedClips = new Dictionary<string, AsyncOperationHandle<AudioClip>>();

        // Volume properties
        private float _bgmVolume = 1.0f;
        private float _sfxVolume = 1.0f;

        public float BGMVolume
        {
            get => _bgmVolume;
            set
            {
                _bgmVolume = Mathf.Clamp01(value);
                if (_bgmSource != null)
                {
                    _bgmSource.volume = _bgmVolume;
                }
            }
        }

        public float SFXVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                foreach (var source in _sfxSources)
                {
                    source.volume = _sfxVolume;
                }
            }
        }

        /// <summary>
        /// Initializes the AudioManager, creates AudioSource components, and sets up for playback.
        /// </summary>
        protected override void Awake()
        {
            base.Awake(); // Sets up the singleton instance and DontDestroyOnLoad
            if (Instance != this) return;

            // Create BGM AudioSource
            var bgmObject = new GameObject("BGM_Source");
            bgmObject.transform.SetParent(this.transform);
            _bgmSource = bgmObject.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            _bgmSource.loop = true;

            // Create a pool of SFX AudioSources
            for (int i = 0; i < INITIAL_SFX_SOURCES; i++)
            {
                CreateSfxSource();
            }
        }
        
        /// <summary>
        /// Creates a new AudioSource for sound effects and adds it to the pool.
        /// </summary>
        private void CreateSfxSource()
        {
            var sfxObject = new GameObject($"SFX_Source_{_sfxSources.Count}");
            sfxObject.transform.SetParent(this.transform);
            var newSource = sfxObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            newSource.loop = false;
            newSource.volume = _sfxVolume;
            _sfxSources.Add(newSource);
        }

        #region Background Music (BGM)
        /// <summary>
        /// Asynchronously loads and plays a background music track.
        /// If another BGM is already playing, it will be stopped and its resources released.
        /// </summary>
        /// <param name="clipAddress">The Addressable key for the BGM clip.</param>
        /// <param name="loop">Whether the music should loop. Defaults to true.</param>
        public async void PlayBGM(string clipAddress, bool loop = true)
        {
            if (string.IsNullOrEmpty(clipAddress))
            {
                Debug.LogError("[AudioManager] BGM address cannot be null or empty.");
                return;
            }

            // If a BGM track is already loaded, release it before loading a new one.
            if (_bgmLoadHandle.IsValid())
            {
                Addressables.Release(_bgmLoadHandle);
            }

            // Load the new clip
            _bgmLoadHandle = Addressables.LoadAssetAsync<AudioClip>(clipAddress);
            await _bgmLoadHandle.Task;

            if (_bgmLoadHandle.Status == AsyncOperationStatus.Succeeded && _bgmSource != null)
            {
                _bgmSource.clip = _bgmLoadHandle.Result;
                _bgmSource.loop = loop;
                _bgmSource.volume = _bgmVolume;
                _bgmSource.Play();
            }
            else
            {
                Debug.LogError($"[AudioManager] Failed to load BGM from address: {clipAddress}");
            }
        }

        /// <summary>
        /// Stops the currently playing background music and releases its Addressable handle.
        /// </summary>
        public void StopBGM()
        {
            if (_bgmSource != null && _bgmSource.isPlaying)
            {
                _bgmSource.Stop();
            }
            if (_bgmLoadHandle.IsValid())
            {
                Addressables.Release(_bgmLoadHandle);
            }
        }
        
        /// <summary>
        /// Pauses the currently playing background music.
        /// </summary>
        public void PauseBGM()
        {
            if (_bgmSource != null) _bgmSource.Pause();
        }

        /// <summary>
        /// Resumes the paused background music.
        /// </summary>
        public void UnPauseBGM()
        {
            if (_bgmSource != null) _bgmSource.UnPause();
        }
        #endregion

        #region Sound Effects (SFX)
        /// <summary>
        /// Asynchronously preloads an audio clip into memory for instant playback later.
        /// Use this during loading screens for latency-sensitive sounds like gunshots or impacts.
        /// </summary>
        /// <param name="clipAddress">The Addressable key of the clip to preload.</param>
        public async void PreloadAudioClip(string clipAddress)
        {
            if (string.IsNullOrEmpty(clipAddress) || _preloadedClips.ContainsKey(clipAddress))
            {
                return;
            }

            var handle = Addressables.LoadAssetAsync<AudioClip>(clipAddress);
            _preloadedClips[clipAddress] = handle; // Store handle immediately to prevent duplicate loads
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[AudioManager] Failed to preload audio clip: {clipAddress}");
                _preloadedClips.Remove(clipAddress); // Remove failed load from dictionary
            }
            else
            {
                 Debug.Log($"[AudioManager] Successfully preloaded audio clip: {clipAddress}");
            }
        }

        /// <summary>
        /// Releases a preloaded audio clip from memory. Call this when the sounds are no longer needed (e.g., on level unload).
        /// </summary>
        /// <param name="clipAddress">The Addressable key of the clip to release.</param>
        public void ReleasePreloadedAudioClip(string clipAddress)
        {
            if (string.IsNullOrEmpty(clipAddress) || !_preloadedClips.TryGetValue(clipAddress, out var handle))
            {
                return;
            }
            Addressables.Release(handle);
            _preloadedClips.Remove(clipAddress);
        }

        /// <summary>
        /// Plays a sound effect. If the clip is preloaded, it plays instantly with no delay.
        /// Otherwise, it will be loaded on-demand, which may introduce a small latency.
        /// </summary>
        /// <param name="clipAddress">The Addressable key for the SFX clip.</param>
        public void PlaySFX(string clipAddress)
        {
            if (string.IsNullOrEmpty(clipAddress)) return;

            // Check if the clip is already preloaded and finished loading
            if (_preloadedClips.TryGetValue(clipAddress, out var handle) && handle.IsDone)
            {
                // Play the preloaded clip instantly
                _sfxSourceIndex = (_sfxSourceIndex + 1) % _sfxSources.Count;
                _sfxSources[_sfxSourceIndex].PlayOneShot(handle.Result, _sfxVolume);
            }
            else
            {
                // If not preloaded, load it on-demand and release after playing.
                StartCoroutine(PlaySFXAndRelease(clipAddress));
            }
        }
        
        /// <summary>
        /// Plays a 3D sound effect at a specific position. Checks for preloaded clips first for instant playback.
        /// </summary>
        /// <param name="clipAddress">The Addressable key for the SFX clip.</param>
        /// <param name="position">The world position to play the sound at.</param>
        public void PlaySFXAtPoint(string clipAddress, Vector3 position)
        {
            if (string.IsNullOrEmpty(clipAddress)) return;

            if (_preloadedClips.TryGetValue(clipAddress, out var handle) && handle.IsDone)
            {
                 // Play preloaded clip instantly.
                 AudioSource.PlayClipAtPoint(handle.Result, position, _sfxVolume);
            }
            else
            {
                // Fallback to on-demand loading for non-preloaded 3D sounds.
                StartCoroutine(PlaySFXAtPointAndRelease(clipAddress, position));
            }
        }

        // Coroutine for on-demand 2D SFX playback and cleanup
        private IEnumerator PlaySFXAndRelease(string clipAddress)
        {
            var handle = Addressables.LoadAssetAsync<AudioClip>(clipAddress);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                AudioClip clip = handle.Result;
                _sfxSourceIndex = (_sfxSourceIndex + 1) % _sfxSources.Count;
                _sfxSources[_sfxSourceIndex].PlayOneShot(clip, _sfxVolume);

                // Wait for the clip to finish playing before releasing the handle
                yield return new WaitForSeconds(clip.length);
                Addressables.Release(handle);
            } 
            else 
            { 
                Debug.LogError($"[AudioManager] Failed to load SFX from address: {clipAddress}");
            }
        }

        // Coroutine for on-demand 3D SFX playback and cleanup
        private IEnumerator PlaySFXAtPointAndRelease(string clipAddress, Vector3 position)
        {
            var handle = Addressables.LoadAssetAsync<AudioClip>(clipAddress);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                AudioClip clip = handle.Result;
                AudioSource.PlayClipAtPoint(clip, position, _sfxVolume);

                // Wait for the clip to finish playing before releasing the handle
                yield return new WaitForSeconds(clip.length);
                Addressables.Release(handle);
            } 
            else 
            {
                Debug.LogError($"[AudioManager] Failed to load 3D SFX from address: {clipAddress}");
            }
        }
        #endregion

        /// <summary>
        /// Handles cleanup when the AudioManager singleton is destroyed.
        /// Releases any remaining Addressable handles.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Release all remaining handles to prevent memory leaks
            if (_bgmLoadHandle.IsValid()) Addressables.Release(_bgmLoadHandle);
            foreach(var handle in _preloadedClips.Values)
            {
                if(handle.IsValid()) Addressables.Release(handle);
            }
            _preloadedClips.Clear();
        }
    }
}