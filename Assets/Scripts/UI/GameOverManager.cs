using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/**
 * Manages win/lose screens when bases are destroyed
 */
public class GameOverManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Win Screen")]
    [SerializeField] private UnityEngine.UI.Button backToMapButton;
    [SerializeField] private TextMeshProUGUI winText;

    [Header("Lose Screen")]
    [SerializeField] private TextMeshProUGUI loseText;

    [Header("Settings")]
    [SerializeField] private string mapSceneName = "Map";
    [SerializeField] private float delayBeforeShowingScreen = 1f;

    private BaseManager baseManager;
    private bool gameOver = false;

    void Start()
    {
        // Hide panels initially
        if (winPanel != null)
            winPanel.SetActive(false);
        if (losePanel != null)
            losePanel.SetActive(false);

        // Setup button
        if (backToMapButton != null)
        {
            backToMapButton.onClick.AddListener(GoToMap);
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
        
        // Stop time or pause game
        Time.timeScale = 0f;

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (winText != null)
        {
            winText.text = "You Win!";
        }
    }

    void ShowLoseScreen()
    {
        Debug.Log("You Lose!");
        
        // Stop time or pause game
        Time.timeScale = 0f;

        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }

        if (loseText != null)
        {
            loseText.text = "You Lose!";
        }
    }

    public void GoToMap()
    {
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
