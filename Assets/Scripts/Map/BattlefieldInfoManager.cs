using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/**
 * BattlefieldInfoManager - Manages battlefield selection and info display
 * Shows info when a battlefield is selected, and displays "March To War" button
 */
public class BattlefieldInfoManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI infoText; // The "Drag_Map_Text" or info text
    [SerializeField] private GameObject marchToWarButton; // The "March To War" image button GameObject (outside Canvas)

    [Header("Default Text")]
    [SerializeField] private string defaultText = "Click and drag the map around";
    [SerializeField] private float defaultFontSize = 35f; // Original font size for drag text
    [SerializeField] private TextAlignmentOptions defaultAlignment = TextAlignmentOptions.Center;

    [Header("Battlefield Info Display")]
    [SerializeField] private float battlefieldFontSize = 19f; // Smaller font for battlefield info
    [SerializeField] private TextAlignmentOptions battlefieldAlignment = TextAlignmentOptions.Left; // Left-aligned for info

    private BattlefieldButton selectedBattlefield;
    private string selectedSceneName = "";
    private float originalFontSize;
    private TextAlignmentOptions originalAlignment;

    void Start()
    {
        // Find info text if not assigned (look for "Drag_Map_Text")
        if (infoText == null)
        {
            GameObject textObj = GameObject.Find("Drag_Map_Text");
            if (textObj != null)
            {
                infoText = textObj.GetComponent<TextMeshProUGUI>();
            }
        }

        // Find marchToWarButton if not assigned (look for "MarchToWarButton" or component)
        if (marchToWarButton == null)
        {
            GameObject buttonObj = GameObject.Find("MarchToWarButton");
            if (buttonObj != null)
            {
                marchToWarButton = buttonObj;
            }
            else
            {
                // Try to find by component
                MarchToWarButton buttonScript = FindFirstObjectByType<MarchToWarButton>();
                if (buttonScript != null)
                {
                    marchToWarButton = buttonScript.gameObject;
                }
            }
        }

        // Store original font size and alignment
        if (infoText != null)
        {
            originalFontSize = infoText.fontSize;
            originalAlignment = infoText.alignment;

            // Use default values if not set
            if (defaultFontSize > 0)
            {
                originalFontSize = defaultFontSize;
            }
            if (defaultAlignment != TextAlignmentOptions.TopLeft) // Check if actually set
            {
                originalAlignment = defaultAlignment;
            }
        }

        // Hide March To War button initially
        if (marchToWarButton != null)
        {
            marchToWarButton.SetActive(false);
        }

        // Set default text
        if (infoText != null && !string.IsNullOrEmpty(defaultText))
        {
            infoText.text = defaultText;
            infoText.fontSize = originalFontSize;
            infoText.alignment = originalAlignment;
        }
    }

    /**
     * Called when a battlefield button is clicked - shows info instead of loading scene
     */
    public void SelectBattlefield(BattlefieldButton battlefield, string battlefieldName, string kingdomState, string detailedInfo, string sceneName)
    {
        selectedBattlefield = battlefield;
        selectedSceneName = sceneName;

        // Find marchToWarButton if not assigned (look for "MarchToWarButton" or component)
        if (marchToWarButton == null)
        {
            GameObject buttonObj = GameObject.Find("MarchToWarButton");
            if (buttonObj != null)
            {
                marchToWarButton = buttonObj;
            }
            else
            {
                // Try to find by component
                MarchToWarButton buttonScript = FindFirstObjectByType<MarchToWarButton>();
                if (buttonScript != null)
                {
                    marchToWarButton = buttonScript.gameObject;
                }
            }
        }

        // Update info text with smaller font and left alignment
        if (infoText != null)
        {
            // Change font size and alignment
            infoText.fontSize = battlefieldFontSize;
            infoText.alignment = battlefieldAlignment;

            // Build the info text
            string info = $"Location: \n{battlefieldName}\n{kingdomState}";

            // Add detailed info if provided
            if (!string.IsNullOrEmpty(detailedInfo))
            {
                info += $"\n\n{detailedInfo}";
            }

            infoText.text = info;
        }

        // Show March To War button
        if (marchToWarButton != null)
        {
            marchToWarButton.SetActive(true);
            Debug.Log($"BattlefieldInfoManager: March To War button activated - {marchToWarButton.name}");
        }
        else
        {
            Debug.LogWarning("BattlefieldInfoManager: March To War button not found! Make sure it's assigned or named 'MarchToWarButton'.");
        }

        Debug.Log($"BattlefieldInfoManager: Selected battlefield - {battlefieldName} ({kingdomState})");
    }

    /**
     * Called when March To War button is clicked - actually loads the battlefield
     */
    public void MarchToWar()
    {
        if (!string.IsNullOrEmpty(selectedSceneName))
        {
            Debug.Log($"BattlefieldInfoManager: Marching to war! Loading scene: {selectedSceneName}");
            SceneManager.LoadScene(selectedSceneName);
        }
        else
        {
            Debug.LogWarning("BattlefieldInfoManager: No battlefield selected!");
        }
    }

    /**
     * Clear selection (optional - if you want to deselect)
     */
    public void ClearSelection()
    {
        selectedBattlefield = null;
        selectedSceneName = "";

        if (infoText != null)
        {
            // Restore original font size and alignment
            infoText.fontSize = originalFontSize;
            infoText.alignment = originalAlignment;

            if (!string.IsNullOrEmpty(defaultText))
            {
                infoText.text = defaultText;
            }
        }

        if (marchToWarButton != null)
        {
            marchToWarButton.SetActive(false);
        }
    }
}
