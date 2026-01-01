using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    private int currentSelectedLevel = 1;
    private List<BattlefieldButton> allBattlefieldButtons = new List<BattlefieldButton>();
    private List<LevelSelectionButton> levelSelectionButtons = new List<LevelSelectionButton>();

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

        // Initialize with level 1 selected (this will show the correct buttons)
        Debug.Log("[LevelManager] Initializing with level 1 selected...");
        SelectLevel(1);

        Debug.Log("=== LevelManager.Start() END ===");
    }

    public void SelectLevel(int levelNumber)
    {
        Debug.Log($"=== LevelManager.SelectLevel({levelNumber}) START ===");
        currentSelectedLevel = levelNumber;

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

