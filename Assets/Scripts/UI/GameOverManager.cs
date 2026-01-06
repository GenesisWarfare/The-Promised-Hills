using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/**
 * Manages win/lose screens when bases are destroyed
 */
public class GameOverManager : MonoBehaviour
{
    [Header("Win Screen")]
    [SerializeField] private GameObject winPanel; // Image GameObject
    [SerializeField] private UnityEngine.UI.Button winBackToMapButton;

    [Header("Lose Screen")]
    [SerializeField] private GameObject losePanel; // Image GameObject
    [SerializeField] private UnityEngine.UI.Button loseBackToMapButton;

    [Header("Settings")]
    [SerializeField] private string mapSceneName = "Map";

    private BaseManager baseManager;
    private bool gameOver = false;

    void Start()
    {
        // Hide panels initially
        if (winPanel != null)
            winPanel.SetActive(false);
        if (losePanel != null)
            losePanel.SetActive(false);

        // Setup buttons
        if (winBackToMapButton != null)
        {
            winBackToMapButton.onClick.AddListener(GoToMap);
        }

        if (loseBackToMapButton != null)
        {
            loseBackToMapButton.onClick.AddListener(GoToMap);
        }

        // Wait a frame for BaseManager to create bases
        StartCoroutine(SubscribeToBases());
    }

    private System.Collections.IEnumerator SubscribeToBases()
    {
        // Wait a frame to ensure BaseManager has created bases
        yield return null;

        // Find BaseManager
        baseManager = FindFirstObjectByType<BaseManager>();
        if (baseManager == null)
        {
            Debug.LogError("GameOverManager: BaseManager not found!");
            yield break;
        }

        // Try to get bases - may need to wait a bit more
        GameBase playerBase = baseManager.GetPlayerBase();
        GameBase enemyBase = baseManager.GetEnemyBase();

        int attempts = 0;
        while ((playerBase == null || enemyBase == null) && attempts < 10)
        {
            yield return null;
            playerBase = baseManager.GetPlayerBase();
            enemyBase = baseManager.GetEnemyBase();
            attempts++;
        }

        // Subscribe to base health changes
        if (playerBase != null)
        {
            playerBase.OnHealthChanged += OnPlayerBaseHealthChanged;
            Debug.Log("GameOverManager: Subscribed to player base");
        }
        else
        {
            Debug.LogError("GameOverManager: Player base not found!");
        }

        if (enemyBase != null)
        {
            enemyBase.OnHealthChanged += OnEnemyBaseHealthChanged;
            Debug.Log("GameOverManager: Subscribed to enemy base");
        }
        else
        {
            Debug.LogError("GameOverManager: Enemy base not found!");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (baseManager != null)
        {
            GameBase playerBase = baseManager.GetPlayerBase();
            GameBase enemyBase = baseManager.GetEnemyBase();

            if (playerBase != null)
            {
                playerBase.OnHealthChanged -= OnPlayerBaseHealthChanged;
            }

            if (enemyBase != null)
            {
                enemyBase.OnHealthChanged -= OnEnemyBaseHealthChanged;
            }
        }
    }

    void OnPlayerBaseHealthChanged(int currentHealth, int maxHealth)
    {
        if (gameOver) return;

        if (currentHealth <= 0)
        {
            gameOver = true;
            ShowLoseScreen();
        }
    }

    void OnEnemyBaseHealthChanged(int currentHealth, int maxHealth)
    {
        if (gameOver) return;

        if (currentHealth <= 0)
        {
            gameOver = true;
            ShowWinScreen();
        }
    }

    void ShowWinScreen()
    {
        Debug.Log("You Win!");

        // Stop background music and play win sound
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.StopMusic();
            audioManager.PlayWin();
        }

        // Stop time or pause game
        Time.timeScale = 0f;

        if (winPanel != null)
        {
            EnsureImageSetup(winPanel);
            winPanel.SetActive(true);
        }

        // Ensure button is active and visible
        if (winBackToMapButton != null)
        {
            winBackToMapButton.gameObject.SetActive(true);
            winBackToMapButton.interactable = true;
        }
    }

    void ShowLoseScreen()
    {
        Debug.Log("You Lose!");

        // Stop background music and play lose sound
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.StopMusic();
            audioManager.PlayLose();
        }

        // Stop time or pause game
        Time.timeScale = 0f;

        if (losePanel != null)
        {
            EnsureImageSetup(losePanel);
            losePanel.SetActive(true);
        }

        // Ensure button is active and visible
        if (loseBackToMapButton != null)
        {
            loseBackToMapButton.gameObject.SetActive(true);
            loseBackToMapButton.interactable = true;
        }
    }

    /**
     * Ensure Image is properly set up and renders on top
     */
    void EnsureImageSetup(GameObject imageObject)
    {
        if (imageObject == null) return;

        // Ensure Image component exists (should already be there)
        Image image = imageObject.GetComponent<Image>();
        if (image == null)
        {
            Debug.LogWarning($"GameOverManager: {imageObject.name} is missing an Image component!");
            return;
        }

        // Ensure the Canvas has high sorting order to render on top
        Canvas canvas = imageObject.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            // Set very high sorting order so it renders above everything
            canvas.sortingOrder = 1000;
        }
    }

    public void GoToMap()
    {
        // Save money before going back to map
        Player player = Player.Instance;
        if (player != null)
        {
            player.SaveMoneyNow();
        }

        // Resume time before loading scene
        Time.timeScale = 1f;
        SceneManager.LoadScene(mapSceneName);
    }

    public void RestartLevel()
    {
        // Resume time before reloading scene
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
