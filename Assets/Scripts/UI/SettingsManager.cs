using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/**
 * SettingsManager - Manages settings UI in the map scene
 * Handles volume controls and other settings
 */
public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button openSettingsButton; // Button to open settings (optional)

    [Header("Settings")]
    [SerializeField] private bool startHidden = true;

    private AudioManager audioManager;

    // Static instance for easy access
    private static SettingsManager instance;
    public static SettingsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SettingsManager>();
            }
            return instance;
        }
    }

    void Awake()
    {
        // Set static instance
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Close settings panel immediately when component initializes (runs before Start)
        // Do this BEFORE anything else can see it
        if (settingsPanel != null)
        {
            string currentScene = SceneManager.GetActiveScene().name.ToLower();
            if (currentScene.Contains("map"))
            {
                settingsPanel.SetActive(false); // Always start closed in map scene
            }
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Close settings panel when map scene loads (including when returning from battlefield)
        string sceneName = scene.name.ToLower();
        if (sceneName.Contains("map") && settingsPanel != null)
        {
            // Close immediately - do this synchronously before any frame renders
            settingsPanel.SetActive(false);
            // Also close after a small delay to ensure it stays closed (in case something else opens it)
            StartCoroutine(EnsureSettingsClosedAfterDelay());
        }
    }

    void OnEnable()
    {
        // Subscribe to scene loaded event to close panel when map scene loads
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private System.Collections.IEnumerator EnsureSettingsClosedAfterDelay()
    {
        // Wait multiple frames to ensure it stays closed
        yield return null; // Wait 1 frame
        yield return null; // Wait another frame
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    void Start()
    {
        // Find AudioManager
        audioManager = AudioManager.Instance;

        // Validate settingsPanel reference
        if (settingsPanel == null)
        {
            Debug.LogError("SettingsManager: settingsPanel is not assigned! Please assign it in the Inspector.");
        }

        // Initialize UI - close settings panel when map scene loads (backup check)
        if (settingsPanel != null)
        {
            string currentScene = SceneManager.GetActiveScene().name.ToLower();
            if (currentScene.Contains("map"))
            {
                settingsPanel.SetActive(false); // Always start closed in map scene
            }
            else
            {
                // For non-map scenes, respect startHidden setting
                settingsPanel.SetActive(!startHidden);
            }
        }

        // Setup sliders
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.value = 0.7f; // Default music volume
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.value = 1f; // Default SFX volume
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Setup buttons
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }

        if (openSettingsButton != null)
        {
            openSettingsButton.onClick.AddListener(OpenSettings);
        }

        // Load saved volume settings
        LoadVolumeSettings();

        // Update UI
        UpdateVolumeTexts();
    }

    void LoadVolumeSettings()
    {
        // Load from PlayerPrefs if available, otherwise use AudioManager defaults
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", -1f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", -1f);

        // If no saved settings, use AudioManager's current values
        if (savedMusicVolume < 0 && audioManager != null)
        {
            savedMusicVolume = audioManager.GetMusicVolume();
        }
        else if (savedMusicVolume < 0)
        {
            savedMusicVolume = 0.7f; // Default fallback
        }

        if (savedSFXVolume < 0 && audioManager != null)
        {
            savedSFXVolume = audioManager.GetSFXVolume();
        }
        else if (savedSFXVolume < 0)
        {
            savedSFXVolume = 1f; // Default fallback
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = savedMusicVolume;
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = savedSFXVolume;
        }

        // Apply to AudioManager
        if (audioManager != null)
        {
            audioManager.SetMusicVolume(savedMusicVolume);
            audioManager.SetSFXVolume(savedSFXVolume);
        }
    }

    void SaveVolumeSettings()
    {
        if (musicVolumeSlider != null)
        {
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        }

        if (sfxVolumeSlider != null)
        {
            PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        }

        PlayerPrefs.Save();
    }

    void OnMusicVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetMusicVolume(value);
        }

        UpdateVolumeTexts();
        SaveVolumeSettings();
    }

    void OnSFXVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetSFXVolume(value);
        }

        UpdateVolumeTexts();
        SaveVolumeSettings();
    }

    void UpdateVolumeTexts()
    {
        if (musicVolumeText != null && musicVolumeSlider != null)
        {
            int percentage = Mathf.RoundToInt(musicVolumeSlider.value * 100f);
            musicVolumeText.text = $"Music: {percentage}%";
        }

        if (sfxVolumeText != null && sfxVolumeSlider != null)
        {
            int percentage = Mathf.RoundToInt(sfxVolumeSlider.value * 100f);
            sfxVolumeText.text = $"SFX: {percentage}%";
        }
    }

    public void OpenSettings()
    {
        Debug.Log("OpenSettings() called");

        if (settingsPanel == null)
        {
            Debug.LogError("SettingsManager: settingsPanel is null! Make sure it's assigned in the Inspector.");
            return;
        }

        Debug.Log($"Settings panel active state before: {settingsPanel.activeSelf}");
        settingsPanel.SetActive(true);
        Debug.Log($"Settings panel active state after: {settingsPanel.activeSelf}");

        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlayButtonClick();
        }
        else
        {
            Debug.LogWarning("SettingsManager: audioManager is null");
        }
    }

    // Static method that can be called from anywhere
    public static void OpenSettingsStatic()
    {
        if (Instance != null)
        {
            Instance.OpenSettings();
        }
        else
        {
            Debug.LogError("SettingsManager: No instance found! Make sure SettingsManager exists in the scene.");
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlayButtonClick();
        }
    }

    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            bool isActive = settingsPanel.activeSelf;
            settingsPanel.SetActive(!isActive);
        }

        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlayButtonClick();
        }
    }

    // Check if settings panel is currently open
    public bool IsSettingsOpen()
    {
        return settingsPanel != null && settingsPanel.activeSelf;
    }
}
