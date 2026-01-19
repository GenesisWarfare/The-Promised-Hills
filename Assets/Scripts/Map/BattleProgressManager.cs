using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using System.Linq;

/**
 * BattleProgressManager - Tracks which battles have been won and manages kingdom unlocking
 * Kingdom progression:
 * - Kingdom of Israel (Level 1): Starts unlocked, has 3 battles
 * - Kingdom of Judah (Level 2): Unlocks when all Level 1 battles are won
 * - Hasmonean Kingdom (Level 3): Unlocks when all Level 2 battles are won
 * - Modern State of Israel (Level 4): Unlocks when all Level 3 battles are won
 */
public class BattleProgressManager : MonoBehaviour
{
    private const string BATTLE_PROGRESS_KEY = "BattleProgress"; // Key for saving won battles
    private const string UNLOCKED_KINGDOMS_KEY = "UnlockedKingdoms"; // Key for saving unlocked kingdoms

    // Track which battles are won (by scene name)
    private HashSet<string> wonBattles = new HashSet<string>();

    // Track which kingdoms are unlocked (1 = Kingdom of Israel, 2 = Judah, 3 = Hasmonean, 4 = Modern Israel)
    private HashSet<int> unlockedKingdoms = new HashSet<int>();

    // Define battles per kingdom (level number -> list of scene names)
    private Dictionary<int, List<string>> battlesPerKingdom = new Dictionary<int, List<string>>()
    {
        { 1, new List<string>() }, // Kingdom of Israel - will be populated from BattlefieldButtons
        { 2, new List<string>() }, // Kingdom of Judah
        { 3, new List<string>() }, // Hasmonean Kingdom
        { 4, new List<string>() }  // Modern State of Israel
    };

