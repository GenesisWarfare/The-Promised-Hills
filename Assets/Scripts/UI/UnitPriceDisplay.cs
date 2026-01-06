using UnityEngine;
using TMPro;

/**
 * Displays the unit price from the unit prefab's Cost property
 * Attach this to a TextMeshProUGUI component that should show the unit price
 */
public class UnitPriceDisplay : MonoBehaviour
{
    [Header("Unit Prefab")]
    [SerializeField] private GameObject unitPrefab; // The unit prefab to get the price from

    private TextMeshProUGUI priceText;

    void Start()
    {
        priceText = GetComponent<TextMeshProUGUI>();
        if (priceText == null)
        {
            Debug.LogWarning($"UnitPriceDisplay: No TextMeshProUGUI component found on {gameObject.name}!");
            return;
        }

        UpdatePrice();
    }

    void UpdatePrice()
    {
        if (unitPrefab == null)
        {
            Debug.LogWarning($"UnitPriceDisplay: No unit prefab assigned to {gameObject.name}!");
            if (priceText != null)
            {
                priceText.text = "0";
            }
            return;
        }

        Unit unitComponent = unitPrefab.GetComponent<Unit>();
        if (unitComponent != null)
        {
            int cost = unitComponent.Cost;
            if (priceText != null)
            {
                priceText.text = cost.ToString();
            }
        }
        else
        {
            Debug.LogWarning($"UnitPriceDisplay: Unit prefab {unitPrefab.name} doesn't have a Unit component!");
            if (priceText != null)
            {
                priceText.text = "0";
            }
        }
    }

    /// <summary>
    /// Call this if you change the unit prefab at runtime
    /// </summary>
    public void SetUnitPrefab(GameObject prefab)
    {
        unitPrefab = prefab;
        UpdatePrice();
    }
}
