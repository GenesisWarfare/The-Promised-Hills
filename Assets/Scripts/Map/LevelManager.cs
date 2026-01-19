using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    private int currentSelectedLevel = 1;
    private List<BattlefieldButton> allBattlefieldButtons = new List<BattlefieldButton>();
    private List<LevelSelectionButton> levelSelectionButtons = new List<LevelSelectionButton>();

    private const string SAVED_LEVEL_KEY = "LastSelectedLevel";

    void Awake()
    {
        Debug.Log("=== LevelManager.Awake() START ===");

        // Find all battlefield buttons - make sure they're all active when we find them
        BattlefieldButton[] allButtons = FindObjectsByType<BattlefieldButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"[LevelManager] Found {allButtons.Length} BattlefieldButtons in Awake");
        foreach (BattlefieldButton button in allButtons)
        {
            if (button != null)
            {
                allBattlefieldButtons.Add(button);
                Debug.Log($"[LevelManager] Added BattlefieldButton '{button.gameObject.name}' with level: {button.GetLevelNumber()}, active: {button.gameObject.activeSelf}");
            }
            else
            {
                Debug.LogWarning("[LevelManager] Found null BattlefieldButton in Awake");
            }
        }

        // Find all level selection buttons
        LevelSelectionButton[] allLevelButtons = FindObjectsByType<LevelSelectionButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"[LevelManager] Found {allLevelButtons.Length} LevelSelectionButtons in Awake");
        foreach (LevelSelectionButton button in allLevelButtons)
        {
            if (button != null)
            {
                levelSelectionButtons.Add(button);
                Debug.Log($"[LevelManager] Added LevelSelectionButton '{button.gameObject.name}' with level: {button.GetLevelNumber()}, active: {button.gameObject.activeSelf}");
            }
            else
            {
                Debug.LogWarning("[LevelManager] Found null LevelSelectionButton in Awake");
            }
        }

        Debug.Log($"=== LevelManager.Awake() END - Total BattlefieldButtons: {allBattlefieldButtons.Count}, Total LevelSelectionButtons: {levelSelectionButtons.Count} ===");
    }

    void Start()
    {
        Debug.Log("=== LevelManager.Start() START ===");

        // Refresh button lists to ensure we have all buttons
        RefreshButtonLists();

        // Wait for BattleProgressManager to load progress before selecting level
        StartCoroutine(WaitForProgressAndInitialize());
    }

    IEnumerator WaitForProgressAndInitialize()
    {
        Debug.Log("[LevelManager] Waiting for BattleProgressManager to load progress...");

        BattleProgressManager progressManager = BattleProgressManager.Instance;
        if (progressManager == null)
        {
            Debug.LogWarning("[LevelManager] BattleProgressManager not found, proceeding anyway...");
        }
        else
        {
            // Wait for BattleProgressManager to finish loading progress
            int maxWaitTime = 50; // 5 seconds max wait
            int waited = 0;
            while (!progressManager.IsProgressLoaded && waited < maxWaitTime)
            {
                yield return new WaitForSeconds(0.1f);
                waited++;
            }

            if (progressManager.IsProgressLoaded)
            {
                Debug.Log("[LevelManager] BattleProgressManager finished loading progress, refreshing battles and unlocks...");
            }
            else
            {
                Debug.LogWarning("[LevelManager] BattleProgressManager did not finish loading in time, proceeding anyway...");
            }

            // Refresh battle progress manager to update battles list and check unlocks
            if (progressManager != null)
            {
                progressManager.RefreshBattlesAndCheckUnlocks();
                // Give it one more frame to process
                yield return null;
            }
        }

        Debug.Log($"[LevelManager] Hiding all {allBattlefieldButtons.Count} battlefield buttons...");
        // Hide all battlefield buttons first
        foreach (BattlefieldButton button in allBattlefieldButtons)
        {
            if (button != null && button.gameObject != null)
            {
                Debug.Log($"[LevelManager] Hiding BattlefieldButton '{button.gameObject.name}' (level {button.GetLevelNumber()})");
                button.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"[LevelManager] Cannot hide button - button is null or gameObject is null");
            }
        }

        // Load last selected level, but only if it's unlocked
        int savedLevel = PlayerPrefs.GetInt(SAVED_LEVEL_KEY, 1);
        if (progressManager != null && !progressManager.IsKingdomUnlocked(savedLevel))
        {
            // If saved level is not unlocked, default to level 1
            savedLevel = 1;
            Debug.Log($"[LevelManager] Saved level {savedLevel} was not unlocked, defaulting to level 1");
        }
        Debug.Log($"[LevelManager] Loading saved level: {savedLevel}");
        SelectLevel(savedLevel);

        // Refresh all button visuals now that progress is loaded
        RefreshAllButtonVisuals();

        Debug.Log("=== LevelManager.Start() END ===");
    }

    /**
     * Refresh all button visuals (kingdom buttons and battlefield buttons) based on current progress
     */
    void RefreshAllButtonVisuals()
    {
        Debug.Log("[LevelManager] Refreshing all button visuals...");

        // Refresh all level selection buttons (kingdom buttons)
        foreach (LevelSelectionButton button in levelSelectionButtons)
        {
            if (button != null)
            {
                button.RefreshVisualState();
            }
        }

        // Refresh all battlefield buttons (to show flags for won battles)
        foreach (BattlefieldButton button in allBattlefieldButtons)
        {
            if (button != null)
            {
                button.RefreshVisualState();
            }
        }

        Debug.Log($"[LevelManager] Refreshed {levelSelectionButtons.Count} level selection buttons and {allBattlefieldButtons.Count} battlefield buttons");
    }

    public void SelectLevel(int levelNumber)
    {
        Debug.Log($"=== LevelManager.SelectLevel({levelNumber}) START ===");

        // Check if this kingdom is unlocked
        BattleProgressManager progressManager = BattleProgressManager.Instance;
        if (progressManager != null && !progressManager.IsKingdomUnlocked(levelNumber))
        {
            Debug.LogWarning($"[LevelManager] Cannot select level {levelNumber} - kingdom is not unlocked!");
            return;
        }

        currentSelectedLevel = levelNumber;

        // Save the selected level so it persists after returning from battle
        PlayerPrefs.SetInt(SAVED_LEVEL_KEY, levelNumber);
        PlayerPrefs.Save();
        Debug.Log($"[LevelManager] Saved selected level {levelNumber} to PlayerPrefs");

        Debug.Log($"[LevelManager] SelectLevel called with levelNumber: {levelNumber}");
        Debug.Log($"[LevelManager] Previous level was: {currentSelectedLevel}");

        // Refresh button list in case buttons were added dynamically
        Debug.Log("[LevelManager] Refreshing button lists...");
        RefreshButtonLists();

        Debug.Log($"[LevelManager] Total battlefield buttons found: {allBattlefieldButtons.Count}");

        int buttonsShown = 0;
        int buttonsHidden = 0;

        // Show/hide battlefield buttons based on level
        foreach (BattlefieldButton button in allBattlefieldButtons)
        {
            if (button != null && button.gameObject != null)
            {
                int buttonLevel = button.GetLevelNumber();
                bool shouldShow = buttonLevel == levelNumber;
                bool currentlyActive = button.gameObject.activeSelf;
                Debug.Log($"[LevelManager] BattlefieldButton '{button.gameObject.name}' - level: {buttonLevel}, shouldShow: {shouldShow}, currentlyActive: {currentlyActive}");

                if (shouldShow != currentlyActive)
                {
                    button.gameObject.SetActive(shouldShow);
                    if (shouldShow)
                    {
                        buttonsShown++;
                        Debug.Log($"[LevelManager] ✓ SHOWED BattlefieldButton '{button.gameObject.name}' (level {buttonLevel})");
                    }
                    else
                    {
                        buttonsHidden++;
                        Debug.Log($"[LevelManager] ✓ HID BattlefieldButton '{button.gameObject.name}' (level {buttonLevel})");
                    }
                }
                else
                {
                    Debug.Log($"[LevelManager] - No change needed for '{button.gameObject.name}' (already {currentlyActive})");
                }
            }
            else if (button == null)
            {
                Debug.LogWarning("[LevelManager] Found null BattlefieldButton in list");
            }
            else
            {
                Debug.LogWarning($"[LevelManager] BattlefieldButton has null gameObject");
            }
        }

        Debug.Log($"[LevelManager] Summary: Shown {buttonsShown} buttons, Hidden {buttonsHidden} buttons for level {levelNumber}");
        Debug.Log($"=== LevelManager.SelectLevel({levelNumber}) END ===");
    }

    private void RefreshButtonLists()
    {
        Debug.Log("[LevelManager] RefreshButtonLists() - Re-finding all buttons...");

        int beforeCount = allBattlefieldButtons.Count;

        // Re-find all buttons to catch any that might have been created dynamically
        BattlefieldButton[] allButtons = FindObjectsByType<BattlefieldButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"[LevelManager] Found {allButtons.Length} BattlefieldButtons during refresh");
        allBattlefieldButtons.Clear();
        foreach (BattlefieldButton button in allButtons)
        {
            if (button != null && !allBattlefieldButtons.Contains(button))
            {
                allBattlefieldButtons.Add(button);
                Debug.Log($"[LevelManager] Refresh: Added BattlefieldButton '{button.gameObject.name}' (level {button.GetLevelNumber()})");
            }
        }

        int afterCount = allBattlefieldButtons.Count;
        if (afterCount != beforeCount)
        {
            Debug.Log($"[LevelManager] Button count changed: {beforeCount} -> {afterCount}");
        }

        LevelSelectionButton[] allLevelButtons = FindObjectsByType<LevelSelectionButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        levelSelectionButtons.Clear();
        foreach (LevelSelectionButton button in allLevelButtons)
        {
            if (button != null && !levelSelectionButtons.Contains(button))
            {
                levelSelectionButtons.Add(button);
            }
        }
    }

    public void RegisterLevelSelectionButton(LevelSelectionButton button)
    {
        if (!levelSelectionButtons.Contains(button))
        {
            levelSelectionButtons.Add(button);
        }
    }

    public void RegisterBattlefieldButton(BattlefieldButton button)
    {
        if (button != null && !allBattlefieldButtons.Contains(button))
        {
            allBattlefieldButtons.Add(button);
            Debug.Log($"[LevelManager] ✓ Registered BattlefieldButton '{button.gameObject.name}' with level: {button.GetLevelNumber()}, active: {button.gameObject.activeSelf}");
        }
        else if (button == null)
        {
            Debug.LogWarning("[LevelManager] Attempted to register null BattlefieldButton");
        }
        else
        {
            Debug.Log($"[LevelManager] BattlefieldButton '{button.gameObject.name}' already registered");
        }
    }

    public int GetCurrentLevel()
    {
        return currentSelectedLevel;
    }
}

