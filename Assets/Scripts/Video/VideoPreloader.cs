using UnityEngine;
using UnityEngine.Video;

/**
 * VideoPreloader - Preloads video URL in the background before transitioning to video scene
 * This allows the video to be ready when the scene loads
 */
public class VideoPreloader : MonoBehaviour
{
    private static VideoPreloader instance;
    private static bool isVideoPreloaded = false;
    private static string preloadedVideoUrl = "";

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
    }

    /**
     * Start preloading the video URL (call this from LoginUIManager after login/register)
     */
    public static void PreloadVideo(string url)
    {
        if (isVideoPreloaded && preloadedVideoUrl == url)
        {
            Debug.Log("VideoPreloader: Video URL already preloaded, skipping");
            return;
        }

        Debug.Log($"VideoPreloader: Preloading video URL: {url}");

        // Create preloader GameObject if it doesn't exist
        if (instance == null)
        {
            GameObject preloaderObj = new GameObject("VideoPreloader");
            instance = preloaderObj.AddComponent<VideoPreloader>();
            DontDestroyOnLoad(preloaderObj);
        }

        // Just store the URL - the VideoPlayerController will handle the actual loading
        preloadedVideoUrl = url;
        isVideoPreloaded = true;

        Debug.Log("VideoPreloader: Video URL stored, ready for use");
    }

    /**
     * Get the preloaded video URL (if available)
     */
    public static string GetPreloadedVideoUrl()
    {
        return preloadedVideoUrl;
    }

    /**
     * Check if video URL is preloaded
     */
    public static bool IsVideoPreloaded()
    {
        return isVideoPreloaded && !string.IsNullOrEmpty(preloadedVideoUrl);
    }

    /**
     * Clear preloaded video URL
     */
    public static void ClearPreloadedVideo()
    {
        isVideoPreloaded = false;
        preloadedVideoUrl = "";
    }
}
