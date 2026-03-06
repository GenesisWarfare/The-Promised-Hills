using UnityEngine;
using UnityEngine.Advertisements;

/**
 * AdManager - Handles Unity Ads integration
 * Shows rewarded ads, interstitial ads, and banner ads
 * 
 * Setup:
 * 1. Install Unity Ads package (Window → Package Manager → Unity Registry → Unity Ads)
 * 2. Get your Game ID from https://dashboard.unity3d.com/
 * 3. Set androidGameId and iOSGameId in Inspector
 * 4. Set testMode to false for production builds
 */
public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [Header("Game IDs (Get from Unity Dashboard)")]
    [SerializeField] private string androidGameId = ""; // Get from Unity Dashboard → Operate → Advertisements
    [SerializeField] private string iOSGameId = ""; // Get from Unity Dashboard → Operate → Advertisements

    [Header("Ad Settings")]
    [SerializeField] private bool testMode = true; // Set to false for production
    [SerializeField] private string rewardedAdPlacementId = "Rewarded_Android"; // Default placement ID
    [SerializeField] private string interstitialAdPlacementId = "Interstitial_Android"; // Default placement ID
    [SerializeField] private string bannerAdPlacementId = "Banner_Android"; // Default placement ID

    [Header("Banner Settings")]
    [SerializeField] private BannerPosition bannerPosition = BannerPosition.BOTTOM_CENTER;
    [SerializeField] private bool showBannerOnStart = false;

    private string gameId;
    private bool isInitialized = false;

    public static AdManager Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Initialize Unity Ads
        InitializeAds();
    }

    /// <summary>
    /// Initialize Unity Ads with the appropriate Game ID
    /// </summary>
    private void InitializeAds()
    {
#if UNITY_ANDROID
        gameId = androidGameId;
#elif UNITY_IOS
        gameId = iOSGameId;
#else
        gameId = androidGameId; // Editor uses Android
#endif

        if (string.IsNullOrEmpty(gameId))
        {
            Debug.LogError("AdManager: Game ID is not set! Please set androidGameId or iOSGameId in the Inspector.");
            return;
        }

        Debug.Log($"AdManager: Initializing Unity Ads with Game ID: {gameId}");
        Advertisement.Initialize(gameId, testMode, this);
    }

    /// <summary>
    /// Show a rewarded ad (user gets reward after watching)
    /// </summary>
    public void ShowRewardedAd()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("AdManager: Ads not initialized yet. Cannot show rewarded ad.");
            return;
        }

        if (Advertisement.isInitialized)
        {
            Advertisement.Load(rewardedAdPlacementId, this);
        }
        else
        {
            Debug.LogWarning("AdManager: Unity Ads not initialized!");
        }
    }

    /// <summary>
    /// Show an interstitial ad (full-screen ad between levels)
    /// </summary>
    public void ShowInterstitialAd()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("AdManager: Ads not initialized yet. Cannot show interstitial ad.");
            return;
        }

        if (Advertisement.isInitialized)
        {
            Advertisement.Load(interstitialAdPlacementId, this);
        }
        else
        {
            Debug.LogWarning("AdManager: Unity Ads not initialized!");
        }
    }

    /// <summary>
    /// Show a banner ad (small ad at top/bottom of screen)
    /// </summary>
    public void ShowBannerAd()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("AdManager: Ads not initialized yet. Cannot show banner ad.");
            return;
        }

        if (Advertisement.isInitialized)
        {
            Advertisement.Banner.Show(bannerAdPlacementId);
        }
    }

    /// <summary>
    /// Hide the banner ad
    /// </summary>
    public void HideBannerAd()
    {
        Advertisement.Banner.Hide();
    }

    // ========== Unity Ads Initialization Callbacks ==========

    public void OnInitializationComplete()
    {
        Debug.Log("AdManager: Unity Ads initialization complete!");
        isInitialized = true;

        // Set banner position
        Advertisement.Banner.SetPosition(bannerPosition);

        // Show banner on start if enabled
        if (showBannerOnStart)
        {
            ShowBannerAd();
        }
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"AdManager: Unity Ads initialization failed: {error} - {message}");
        isInitialized = false;
    }

    // ========== Unity Ads Load Callbacks ==========

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log($"AdManager: Ad loaded for placement: {placementId}");
        
        // Show the ad once it's loaded
        if (placementId == rewardedAdPlacementId || placementId == interstitialAdPlacementId)
        {
            Advertisement.Show(placementId, this);
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"AdManager: Ad failed to load for placement {placementId}: {error} - {message}");
    }

    // ========== Unity Ads Show Callbacks ==========

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"AdManager: Ad show failed for placement {placementId}: {error} - {message}");
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log($"AdManager: Ad started showing for placement: {placementId}");
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log($"AdManager: Ad clicked for placement: {placementId}");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"AdManager: Ad completed for placement: {placementId}, completion state: {showCompletionState}");

        // Give reward if user watched the full rewarded ad
        if (placementId == rewardedAdPlacementId && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            OnRewardedAdCompleted();
        }
    }

    /// <summary>
    /// Called when user successfully watches a rewarded ad
    /// Override this or subscribe to events to give rewards
    /// </summary>
    private void OnRewardedAdCompleted()
    {
        Debug.Log("AdManager: Rewarded ad completed - giving reward!");

        // Give money reward (example)
        Player player = Player.Instance;
        if (player != null)
        {
            player.AddMoney(50); // Give 50 money for watching ad
            Debug.Log("AdManager: Gave 50 money for watching rewarded ad!");
        }

        // You can add more rewards here (extra lives, power-ups, etc.)
    }
}
