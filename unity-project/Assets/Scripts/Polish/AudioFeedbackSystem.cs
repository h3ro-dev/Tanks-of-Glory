using UnityEngine;

namespace TankCommander.Polish
{
    /// <summary>
    /// Provides audio feedback hooks and procedural sound cues
    /// Works with or without actual audio files - provides visual feedback as fallback
    /// </summary>
    public class AudioFeedbackSystem : MonoBehaviour
    {
        public static AudioFeedbackSystem Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource uiSoundSource;
        [SerializeField] private AudioSource impactSoundSource;
        [SerializeField] private AudioSource ambientSource;

        [Header("Audio Clips (Optional)")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip weaponFireSound;
        [SerializeField] private AudioClip impactSound;
        [SerializeField] private AudioClip explosionSound;
        [SerializeField] private AudioClip lowHealthSound;
        [SerializeField] private AudioClip reloadSound;

        [Header("Fallback Settings")]
        [SerializeField] private bool useVisualFeedback = true;
        [SerializeField] private bool logAudioEvents = false;

        [Header("Pitch Variation")]
        [SerializeField] private float pitchVariation = 0.1f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                SetupAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SetupAudioSources()
        {
            // Create audio sources if not assigned
            if (uiSoundSource == null)
            {
                GameObject uiObj = new GameObject("UI_AudioSource");
                uiObj.transform.SetParent(transform);
                uiSoundSource = uiObj.AddComponent<AudioSource>();
                uiSoundSource.playOnAwake = false;
                uiSoundSource.spatialBlend = 0f; // 2D sound
            }

            if (impactSoundSource == null)
            {
                GameObject impactObj = new GameObject("Impact_AudioSource");
                impactObj.transform.SetParent(transform);
                impactSoundSource = impactObj.AddComponent<AudioSource>();
                impactSoundSource.playOnAwake = false;
                impactSoundSource.spatialBlend = 0f;
            }

            if (ambientSource == null)
            {
                GameObject ambientObj = new GameObject("Ambient_AudioSource");
                ambientObj.transform.SetParent(transform);
                ambientSource = ambientObj.AddComponent<AudioSource>();
                ambientSource.playOnAwake = false;
                ambientSource.loop = true;
                ambientSource.spatialBlend = 0f;
                ambientSource.volume = 0.3f;
            }
        }

        // UI Sounds
        public void PlayButtonClick()
        {
            PlaySound(uiSoundSource, buttonClickSound, "Button Click", 0.5f, 1.0f);
        }

        public void PlayMenuOpen()
        {
            PlaySound(uiSoundSource, null, "Menu Open", 0.4f, 1.2f);
        }

        public void PlayMenuClose()
        {
            PlaySound(uiSoundSource, null, "Menu Close", 0.4f, 0.8f);
        }

        // Weapon Sounds
        public void PlayWeaponFire()
        {
            PlaySound(impactSoundSource, weaponFireSound, "Weapon Fire", 0.6f, 1.0f);

            if (GameFeelEnhancer.Instance != null)
            {
                GameFeelEnhancer.Instance.OnFire();
            }
        }

        public void PlayReload()
        {
            PlaySound(impactSoundSource, reloadSound, "Reload", 0.5f, 1.0f);
        }

        public void PlayWeaponEmpty()
        {
            PlaySound(impactSoundSource, null, "Weapon Empty", 0.3f, 0.8f);
        }

        // Impact Sounds
        public void PlayImpact(Vector3 position, float intensity = 1f)
        {
            PlaySoundAtPosition(impactSound, position, "Impact", intensity * 0.7f, 1.0f);
        }

        public void PlayExplosion(Vector3 position, float intensity = 1f)
        {
            PlaySoundAtPosition(explosionSound, position, "Explosion", intensity, 1.0f);

            // Screen shake
            if (CameraShakeManager.Instance != null)
            {
                CameraShakeManager.Instance.ShakeExplosionAtDistance(position);
            }
        }

        // Damage Sounds
        public void PlayHitConfirm()
        {
            PlaySound(impactSoundSource, null, "Hit Confirm", 0.4f, 1.2f);

            if (HitMarker.Instance != null)
            {
                HitMarker.Instance.ShowNormalHit();
            }
        }

        public void PlayCriticalHit()
        {
            PlaySound(impactSoundSource, null, "Critical Hit", 0.6f, 1.4f);

            if (HitMarker.Instance != null)
            {
                HitMarker.Instance.ShowCriticalHit();
            }
        }

        public void PlayKillConfirm()
        {
            PlaySound(impactSoundSource, null, "Kill Confirm", 0.7f, 1.5f);

            if (HitMarker.Instance != null)
            {
                HitMarker.Instance.ShowKillHit();
            }
        }

        public void PlayTakeDamage(float damageAmount)
        {
            float pitch = Mathf.Lerp(0.9f, 1.1f, damageAmount / 100f);
            PlaySound(impactSoundSource, null, "Take Damage", 0.5f, pitch);

            if (CameraShakeManager.Instance != null)
            {
                CameraShakeManager.Instance.ShakeDamage();
            }
        }

        // Status Sounds
        public void PlayLowHealthWarning()
        {
            if (!ambientSource.isPlaying)
            {
                PlaySound(ambientSource, lowHealthSound, "Low Health Warning", 0.3f, 1.0f);
            }
        }

        public void StopLowHealthWarning()
        {
            if (ambientSource.isPlaying)
            {
                ambientSource.Stop();
            }
        }

        // Game Events
        public void PlayVictory()
        {
            PlaySound(uiSoundSource, null, "Victory", 0.8f, 1.2f);
        }

        public void PlayDefeat()
        {
            PlaySound(uiSoundSource, null, "Defeat", 0.6f, 0.8f);
        }

        public void PlayCountdown(int number)
        {
            float pitch = 1.0f + (number * 0.1f);
            PlaySound(uiSoundSource, null, $"Countdown {number}", 0.7f, pitch);
        }

        // Core sound playing method
        private void PlaySound(AudioSource source, AudioClip clip, string eventName, float volume, float pitch)
        {
            if (source == null) return;

            // Apply pitch variation
            float finalPitch = pitch + Random.Range(-pitchVariation, pitchVariation);
            source.pitch = finalPitch;

            if (clip != null)
            {
                // Play actual audio clip
                source.PlayOneShot(clip, volume);
            }
            else
            {
                // Log event if no clip
                if (logAudioEvents)
                {
                    Debug.Log($"[Audio] {eventName} (volume: {volume}, pitch: {finalPitch})");
                }

                // Provide visual feedback as fallback
                if (useVisualFeedback)
                {
                    ProvideVisualFeedback(eventName);
                }
            }
        }

        private void PlaySoundAtPosition(AudioClip clip, Vector3 position, string eventName, float volume, float pitch)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, volume);
            }
            else
            {
                if (logAudioEvents)
                {
                    Debug.Log($"[Audio 3D] {eventName} at {position}");
                }

                if (useVisualFeedback)
                {
                    ProvideVisualFeedback(eventName);
                }
            }
        }

        private void ProvideVisualFeedback(string eventName)
        {
            // Visual feedback when audio isn't available
            // Could flash screen, show icon, etc.
            // For now, we just ensure other visual systems are triggered
        }

        // Helper methods for integration
        public bool HasAudioClip(string clipName)
        {
            switch (clipName)
            {
                case "fire": return weaponFireSound != null;
                case "impact": return impactSound != null;
                case "explosion": return explosionSound != null;
                case "reload": return reloadSound != null;
                default: return false;
            }
        }

        public void SetMasterVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
        }

        [ContextMenu("Test All Audio Events")]
        public void TestAllAudioEvents()
        {
            Debug.Log("Testing all audio events...");

            Invoke(nameof(PlayButtonClick), 0.5f);
            Invoke(nameof(PlayWeaponFire), 1.0f);
            Invoke(nameof(PlayHitConfirm), 1.5f);
            Invoke(nameof(PlayReload), 2.0f);

            Debug.Log("Audio test sequence started");
        }
    }
}
