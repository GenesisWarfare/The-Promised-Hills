using UnityEngine;
using UnityEngine.UI;
using TMPro;

/**
 * Health bar UI for bases
 * Displays health bar using sprites (8 sprites for different health percentages) and text for player and enemy bases
 */
public class BaseHealthBar : MonoBehaviour
{

    [Header("Base Reference")]
    [SerializeField] private GameBase targetBase; // Assign manually, or leave empty and set Is Player Base below
    [SerializeField] private bool isPlayerBase = true; // If targetBase is not assigned, this determines which base to get from BaseManager

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Health Bar Sprites")]
    [Tooltip("Array of 8 sprites representing health from 0% to 100%. Index 0 = empty, Index 7 = full")]
    [SerializeField] private Sprite[] healthBarSprites = new Sprite[8];

    [Header("Settings")]
    [SerializeField] private bool showHealthText = true;

    void Start()
    {
        // Validate sprite array
        if (healthBarSprites == null || healthBarSprites.Length != 8)
        {
            Debug.LogWarning($"BaseHealthBar on {gameObject.name}: Health bar sprites array should have exactly 8 sprites! Current count: {(healthBarSprites?.Length ?? 0)}");
        }

        // Try to find base - may need to wait a frame for BaseManager to create bases
        StartCoroutine(FindBaseCoroutine());
    }

    private System.Collections.IEnumerator FindBaseCoroutine()
    {
        // Wait a frame to ensure BaseManager has created bases
        yield return null;

        // If base not manually assigned, get it from BaseManager
        if (targetBase == null)
        {
            BaseManager baseManager = FindFirstObjectByType<BaseManager>();
            if (baseManager != null)
            {
                if (isPlayerBase)
                {
                    targetBase = baseManager.GetPlayerBase();
                }
                else
                {
                    targetBase = baseManager.GetEnemyBase();
                }
            }
        }

        // Subscribe to health changes
        if (targetBase != null)
        {
            targetBase.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(targetBase.Health, targetBase.maxHealth);
            Debug.Log($"[BaseHealthBar] {gameObject.name}: Connected to base with health {targetBase.Health}/{targetBase.maxHealth}. Sprites count: {healthBarSprites?.Length ?? 0}");
        }
        else
        {
            Debug.LogWarning($"[BaseHealthBar] {gameObject.name}: No target base found! Make sure BaseManager is in the scene or assign targetBase manually.");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (targetBase != null)
        {
            targetBase.OnHealthChanged -= UpdateHealthBar;
        }
    }

    void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (maxHealth <= 0)
        {
            Debug.LogWarning($"[BaseHealthBar] {gameObject.name}: maxHealth is 0 or less!");
            return;
        }

        float healthPercent = (float)currentHealth / maxHealth;

        // Get Image component from this GameObject
        Image healthBarImage = GetComponent<Image>();
        if (healthBarImage == null)
        {
            return; // No Image component, skip update
        }

        // Update health bar sprite based on health percentage
        if (healthBarSprites == null || healthBarSprites.Length != 8)
        {
            Debug.LogWarning($"[BaseHealthBar] {gameObject.name}: Health bar sprites array is null or doesn't have 8 sprites! Current: {(healthBarSprites?.Length ?? 0)}");
        }
        else
        {
            // Map health percentage to sprite index (0-7)
            // 0% = index 0, 12.5% = index 1, 25% = index 2, ..., 87.5% = index 7, 100% = index 7
            int spriteIndex = Mathf.Clamp(Mathf.FloorToInt(healthPercent * 8f), 0, 7);
            
            // If health is exactly 0, use the first sprite (empty)
            if (currentHealth <= 0)
            {
                spriteIndex = 0;
            }
            // If health is full (100%), use the last sprite
            else if (healthPercent >= 1f)
            {
                spriteIndex = 7;
            }

            // Set the sprite if it exists
            if (spriteIndex < healthBarSprites.Length && healthBarSprites[spriteIndex] != null)
            {
                healthBarImage.sprite = healthBarSprites[spriteIndex];
                Debug.Log($"[BaseHealthBar] {gameObject.name}: Updated sprite to index {spriteIndex} (health: {currentHealth}/{maxHealth}, percent: {healthPercent:F2})");
            }
            else
            {
                Debug.LogWarning($"[BaseHealthBar] {gameObject.name}: Health bar sprite at index {spriteIndex} is null!");
            }
        }

        // Update text
        if (healthText != null && showHealthText)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
        else if (healthText == null && showHealthText)
        {
            Debug.LogWarning($"[BaseHealthBar] {gameObject.name}: Health text is null but showHealthText is true!");
        }
    }
}
