using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/**
 * VideoPlayerController - Plays video files using URL only
 * 
 * SETUP INSTRUCTIONS:
 * 
 * 1. Create a folder called "StreamingAssets" in your Assets folder (if it doesn't exist)
 * 2. Copy your MP4 file into the StreamingAssets folder
 * 3. In the Inspector, set "Video Url" to just the filename (e.g., "trailer.mp4")
 *    OR set it to a full URL (e.g., "https://example.com/video.mp4")
 * 
 * NOTE: This script uses URL-based video loading only (no VideoClip support)
 * 
 * DISPLAY SETUP FOR 2D GAMES (Recommended):
 * 
 * OPTION 1 - UI RawImage (Best for 2D, easiest to control size):
 * 1. Set Render Mode to "Render Texture"
 * 2. Create a Canvas in your scene (if you don't have one)
 * 3. Right-click Canvas → UI → Raw Image
 * 4. Resize the RawImage's RectTransform to your desired video size
 *    (e.g., Width: 800, Height: 450 for a smaller video)
 * 5. Assign the RawImage component to "Target Raw Image" in the script
 * 6. Adjust "Video Size" to match your video resolution (e.g., 1920x1080)
 * 
 * OPTION 2 - SpriteRenderer (For 2D world space):
 * 1. Set Render Mode to "Material Override"
 * 2. Create a GameObject with a SpriteRenderer component
 * 3. Assign the SpriteRenderer to "Target Renderer" in the script
 * 4. Scale the GameObject's Transform to control video size
 * 
 * OPTION 3 - Camera Fill (Full screen):
 * 1. Set Render Mode to "Camera Far Plane" or "Camera Near Plane"
 * 2. Assign your Camera to "Target Camera"
 * Note: This fills the entire camera view
 */
