using UnityEngine;
using System.Collections.Generic;

namespace TankCommander
{
    /// <summary>
    /// Manages all audio in the game including music, sound effects, and voice
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [System.Serializable]
        public class Sound
        {
            public string name;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
            [Range(0.1f, 3f)] public float pitch = 1f;
            public bool loop = false;
            [HideInInspector] public AudioSource source;
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceSource;

        [Header("Music Tracks")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip victoryMusic;
        [SerializeField] private AudioClip defeatMusic;

        [Header("Sound Effects")]
        [SerializeField] private List<Sound> soundEffects = new List<Sound>();

        [Header("Settings")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float voiceVolume = 1f;

        private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();
        private List<AudioSource> pooledSources = new List<AudioSource>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAudio()
        {
            // Create audio sources if they don't exist
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (voiceSource == null)
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
                voiceSource.playOnAwake = false;
            }

            // Build sound dictionary
            foreach (var sound in soundEffects)
            {
                if (!soundDictionary.ContainsKey(sound.name))
                {
                    soundDictionary.Add(sound.name, sound);
                }
            }

            // Create pooled audio sources for 3D sounds
            for (int i = 0; i < 10; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 1f; // 3D sound
                pooledSources.Add(source);
            }

            UpdateVolumes();
        }

        #region Music Control

        public void PlayMenuMusic()
        {
            PlayMusic(menuMusic);
        }

        public void PlayGameplayMusic()
        {
            PlayMusic(gameplayMusic);
        }

        public void PlayVictoryMusic()
        {
            PlayMusic(victoryMusic);
        }

        public void PlayDefeatMusic()
        {
            PlayMusic(defeatMusic);
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource != null && clip != null)
            {
                if (musicSource.clip == clip && musicSource.isPlaying)
                    return;

                musicSource.clip = clip;
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        public void PauseMusic()
        {
            if (musicSource != null)
            {
                musicSource.Pause();
            }
        }

        public void ResumeMusic()
        {
            if (musicSource != null)
            {
                musicSource.UnPause();
            }
        }

        #endregion

        #region Sound Effects

        public void PlaySound(string soundName)
        {
            if (soundDictionary.TryGetValue(soundName, out Sound sound))
            {
                PlaySound(sound.clip, sound.volume, sound.pitch);
            }
            else
            {
                Debug.LogWarning($"Sound '{soundName}' not found!");
            }
        }

        public void PlaySound(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.pitch = pitch;
                sfxSource.PlayOneShot(clip, volume * sfxVolume * masterVolume);
            }
        }

        public void PlaySoundAtPosition(string soundName, Vector3 position)
        {
            if (soundDictionary.TryGetValue(soundName, out Sound sound))
            {
                PlaySoundAtPosition(sound.clip, position, sound.volume, sound.pitch);
            }
        }

        public void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetAvailableSource();
            if (source != null)
            {
                source.transform.position = position;
                source.clip = clip;
                source.volume = volume * sfxVolume * masterVolume;
                source.pitch = pitch;
                source.Play();
            }
            else
            {
                // Fallback to static 2D sound if no pooled source available
                AudioSource.PlayClipAtPoint(clip, position, volume * sfxVolume * masterVolume);
            }
        }

        private AudioSource GetAvailableSource()
        {
            foreach (var source in pooledSources)
            {
                if (!source.isPlaying)
                    return source;
            }
            return null;
        }

        #endregion

        #region Voice

        public void PlayVoice(AudioClip clip)
        {
            if (voiceSource != null && clip != null)
            {
                voiceSource.Stop();
                voiceSource.clip = clip;
                voiceSource.Play();
            }
        }

        public void StopVoice()
        {
            if (voiceSource != null)
            {
                voiceSource.Stop();
            }
        }

        #endregion

        #region Volume Control

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetVoiceVolume(float volume)
        {
            voiceVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;

            if (voiceSource != null)
                voiceSource.volume = voiceVolume * masterVolume;
        }

        #endregion
    }
}
