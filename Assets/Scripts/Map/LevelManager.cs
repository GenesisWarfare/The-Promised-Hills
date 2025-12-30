using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    private int currentSelectedLevel = 1;
    private List<BattlefieldButton> allBattlefieldButtons = new List<BattlefieldButton>();
    private List<LevelSelectionButton> levelSelectionButtons = new List<LevelSelectionButton>();

    void Awake()
    {
        // Find all battlefield buttons - make sure they're all active when we find them
        BattlefieldButton[] allButtons = FindObjectsByType<BattlefieldButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"Found {allButtons.Length} BattlefieldButtons in Awake");
        foreach (BattlefieldButton button in allButtons)
        {
            if (button != null)
            {
                allBattlefieldButtons.Add(button);
                Debug.Log($"Added BattlefieldButton with level: {button.GetLevelNumber()}");
            }
        }

        // Find all level selection buttons
        LevelSelectionButton[] allLevelButtons = FindObjectsByType<LevelSelectionButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"Found {allLevelButtons.Length} LevelSelectionButtons in Awake");
        foreach (LevelSelectionButton button in allLevelButtons)
        {
            if (button != null)
            {
                levelSelectionButtons.Add(button);
                Debug.Log($"Added LevelSelectionButton with level: {button.GetLevelNumber()}");
            }
        }
    }

    void Start()
    {
        // Hide all battlefield buttons first
        foreach (BattlefieldButton button in allBattlefieldButtons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(false);
            }
        }
        
        // Initialize with level 1 selected (this will show the correct buttons)
        SelectLevel(1);
    }

    public void SelectLevel(int levelNumber)
    {
        currentSelectedLevel = levelNumber;
        
        Debug.Log($"SelectLevel called with levelNumber: {levelNumber}");
        Debug.Log($"Total battlefield buttons found: {allBattlefieldButtons.Count}");
        
        // Show/hide battlefield buttons based on level
        foreach (BattlefieldButton button in allBattlefieldButtons)
        {
            if (button != null)
            {
                int buttonLevel = button.GetLevelNumber();
                bool shouldShow = buttonLevel == levelNumber;
                Debug.Log($"BattlefieldButton level: {buttonLevel}, shouldShow: {shouldShow}");
                button.gameObject.SetActive(shouldShow);
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

    public int GetCurrentLevel()
    {
        return currentSelectedLevel;
    }
}