public class VideoPlayerController : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string videoUrl = ""; // URL or path to video file (required)
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = false;

    [Header("Display Settings")]
    [SerializeField] private VideoRenderMode renderMode = VideoRenderMode.RenderTexture; // RenderTexture recommended for 2D/UI
    [SerializeField] private Camera targetCamera; // For Camera render mode
    [SerializeField] private Renderer targetRenderer; // For Material Override mode (SpriteRenderer for 2D)
    [SerializeField] private RawImage targetRawImage; // For UI RawImage (2D - recommended, use this instead of Image)
    [SerializeField] private Vector2 videoSize = new Vector2(1920, 1080); // Size for RenderTexture mode

    [Header("Scene Loading")]
    [SerializeField] private bool loadSceneOnVideoEnd = true;
    [SerializeField] private string nextSceneName = ""; // Name of scene to load when video ends (must match Build Settings)

    [Header("Skip Settings")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private bool skipOnAnyKey = true; // Skip on any keyboard key
    [SerializeField] private bool skipOnMouseClick = true; // Skip on mouse click
    [SerializeField] private bool skipOnEscape = true; // Skip on Escape key
    [SerializeField] private bool skipOnSpace = true; // Skip on Space key

    void Start()
    {
        // Get VideoPlayer component if not assigned
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null)
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
        }

        // Setup video player
        SetupVideoPlayer();

        // Play on start if enabled
        if (playOnStart)
        {
            PlayVideo();
        }
    }

    void SetupVideoPlayer()
    {
        if (videoPlayer == null) return;

        // Always use URL-based video (works on all platforms)
        if (!string.IsNullOrEmpty(videoUrl))
        {
            // Check if it's a YouTube URL (won't work)
            if (videoUrl.Contains("youtube.com") || videoUrl.Contains("youtu.be"))
            {
                Debug.LogError("VideoPlayerController: YouTube URLs don't work with Unity VideoPlayer! You need a direct video file URL (ending in .mp4, .webm, etc.) or a file in StreamingAssets folder.");
                Debug.LogError("You must either:");
                Debug.LogError("1. Put your video file in Assets/StreamingAssets/ folder and set Video Url to just the filename (e.g., 'trailer.mp4')");
                Debug.LogError("2. Use a direct video file URL from a web server (e.g., 'https://yourserver.com/video.mp4')");
                return;
            }

            // If videoUrl doesn't start with http, assume it's in StreamingAssets
            if (!videoUrl.StartsWith("http://") && !videoUrl.StartsWith("https://"))
            {
                string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoUrl);
                videoPlayer.url = fullPath;
                Debug.Log($"VideoPlayerController: Using StreamingAssets path: {fullPath}");
            }
            else
            {
                videoPlayer.url = videoUrl;
                Debug.Log($"VideoPlayerController: Using URL: {videoUrl}");
            }
        }
        else
        {
            Debug.LogError("VideoPlayerController: No video URL specified! Set 'Video Url' to a filename in StreamingAssets (e.g., 'trailer.mp4') or a direct video file URL.");
        }

        // Set render mode
        videoPlayer.renderMode = renderMode;

        // Configure based on render mode
        switch (renderMode)
        {
            case VideoRenderMode.CameraFarPlane:
            case VideoRenderMode.CameraNearPlane:
                if (targetCamera != null)
                {
                    videoPlayer.targetCamera = targetCamera;
                }
                else
                {
                    videoPlayer.targetCamera = Camera.main;
                }
                break;

            case VideoRenderMode.MaterialOverride:
                if (targetRenderer != null)
                {
                    videoPlayer.targetMaterialRenderer = targetRenderer;
                    videoPlayer.targetMaterialProperty = "_MainTex"; // Standard shader property
                }
                break;

            case VideoRenderMode.RenderTexture:
                // Create a RenderTexture for size control (best for 2D/UI)
                RenderTexture renderTexture = new RenderTexture((int)videoSize.x, (int)videoSize.y, 0);
                videoPlayer.targetTexture = renderTexture;
                
                // If UI RawImage is assigned, display the video on it (perfect for 2D)
                if (targetRawImage != null)
                {
                    targetRawImage.texture = renderTexture;
                }
                break;
        }

        // Set loop
        videoPlayer.isLooping = loop;

        // Set audio output mode (if video has audio)
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        
        // Add AudioSource if needed
        if (videoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }

        // Add error handling
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"VideoPlayerController: Video error - {message}");
        Debug.LogError("Common issues:");
        Debug.LogError("1. Video file not found in StreamingAssets folder");
        Debug.LogError("2. Video URL is invalid or not accessible");
        Debug.LogError("3. Video format not supported (WebGL requires H.264/MP4)");
        Debug.LogError($"Current URL: {source.url}");
    }

    void OnVideoPrepared(VideoPlayer source)
    {
        Debug.Log($"VideoPlayerController: Video prepared successfully. URL: {source.url}");
        Debug.Log($"Video resolution: {source.width}x{source.height}");
    }

    public void PlayVideo()
    {
        if (videoPlayer != null)
        {
            if (string.IsNullOrEmpty(videoPlayer.url))
            {
                Debug.LogWarning("VideoPlayerController: Video URL is not set!");
                return;
            }
            videoPlayer.Play();
        }
        else
        {
            Debug.LogWarning("VideoPlayerController: VideoPlayer is not assigned!");
        }
    }

    public void PauseVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
    }

    public void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
    }

    public void RestartVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.Play();
        }
    }

    public bool IsPlaying()
    {
        return videoPlayer != null && videoPlayer.isPlaying;
    }

    void Update()
    {
        // Check for skip input
        if (allowSkip && IsPlaying())
        {
            bool shouldSkip = false;

            // Check keyboard input
            if (skipOnAnyKey && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                // Don't skip if only specific keys are enabled and this isn't one of them
                if (skipOnEscape || skipOnSpace)
                {
                    if ((skipOnEscape && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                        (skipOnSpace && Keyboard.current.spaceKey.wasPressedThisFrame))
                    {
                        shouldSkip = true;
                    }
                }
                else
                {
                    shouldSkip = true;
                }
            }

            // Check mouse click
            if (!shouldSkip && skipOnMouseClick && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                shouldSkip = true;
            }

            // Skip the video
            if (shouldSkip)
            {
                SkipVideo();
            }
        }
    }

    /// <summary>
    /// Skip the video and load the next scene immediately (video continues playing in background)
    /// </summary>
    public void SkipVideo()
    {
        if (!allowSkip)
        {
            Debug.Log("VideoPlayerController: Skipping is disabled");
            return;
        }

        Debug.Log("VideoPlayerController: Video skipped by user - loading next scene immediately");
        
        // Load next scene immediately (don't stop the video)
        if (loadSceneOnVideoEnd)
        {
            LoadNextScene();
        }
    }

    /// <summary>
    /// Skip the video and load a specific scene immediately
    /// </summary>
    /// <param name="sceneName">Name of the scene to load (must match Build Settings)</param>
    public void SkipToScene(string sceneName)
    {
        if (!allowSkip)
        {
            Debug.Log("VideoPlayerController: Skipping is disabled");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("VideoPlayerController: Scene name is empty! Cannot skip to scene.");
            return;
        }

        Debug.Log($"VideoPlayerController: Video skipped by user - loading scene '{sceneName}' immediately");
        SceneManager.LoadScene(sceneName);
    }

    // Called when video finishes playing (if not looping)
    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video finished playing");
        
        // Load next scene if enabled
        if (loadSceneOnVideoEnd)
        {
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("VideoPlayerController: nextSceneName is not set! Video ended but no scene to load.");
            return;
        }

        Debug.Log($"VideoPlayerController: Loading scene '{nextSceneName}'");
        SceneManager.LoadScene(nextSceneName);
    }

    void OnEnable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }
}
