using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;

/**
 * Manages player money - earning, spending, and saving
 * Money is saved to Unity Cloud Save and persists across levels
 */
public class MoneyManager : MonoBehaviour
{
    [Header("Money Settings")]
    [SerializeField] private int startingMoney = 100;
    [SerializeField] private int moneyPerKill = 10; // Money earned when killing an enemy

    [Header("UI")]
    [SerializeField] private TMPro.TextMeshProUGUI moneyText; // Optional UI text to display money

    private int currentMoney = 0;
    private const string MONEY_KEY = "PlayerMoney";
    private bool isInitialized = false;

    // Singleton pattern for easy access
    private static MoneyManager instance;
    public static MoneyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MoneyManager>();
            }
            return instance;
        }
    }

    // Public property to get current money
    public int CurrentMoney => currentMoney;
    public int MoneyPerKill => moneyPerKill;

    async void Start()
    {
        instance = this;
        await LoadMoneyFromCloud();
        UpdateMoneyUI();
    }

    /**
     * Load money from Cloud Save
     */
    async Task LoadMoneyFromCloud()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("MoneyManager: Not signed in, using starting money");
            currentMoney = startingMoney;
            isInitialized = true;
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
                    currentMoney = savedMoney;
                    Debug.Log($"MoneyManager: Loaded {currentMoney} money from cloud");
                }
                else
                {
                    currentMoney = startingMoney;
                    Debug.LogWarning("MoneyManager: Failed to parse saved money, using starting money");
                }
            }
            else
            {
                currentMoney = startingMoney;
                Debug.Log("MoneyManager: No saved money found, using starting money");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MoneyManager: Error loading money from cloud: {ex.Message}");
            currentMoney = startingMoney;
        }

        isInitialized = true;
    }

    /**
     * Save money to Cloud Save
     */
    async Task SaveMoneyToCloud()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("MoneyManager: Not signed in, cannot save to cloud");
            return;
        }

        try
        {
            await DatabaseManager.SaveData((MONEY_KEY, currentMoney));
            Debug.Log($"MoneyManager: Saved {currentMoney} money to cloud");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MoneyManager: Error saving money to cloud: {ex.Message}");
        }
    }

    /**
     * Add money (e.g., when killing an enemy)
     */
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        currentMoney += amount;
        UpdateMoneyUI();
        _ = SaveMoneyToCloud(); // Save async but don't wait (fire-and-forget)
        Debug.Log($"MoneyManager: Added {amount} money. Total: {currentMoney}");
    }

    /**
     * Spend money (e.g., when buying a unit)
     * Returns true if successful, false if not enough money
     */
    public bool SpendMoney(int amount)
    {
        if (amount <= 0) return true; // Free items are always allowed

        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateMoneyUI();
            _ = SaveMoneyToCloud(); // Save async but don't wait (fire-and-forget)
            Debug.Log($"MoneyManager: Spent {amount} money. Remaining: {currentMoney}");
            return true;
        }

        Debug.LogWarning($"MoneyManager: Not enough money! Need {amount}, have {currentMoney}");
        return false;
    }

    /**
     * Check if player has enough money
     */
    public bool HasEnoughMoney(int amount)
    {
        return currentMoney >= amount;
    }

    /**
     * Update money display in UI
     */
    void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"Money: {currentMoney}";
        }
    }

    /**
     * Called when an enemy unit is killed - gives money to player
     */
    public void OnEnemyKilled()
    {
        if (isInitialized)
        {
            AddMoney(moneyPerKill);
        }
    }
}
