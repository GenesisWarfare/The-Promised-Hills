using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * AudioManager - Handles background music and sound effects
 * Persists across scenes (DontDestroyOnLoad)
 * Automatically plays appropriate music based on scene name
 */
public class AudioManager : MonoBehaviour
{
    [Header("Background Music")]
    [SerializeField] private AudioClip calmMusic; // For register, map scenes
    [SerializeField] private AudioClip combatMusic; // For battlefield, tutorial scenes

    [Header("Sound Effects")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip baseHitSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    [Header("Settings")]
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float sfxVolume = 1f;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    // Singleton pattern
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();
                if (instance == null)
                {
                    GameObject audioObj = new GameObject("AudioManager");
                    instance = audioObj.AddComponent<AudioManager>();
                    DontDestroyOnLoad(audioObj);
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
            LoadAudioClips();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Subscribe to scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Play music for current scene
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void SetupAudioSources()
    {
        // Music source (loops)
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        // SFX source (one-shots)
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    void LoadAudioClips()
    {
        // Audio clips should be assigned in Inspector
        // If not assigned, try to load from Resources (if moved to Resources folder)
        if (calmMusic == null)
        {
            calmMusic = Resources.Load<AudioClip>("sound_calm");
        }
        if (combatMusic == null)
        {
            combatMusic = Resources.Load<AudioClip>("sound_combat");
        }
        if (buttonClickSound == null)
        {
            // Try to load a proper button sound, but don't use hit sound as fallback
            buttonClickSound = Resources.Load<AudioClip>("sound_button");
            // If no button sound exists, leave it null (no sound will play)
        }
        if (baseHitSound == null)
        {
            baseHitSound = Resources.Load<AudioClip>("sound_impact");
        }
        if (winSound == null)
        {
            winSound = Resources.Load<AudioClip>("sound_win");
        }
        if (loseSound == null)
        {
            loseSound = Resources.Load<AudioClip>("sound_lose");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    void PlayMusicForScene(string sceneName)
    {
        if (musicSource == null) return;

        AudioClip musicToPlay = null;

        // Determine which music to play based on scene name
        string sceneLower = sceneName.ToLower();
        if (sceneLower.Contains("register") || sceneLower.Contains("login") || sceneLower.Contains("map") || sceneLower.Contains("mainmenu"))
        {
            musicToPlay = calmMusic;
        }
        else if (sceneLower.Contains("battlefield") || sceneLower.Contains("tutorial") || sceneLower.Contains("ella") || sceneLower.Contains("valley"))
        {
            musicToPlay = combatMusic;
        }

        // Play the music if it's different from what's currently playing, OR if music isn't playing
        if (musicToPlay != null)
        {
            if (musicSource.clip != musicToPlay || !musicSource.isPlaying)
            {
                musicSource.clip = musicToPlay;
                musicSource.Play();
            }
        }
        else if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    // Public methods to play sounds
    public void PlayButtonClick()
    {
        if (buttonClickSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }

    public void PlayBaseHit()
    {
        if (baseHitSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(baseHitSound);
        }
    }

    public void PlayWin()
    {
        if (winSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(winSound);
        }
    }

    public void PlayLose()
    {
        if (loseSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(loseSound);
        }
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    // Volume controls
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }
}
