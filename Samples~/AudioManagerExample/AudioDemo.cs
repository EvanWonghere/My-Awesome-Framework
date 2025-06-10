using UnityEngine;
using UnityEngine.UI; // For UI Sliders if you use them
using MAF.AudioManager;
using System.Collections.Generic;

namespace MAF.Samples
{
    /// <summary>
    /// Demonstrates the usage of the AudioManager, including preloading and on-demand playback.
    /// </summary>
    public class AudioDemo : MonoBehaviour
    {
        // --- IMPORTANT ---
        // These strings must match the ADDRESSABLE KEYS of your audio clips.
        [Header("Addressable Keys")]
        [SerializeField] private string bgmAddress = "YourBackgroundMusic";
        [SerializeField] private string jumpSfxAddress = "YourJumpSound";
        [SerializeField] private string coinSfxAddress = "YourCoinSound";
        [SerializeField] private string criticalSfxAddress = "YourCriticalSound"; // A sound to be preloaded

        [Header("UI (Optional)")]
        public Slider bgmVolumeSlider;
        public Slider sfxVolumeSlider;

        void Start()
        {
            // --- 1. Preload critical sounds needed for instant feedback ---
            Debug.Log("Preloading critical audio...");
            AudioManager.Instance.PreloadAudioClip(criticalSfxAddress);

            // --- 2. Initialize UI ---
            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.value = AudioManager.Instance.BGMVolume;
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.SFXVolume;
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            // --- 3. Start playing background music ---
            Debug.Log($"Playing BGM from Address: {bgmAddress}");
            AudioManager.Instance.PlayBGM(bgmAddress);
        }

        void Update()
        {
            // --- 4. Play sounds based on input ---
            
            // Play a non-critical sound (loaded on-demand)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log($"Playing JUMP SFX (on-demand) from Address: {jumpSfxAddress}");
                AudioManager.Instance.PlaySFX(jumpSfxAddress);
            }

            // Play a preloaded critical sound for instant feedback
            if (Input.GetKeyDown(KeyCode.Return)) // Enter key
            {
                Debug.Log($"Playing CRITICAL SFX (preloaded) from Address: {criticalSfxAddress}");
                AudioManager.Instance.PlaySFX(criticalSfxAddress);
            }

            // Play a 3D sound at the object's position
            if (Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log($"Playing 3D COIN SFX from Address: {coinSfxAddress} at player position.");
                AudioManager.Instance.PlaySFXAtPoint(coinSfxAddress, transform.position);
            }
        }

        public void OnBGMVolumeChanged(float value)
        {
            AudioManager.Instance.BGMVolume = value;
        }

        public void OnSFXVolumeChanged(float value)
        {
            AudioManager.Instance.SFXVolume = value;
        }

        void OnDestroy()
        {
            // --- 5. Clean up preloaded sounds when this object is destroyed ---
            // In a real game, this would happen during a level unload or scene change.
            Debug.Log("Releasing preloaded audio...");
            if(AudioManager.Instance != null) // Check if instance still exists
            {
                AudioManager.Instance.ReleasePreloadedAudioClip(criticalSfxAddress);
            }

            // Unsubscribe from UI events
             if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
             if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        }
    }
}