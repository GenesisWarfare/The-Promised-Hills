using UnityEngine;
using UnityEngine.UI;
using TMPro;

/**
 * Health bar UI for bases
 * Displays health bar and text for player and enemy bases
 */
public class BaseHealthBar : MonoBehaviour
{
    [Header("Base Reference")]
    [SerializeField] private GameBase targetBase;

    [Header("UI Elements")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image fillImage;

    [Header("Colors")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color damagedColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;

    [Header("Settings")]
    [SerializeField] private bool showHealthText = true;
    [SerializeField] private float colorChangeThreshold = 0.3f; // Change to yellow at 30%
    [SerializeField] private float criticalThreshold = 0.15f; // Change to red at 15%

    void Start()
    {
        // Setup health bar first
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
            healthSlider.interactable = false; // Make slider non-interactive (display only)
        }

        // Try to find base - may need to wait a frame for BaseManager to create bases
        StartCoroutine(FindBaseCoroutine());
    }

    private System.Collections.IEnumerator FindBaseCoroutine()
    {
        // Wait a frame to ensure BaseManager has created bases
        yield return null;

        // Find base if not assigned
        if (targetBase == null)
        {
            BaseManager baseManager = FindFirstObjectByType<BaseManager>();
            if (baseManager != null)
            {
                // Try to find the appropriate base based on this object's name or tag
                if (gameObject.name.Contains("Player") || gameObject.CompareTag("Player"))
                {
                    targetBase = baseManager.GetPlayerBase();
                }
                else if (gameObject.name.Contains("Enemy") || gameObject.CompareTag("Enemy"))
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
            Debug.Log($"{gameObject.name}: Connected to base with health {targetBase.Health}/{targetBase.maxHealth}");
        }
        else
        {
            Debug.LogWarning($"BaseHealthBar on {gameObject.name}: No target base found! Make sure BaseManager is in the scene.");
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
        if (maxHealth <= 0) return;

        float healthPercent = (float)currentHealth / maxHealth;

        // Update slider
        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;
        }

        // Update text
        if (healthText != null && showHealthText)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }

        // Update color based on health percentage
        if (fillImage != null)
        {
            if (healthPercent <= criticalThreshold)
            {
                fillImage.color = criticalColor;
            }
            else if (healthPercent <= colorChangeThreshold)
            {
                fillImage.color = damagedColor;
            }
            else
            {
                fillImage.color = healthyColor;
            }
        }
    }
}
