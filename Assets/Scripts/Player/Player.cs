using UnityEngine;
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
    public void SetCredentials(string name, string password)
    {
        this.playerName = name;
        this.password = password;
        SavePlayerData();
        Debug.Log($"Player: Set credentials for {name}");
    }

    /**
     * Load player name and password from PlayerPrefs
     */
    void LoadPlayerData()
    {
        playerName = PlayerPrefs.GetString(NAME_KEY, "");
        password = PlayerPrefs.GetString(PASSWORD_KEY, "");

        if (!string.IsNullOrEmpty(playerName))
        {
            Debug.Log($"Player: Loaded player data for {playerName}");
        }
    }

    /**
     * Save player name and password to PlayerPrefs
     */
    void SavePlayerData()
    {
        if (!string.IsNullOrEmpty(playerName))
        {
            PlayerPrefs.SetString(NAME_KEY, playerName);
            PlayerPrefs.SetString(PASSWORD_KEY, password);
            PlayerPrefs.Save();
            Debug.Log($"Player: Saved player data for {playerName}");
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
        if (moneyText != null)
        {
            moneyText.text = $"Money: {money}";
        }
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