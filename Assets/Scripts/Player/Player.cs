using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;

/**
 * Player - Central player data object
 * Contains: name, password, and money
 * Handles saving/loading from PlayerPrefs and Cloud Save
 */
public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private int startingMoney = 100;
    [SerializeField] private int moneyPerKill = 10; // Money earned when killing an enemy

    [Header("UI")]
    [SerializeField] private TMPro.TextMeshProUGUI moneyText; // Optional UI text to display money

    // Player data
    private string playerName = "";
    private string password = "";
    private int money = 0;

    // Keys for saving
    private const string NAME_KEY = "PlayerName";
    private const string PASSWORD_KEY = "PlayerPassword";
    private const string MONEY_KEY = "PlayerMoney";

    // Singleton pattern
    private static Player instance;
    public static Player Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<Player>();
                if (instance == null)
                {
                    // Create a new GameObject with Player component if none exists
                    GameObject playerObj = new GameObject("Player");
                    instance = playerObj.AddComponent<Player>();
                    DontDestroyOnLoad(playerObj);
                }
            }
            return instance;
        }
    }

    // Public properties
    public string Name => playerName;
    public string Password => password;
    public int Money => money;

    async void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Load player data
        LoadPlayerData();

        // Wait for Unity Services to initialize before loading money
        await WaitForUnityServices();
        await LoadMoneyFromCloud();
        UpdateMoneyUI();

        // Subscribe to scene loaded event to update money UI when entering any scene
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Start coroutine to continuously update money UI
        StartCoroutine(ContinuousMoneyUIUpdate());
    }

    /**
     * Coroutine that continuously tries to find and update money UI
     * This ensures the money text is always found and updated, even if UI loads late
     */
    IEnumerator ContinuousMoneyUIUpdate()
    {
        while (true)
        {
            // Reset reference if invalid
            if (moneyText == null || moneyText.gameObject == null)
            {
                moneyText = null;
            }

            // Try to find and update money text
            UpdateMoneyUI();

            // Wait a bit before trying again (not too often to avoid performance issues)
            yield return new WaitForSeconds(0.5f);
        }
    }

    /**
     * Called whenever a scene is loaded - updates money UI
     */
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset moneyText reference so it finds the new one in this scene
        moneyText = null;
        // Start a coroutine to find the text after a small delay (UI might not be ready yet)
        StartCoroutine(FindMoneyTextAfterDelay());
    }

    /**
     * Wait a moment for UI to initialize, then find and update money text
     */
    IEnumerator FindMoneyTextAfterDelay()
    {
        // Wait a frame for UI to initialize
        yield return null;
        // Try to find and update
        UpdateMoneyUI();
    }

    void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /**
     * Wait for Unity Services to be initialized
     */
    async Task WaitForUnityServices()
    {
        // Check if Unity Services is already initialized
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            return;
        }

        // If not initialized, wait a bit and check again
        int maxWaitTime = 5; // Max 5 seconds
        int waited = 0;
        while (UnityServices.State != ServicesInitializationState.Initialized && waited < maxWaitTime)
        {
            await Task.Delay(100); // Wait 100ms
            waited++;
        }

        // If still not initialized, try to initialize it ourselves
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            try
            {
                await UnityServices.InitializeAsync();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Player: Could not initialize Unity Services: {ex.Message}");
            }
        }
    }

    /**
     * Set player name and password (called after login/register)
     */
    public async void SetCredentials(string name, string password)
    {
        this.playerName = name;
        this.password = password;
        SavePlayerData(); // Save locally to PlayerPrefs
        _ = SavePlayerDataToCloud(); // Save to Cloud Save (fire-and-forget)
        Debug.Log($"Player: Set credentials for {name}");
    }

    /**
     * Load player name and password from PlayerPrefs and Cloud Save
     */
    async void LoadPlayerData()
    {
        // Load from PlayerPrefs first (for quick access)
        playerName = PlayerPrefs.GetString(NAME_KEY, "");
        password = PlayerPrefs.GetString(PASSWORD_KEY, "");

        // Also try to load from Cloud Save (will override if available)
        await LoadPlayerDataFromCloud();

        if (!string.IsNullOrEmpty(playerName))
        {
            Debug.Log($"Player: Loaded player data for {playerName}");
        }
    }

    /**
     * Save player name and password to PlayerPrefs (local)
     */
    void SavePlayerData()
    {
        if (!string.IsNullOrEmpty(playerName))
        {
            PlayerPrefs.SetString(NAME_KEY, playerName);
            PlayerPrefs.SetString(PASSWORD_KEY, password);
            PlayerPrefs.Save();
            Debug.Log($"Player: Saved player data locally for {playerName}");
        }
    }

    /**
     * Save player name and password to Cloud Save
     */
    async Task SavePlayerDataToCloud()
    {
        // Check if Unity Services is initialized
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogWarning("Player: Unity Services not initialized, cannot save player data to cloud");
            return;
        }

        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogWarning("Player: Not signed in, cannot save player data to cloud");
                return;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Player: AuthenticationService not available: {ex.Message}");
            return;
        }

        try
        {
            await DatabaseManager.SaveData((NAME_KEY, playerName), (PASSWORD_KEY, password));
            Debug.Log($"Player: Saved player data (name, password) to cloud for {playerName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Player: Error saving player data to cloud: {ex.Message}");
        }
    }

    /**
     * Load player name and password from Cloud Save
     */
    async Task LoadPlayerDataFromCloud()
    {
        // Check if Unity Services is initialized
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            return;
        }

        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                return;
            }
        }
        catch (System.Exception)
        {
            return;
        }

        try
        {
            var data = await DatabaseManager.LoadData(NAME_KEY, PASSWORD_KEY);

            if (data.ContainsKey(NAME_KEY) && data[NAME_KEY] != null)
            {
                string savedName = data[NAME_KEY].Value.GetAsString();
                if (!string.IsNullOrEmpty(savedName))
                {
                    playerName = savedName;
                    Debug.Log($"Player: Loaded name from cloud: {playerName}");
                }
            }

            if (data.ContainsKey(PASSWORD_KEY) && data[PASSWORD_KEY] != null)
            {
                string savedPassword = data[PASSWORD_KEY].Value.GetAsString();
                if (!string.IsNullOrEmpty(savedPassword))
                {
                    password = savedPassword;
                    Debug.Log($"Player: Loaded password from cloud");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Player: Error loading player data from cloud: {ex.Message}");
        }
    }

    /**
     * Load money from Cloud Save
     */
    async Task LoadMoneyFromCloud()
    {
        // Check if Unity Services is initialized and user is signed in
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogWarning("Player: Unity Services not initialized, using starting money");
            money = startingMoney;
            return;
        }

        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogWarning("Player: Not signed in, using starting money");
                money = startingMoney;
                return;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Player: AuthenticationService not available: {ex.Message}, using starting money");
            money = startingMoney;
            return;
        }

        try
        {
            var data = await DatabaseManager.LoadData(MONEY_KEY);
            if (data.ContainsKey(MONEY_KEY) && data[MONEY_KEY] != null)
            {
                string moneyValue = data[MONEY_KEY].Value.GetAsString();
                if (int.TryParse(moneyValue, out int savedMoney))
                {
                    money = savedMoney;
                    Debug.Log($"Player: Loaded {money} money from cloud");
                }
                else
                {
                    money = startingMoney;
                    Debug.LogWarning("Player: Failed to parse saved money, using starting money");
                }
            }
            else
            {
                money = startingMoney;
                Debug.Log("Player: No saved money found, using starting money");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Player: Error loading money from cloud: {ex.Message}");
            money = startingMoney;
        }
    }

    /**
     * Save money to Cloud Save immediately (called when needed)
     */
    public async void SaveMoneyNow()
    {
        await SaveMoneyToCloud();
    }

    /**
     * Save money to Cloud Save
     */
    async Task SaveMoneyToCloud()
    {
        // Check if Unity Services is initialized
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogWarning("Player: Unity Services not initialized, cannot save to cloud");
            return;
        }

        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogWarning("Player: Not signed in, cannot save to cloud");
                return;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Player: AuthenticationService not available: {ex.Message}");
            return;
        }

        try
        {
            await DatabaseManager.SaveData((MONEY_KEY, money));
            Debug.Log($"Player: Saved {money} money to cloud");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Player: Error saving money to cloud: {ex.Message}");
        }
    }

    /**
     * Set money directly (used for initialization)
     */
    public void SetMoney(int amount)
    {
        money = amount;
        UpdateMoneyUI();
        Debug.Log($"Player: Set money to {money}");
    }

    /**
     * Add money (e.g., when killing an enemy)
     */
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        money += amount;
        UpdateMoneyUI();
        _ = SaveMoneyToCloud(); // Save async but don't wait (fire-and-forget)
        Debug.Log($"Player: Added {amount} money. Total: {money}");
    }

    /**
     * Spend money (e.g., when buying a unit)
     * Returns true if successful, false if not enough money
     */
    public bool SpendMoney(int amount)
    {
        if (amount <= 0) return true; // Free items are always allowed

        if (money >= amount)
        {
            money -= amount;
            UpdateMoneyUI();
            _ = SaveMoneyToCloud(); // Save async but don't wait (fire-and-forget)
            Debug.Log($"Player: Spent {amount} money. Remaining: {money}");
            return true;
        }

        Debug.LogWarning($"Player: Not enough money! Need {amount}, have {money}");
        return false;
    }

    /**
     * Check if player has enough money
     */
    public bool HasEnoughMoney(int amount)
    {
        return money >= amount;
    }

    /**
     * Update money display in UI
     */
    void UpdateMoneyUI()
    {
        // Try to find money text if not assigned or if the reference is invalid
        if (moneyText == null || moneyText.gameObject == null)
        {
            FindMoneyTextInScene();
        }

        if (moneyText != null)
        {
            moneyText.text = money.ToString();
        }
    }

    /**
     * Find money text UI in current scene
     */
    void FindMoneyTextInScene()
    {
        // Try common names (exact matches first)
        GameObject moneyObj = GameObject.Find("MoneyText");
        if (moneyObj == null) moneyObj = GameObject.Find("Money_Text");
        if (moneyObj == null) moneyObj = GameObject.Find("MoneyTextUI");
        if (moneyObj == null) moneyObj = GameObject.Find("Money");
        if (moneyObj == null) moneyObj = GameObject.Find("MoneyText (TMP)");

        if (moneyObj != null)
        {
            moneyText = moneyObj.GetComponent<TMPro.TextMeshProUGUI>();
            if (moneyText != null)
            {
                Debug.Log($"Player: Found money text by name: {moneyObj.name}");
                return;
            }
        }

        // Search all TextMeshProUGUI components for one with "money" in name
        TMPro.TextMeshProUGUI[] allTexts = FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (TMPro.TextMeshProUGUI text in allTexts)
        {
            if (text == null || text.gameObject == null) continue;

            string name = text.name.ToLower();
            string goName = text.gameObject.name.ToLower();
            if (name.Contains("money") || goName.Contains("money"))
            {
                moneyText = text;
                Debug.Log($"Player: Found money text by search: {text.gameObject.name}");
                return;
            }
        }

        Debug.LogWarning("Player: Could not find money text UI in current scene. Make sure there's a TextMeshProUGUI with 'money' in its name.");
    }

    /**
     * Called when an enemy unit is killed - gives money to player
     * Money earned = enemy's cost + 3
     */
    public void OnEnemyKilled(Unit enemyUnit)
    {
        if (enemyUnit != null)
        {
            int reward = enemyUnit.Cost + 3;
            AddMoney(reward);
        }
        else
        {
            // Fallback to default if no enemy unit provided
            AddMoney(moneyPerKill);
        }
    }

    /**
     * Clear all player data (for logout)
     */
    public void ClearPlayerData()
    {
        playerName = "";
        password = "";
        money = startingMoney;

        PlayerPrefs.DeleteKey(NAME_KEY);
        PlayerPrefs.DeleteKey(PASSWORD_KEY);
        PlayerPrefs.Save();

        Debug.Log("Player: Cleared all player data");
    }
}