    // Singleton pattern
    private static BattleProgressManager instance;
    public static BattleProgressManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<BattleProgressManager>();
                if (instance == null)
                {
                    GameObject managerObj = new GameObject("BattleProgressManager");
                    instance = managerObj.AddComponent<BattleProgressManager>();
                    DontDestroyOnLoad(managerObj);
                }
            }
            return instance;
        }
    }

    void Awake()
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

        // Kingdom of Israel (Level 1) always starts unlocked
        unlockedKingdoms.Add(1);
    }

    async void Start()
    {
        // Wait for Unity Services to initialize
        await WaitForUnityServices();
        
        // Load battle progress from cloud save
        await LoadBattleProgress();
        
        // Populate battles per kingdom from BattlefieldButtons
        PopulateBattlesPerKingdom();
        
        // Check and unlock kingdoms based on won battles
        CheckAndUnlockKingdoms();
    }

    /**
     * Wait for Unity Services to be initialized
     */
    async Task WaitForUnityServices()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            return;
        }

        int maxWaitTime = 5;
        int waited = 0;
        while (UnityServices.State != ServicesInitializationState.Initialized && waited < maxWaitTime)
        {
            await Task.Delay(100);
            waited++;
        }

        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            try
            {
                await UnityServices.InitializeAsync();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"BattleProgressManager: Could not initialize Unity Services: {ex.Message}");
            }
        }
    }

    /**
     * Populate battles per kingdom from all BattlefieldButtons in the scene
     */
    void PopulateBattlesPerKingdom()
    {
        // Clear existing data
        foreach (var key in battlesPerKingdom.Keys.ToList())
        {
            battlesPerKingdom[key] = new List<string>();
        }

        // Find all BattlefieldButtons
        BattlefieldButton[] allButtons = FindObjectsByType<BattlefieldButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (BattlefieldButton button in allButtons)
        {
            if (button != null && !string.IsNullOrEmpty(button.GetSceneName()))
            {
                int level = button.GetLevelNumber();
                string sceneName = button.GetSceneName();
                
                if (battlesPerKingdom.ContainsKey(level))
                {
                    if (!battlesPerKingdom[level].Contains(sceneName))
                    {
                        battlesPerKingdom[level].Add(sceneName);
                        Debug.Log($"BattleProgressManager: Added battle '{sceneName}' to kingdom {level}");
                    }
                }
            }
        }

        // Log summary
        foreach (var kvp in battlesPerKingdom)
        {
            Debug.Log($"BattleProgressManager: Kingdom {kvp.Key} has {kvp.Value.Count} battles");
        }
    }

    /**
     * Mark a battle as won (called when player wins)
     */
    public async void MarkBattleAsWon(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("BattleProgressManager: Cannot mark battle as won - scene name is empty");
            return;
        }

        if (wonBattles.Contains(sceneName))
        {
            Debug.Log($"BattleProgressManager: Battle '{sceneName}' already marked as won");
            return;
        }

        wonBattles.Add(sceneName);
        Debug.Log($"BattleProgressManager: Marked battle '{sceneName}' as won");

        // Check if this unlocks a new kingdom
        CheckAndUnlockKingdoms();

        // Save to cloud
        await SaveBattleProgress();
    }

    /**
     * Check if a battle has been won
     */
    public bool IsBattleWon(string sceneName)
    {
        return wonBattles.Contains(sceneName);
    }

    /**
     * Check if a kingdom is unlocked
     */
    public bool IsKingdomUnlocked(int kingdomLevel)
    {
        return unlockedKingdoms.Contains(kingdomLevel);
    }

    /**
     * Check if all battles in a kingdom are won, and unlock the next kingdom if so
     */
    void CheckAndUnlockKingdoms()
    {
        // Check each kingdom (except the last one)
        for (int kingdom = 1; kingdom <= 3; kingdom++)
        {
            if (!battlesPerKingdom.ContainsKey(kingdom))
            {
                continue;
            }

            List<string> battles = battlesPerKingdom[kingdom];
            if (battles.Count == 0)
            {
                continue; // No battles defined for this kingdom yet
            }

            // Check if all battles in this kingdom are won
            bool allWon = true;
            foreach (string battleScene in battles)
            {
                if (!wonBattles.Contains(battleScene))
                {
                    allWon = false;
                    break;
                }
            }

            // If all battles are won, unlock the next kingdom
            if (allWon && kingdom < 4)
            {
                int nextKingdom = kingdom + 1;
                if (!unlockedKingdoms.Contains(nextKingdom))
                {
                    unlockedKingdoms.Add(nextKingdom);
                    Debug.Log($"BattleProgressManager: Unlocked Kingdom {nextKingdom} (all battles in Kingdom {kingdom} completed)");
                }
            }
        }
    }

    /**
     * Save battle progress to Cloud Save
     */
    async Task SaveBattleProgress()
    {
        // Don't save for guests
        if (Player.Instance != null)
        {
            string playerName = Player.Instance.Name;
            bool isGuest = playerName.Equals("Guest", System.StringComparison.OrdinalIgnoreCase);
            if (isGuest)
            {
                Debug.Log("BattleProgressManager: Guest player - not saving battle progress");
                return;
            }
        }

        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogWarning("BattleProgressManager: Unity Services not initialized, cannot save");
            return;
        }

        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogWarning("BattleProgressManager: Not signed in, cannot save");
                return;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"BattleProgressManager: AuthenticationService not available: {ex.Message}");
            return;
        }

        try
        {
            // Convert won battles to comma-separated string
            string wonBattlesString = string.Join(",", wonBattles);
            
            // Convert unlocked kingdoms to comma-separated string
            string unlockedKingdomsString = string.Join(",", unlockedKingdoms.Select(k => k.ToString()));

            await DatabaseManager.SaveData(
                (BATTLE_PROGRESS_KEY, wonBattlesString),
                (UNLOCKED_KINGDOMS_KEY, unlockedKingdomsString)
            );

            Debug.Log($"BattleProgressManager: Saved battle progress ({wonBattles.Count} won battles, {unlockedKingdoms.Count} unlocked kingdoms)");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"BattleProgressManager: Error saving battle progress: {ex.Message}");
        }
    }

    /**
     * Load battle progress from Cloud Save
     */
    async Task LoadBattleProgress()
    {
        // Don't load for guests
        if (Player.Instance != null)
        {
            string playerName = Player.Instance.Name;
            bool isGuest = playerName.Equals("Guest", System.StringComparison.OrdinalIgnoreCase);
            if (isGuest)
            {
                Debug.Log("BattleProgressManager: Guest player - starting fresh (no battle progress loaded)");
                return;
            }
        }

        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogWarning("BattleProgressManager: Unity Services not initialized, cannot load");
            return;
        }

        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogWarning("BattleProgressManager: Not signed in, cannot load");
                return;
            }
        }
        catch (System.Exception)
        {
            return;
        }

        try
        {
            var data = await DatabaseManager.LoadData(BATTLE_PROGRESS_KEY, UNLOCKED_KINGDOMS_KEY);

            // Load won battles
            if (data.ContainsKey(BATTLE_PROGRESS_KEY) && data[BATTLE_PROGRESS_KEY] != null)
            {
                string wonBattlesString = data[BATTLE_PROGRESS_KEY].Value.GetAsString();
                if (!string.IsNullOrEmpty(wonBattlesString))
                {
                    string[] battles = wonBattlesString.Split(',');
                    wonBattles = new HashSet<string>(battles);
                    Debug.Log($"BattleProgressManager: Loaded {wonBattles.Count} won battles from cloud");
                }
            }

            // Load unlocked kingdoms
            if (data.ContainsKey(UNLOCKED_KINGDOMS_KEY) && data[UNLOCKED_KINGDOMS_KEY] != null)
            {
                string unlockedKingdomsString = data[UNLOCKED_KINGDOMS_KEY].Value.GetAsString();
                if (!string.IsNullOrEmpty(unlockedKingdomsString))
                {
                    string[] kingdoms = unlockedKingdomsString.Split(',');
                    unlockedKingdoms = new HashSet<int>(kingdoms.Select(k => int.Parse(k)));
                    Debug.Log($"BattleProgressManager: Loaded {unlockedKingdoms.Count} unlocked kingdoms from cloud");
                }
            }

            // Ensure Kingdom of Israel (Level 1) is always unlocked
            if (!unlockedKingdoms.Contains(1))
            {
                unlockedKingdoms.Add(1);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"BattleProgressManager: Error loading battle progress: {ex.Message}");
        }
    }

    /**
     * Get all won battles (for debugging)
     */
    public HashSet<string> GetWonBattles()
    {
        return new HashSet<string>(wonBattles);
    }

    /**
     * Get all unlocked kingdoms (for debugging)
     */
    public HashSet<int> GetUnlockedKingdoms()
    {
        return new HashSet<int>(unlockedKingdoms);
    }

    /**
     * Refresh battles per kingdom and check unlocks (call this when map scene loads)
     */
    public void RefreshBattlesAndCheckUnlocks()
    {
        PopulateBattlesPerKingdom();
        CheckAndUnlockKingdoms();
    }
}
