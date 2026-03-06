using UnityEngine;
using UnityEngine.UI;

/**
 * AdButton - Simple button to show ads
 * Attach this to a UI Button to show rewarded/interstitial ads when clicked
 */
public class AdButton : MonoBehaviour
{
    [Header("Ad Type")]
    [SerializeField] private AdType adType = AdType.Rewarded;

    private Button button;

    public enum AdType
    {
        Rewarded,      // User gets reward after watching
        Interstitial,  // Full-screen ad (no reward)
        Banner         // Small banner ad
    }

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogWarning("AdButton: No Button component found!");
        }
    }

    void OnButtonClicked()
    {
        if (AdManager.Instance == null)
        {
            Debug.LogWarning("AdButton: AdManager not found in scene!");
            return;
        }

        switch (adType)
        {
            case AdType.Rewarded:
                AdManager.Instance.ShowRewardedAd();
                break;
            case AdType.Interstitial:
                AdManager.Instance.ShowInterstitialAd();
                break;
            case AdType.Banner:
                AdManager.Instance.ShowBannerAd();
                break;
        }
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}
