using TMPro;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Handles login/register UI and transitions to menu after successful authentication
 */
public class LoginUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TextMeshProUGUI statusText; // Optional - can be null

    [Header("Scene Settings")]
    [SerializeField] private string menuSceneName = "OpeningVideo"; // Scene after login/register (plays video then goes to map)
    [SerializeField] private string guestSceneName = "Map"; // Scene after guest login (goes directly to map, no video)

    [Header("Video Preload Settings")]
    [SerializeField] private string videoUrl = "https://genesiswarfare.github.io/intro/Thepromisedhillstrailer.mp4"; // Video URL to preload
    [SerializeField] private bool preloadVideo = true; // Enable video preloading

    [Header("Authentication")]
    [SerializeField] private AuthenticationManagerWithPassword authManager;

    private bool isGuestLogin = false; // Flag to track if we're doing a guest login

    void Start()
    {
        // Set password field to hide password (show *****)
        if (passwordInputField != null)
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Password;
            passwordInputField.ForceLabelUpdate();
        }

        // Check if already signed in
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Player already signed in, going to menu");
            GoToMenu();
        }
        else
        {
            // Listen for sign in event
            AuthenticationService.Instance.SignedIn += OnSignedIn;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (AuthenticationService.Instance != null)
        {
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
        }
    }

    void OnSignedIn()
    {
        Debug.Log("User signed in successfully!");

        // Check if this is a guest login - if so, don't transition here
        // (OnGuestButtonClicked already handles the transition)
        if (isGuestLogin)
        {
            Debug.Log("LoginUIManager: Guest login detected in OnSignedIn - transition already handled by OnGuestButtonClicked");
            isGuestLogin = false; // Reset flag
            return;
        }

        // Regular login/register - go to opening video
        GoToMenu();
    }

    public async void OnLoginButtonClicked()
    {
        if (usernameInputField == null || passwordInputField == null)
        {
            Debug.LogError("Username or Password input fields not assigned!");
            return;
        }

        string username = usernameInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetStatus("Please enter username and password", Color.red);
            return;
        }

        SetStatus("Logging in...", Color.yellow);

        if (authManager == null)
        {
            authManager = FindFirstObjectByType<AuthenticationManagerWithPassword>();
            if (authManager == null)
            {
                SetStatus("Authentication Manager not found!", Color.red);
                return;
            }
        }

        try
        {
            string message = await authManager.LoginWithUsernameAndPassword(username, password);
            bool isSuccess = message.ToLower().Contains("success");
            SetStatus(message, isSuccess ? Color.green : Color.red);

            if (isSuccess)
            {
                // Save player credentials to Player object
                Player player = Player.Instance;
                if (player != null)
                {
                    player.SetCredentials(username, password);
                }
                // Start preloading video in background before transitioning
                StartPreloadingVideo();
                // Transition to menu - use coroutine to ensure sign-in state is updated
                StartCoroutine(TransitionToMenuAfterDelay());
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during login: {ex}");
            SetStatus("Login failed: An unexpected error occurred. Please try again.", Color.red);
        }
    }

    public async void OnRegisterButtonClicked()
    {
        if (usernameInputField == null || passwordInputField == null)
        {
            Debug.LogError("Username or Password input fields not assigned!");
            return;
        }

        string username = usernameInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetStatus("Please enter username and password", Color.red);
            return;
        }

        // Validate username: 3-20 characters, letters A-Z, numbers, and symbols ., -, @, _
        if (!IsValidUsername(username))
        {
            SetStatus("Username: 3-20 chars, letters, numbers, and . - @ _ only", Color.red);
            return;
        }

        // Validate password: 8-30 chars, at least 1 lowercase, 1 uppercase, 1 number, 1 symbol
        if (!IsValidPassword(password))
        {
            SetStatus("Password: 8-30 chars, needs lowercase, uppercase, number, and symbol", Color.red);
            return;
        }

        SetStatus("Registering...", Color.yellow);

        if (authManager == null)
        {
            authManager = FindFirstObjectByType<AuthenticationManagerWithPassword>();
            if (authManager == null)
            {
                SetStatus("Authentication Manager not found!", Color.red);
                return;
            }
        }

        try
        {
            string message = await authManager.RegisterWithUsernameAndPassword(username, password);
            bool isSuccess = message.ToLower().Contains("success");
            SetStatus(message, isSuccess ? Color.green : Color.red);

            if (isSuccess)
            {
                // Save player credentials to Player object
                Player player = Player.Instance;
                if (player != null)
                {
                    player.SetCredentials(username, password);
                    // New player registration - set money to 100
                    player.SetMoney(100);
                    player.SaveMoneyNow();
                }
                // Start preloading video in background before transitioning
                StartPreloadingVideo();
                // Transition to menu - use coroutine to ensure sign-in state is updated
                StartCoroutine(TransitionToMenuAfterDelay());
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during registration: {ex}");
            SetStatus("Registration failed: An unexpected error occurred. Please try again.", Color.red);
        }
    }

    /**
     * Handle guest login - sign in anonymously and set player name to "Guest"
     * Matches the exact signature of OnLoginButtonClicked for Unity Button compatibility
     */
    public async void OnGuestButtonClicked()
    {
        SetStatus("Joining as guest...", Color.yellow);

        // Set flag to indicate this is a guest login (prevents OnSignedIn from transitioning)
        isGuestLogin = true;

        if (authManager == null)
        {
            authManager = FindFirstObjectByType<AuthenticationManagerWithPassword>();
            if (authManager == null)
            {
                Debug.LogError("Authentication Manager not found!");
                SetStatus("Authentication Manager not found!", Color.red);
                isGuestLogin = false; // Reset flag on error
                return;
            }
        }

        try
        {
            // Sign in anonymously
            string message = await authManager.SignInAnonymously();
            bool isSuccess = message.ToLower().Contains("success");
            SetStatus(message, isSuccess ? Color.green : Color.red);

            if (isSuccess)
            {
                // Set player name to "Guest" (no password for guest)
                Player player = Player.Instance;
                if (player != null)
                {
                    player.SetCredentials("Guest", "");
                    // Guest starts with 100 money
                    player.SetMoney(100);
                    player.SaveMoneyNow();
                }
                // Guest login goes directly to map (no video) - use coroutine to ensure sign-in state is updated
                StartCoroutine(TransitionToSceneAfterDelay(guestSceneName));
            }
            else
            {
                isGuestLogin = false; // Reset flag on failure
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during guest login: {ex}");
            SetStatus("Guest login failed: An unexpected error occurred. Please try again.", Color.red);
            isGuestLogin = false; // Reset flag on error
        }
    }

    void SetStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
        Debug.Log($"Status: {message}");
    }

    void GoToMenu()
    {
        if (!string.IsNullOrEmpty(menuSceneName))
        {
            Debug.Log($"LoginUIManager: Loading scene '{menuSceneName}'");
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogError("Menu scene name not set!");
        }
    }

    void GoToScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log($"LoginUIManager: Loading scene '{sceneName}'");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Scene name '{sceneName}' is empty!");
        }
    }

    /// <summary>
    /// Transition to menu after a short delay to ensure authentication state is updated
    /// This is more reliable than relying solely on the SignedIn event, especially in WebGL
    /// </summary>
    private System.Collections.IEnumerator TransitionToMenuAfterDelay()
    {
        yield return TransitionToSceneAfterDelay(menuSceneName);
    }

    /// <summary>
    /// Transition to a specific scene after a short delay to ensure authentication state is updated
    /// This is more reliable than relying solely on the SignedIn event, especially in WebGL
    /// </summary>
    private System.Collections.IEnumerator TransitionToSceneAfterDelay(string sceneName)
    {
        // Wait a frame to ensure authentication state is updated
        yield return null;

        // Verify we're actually signed in before transitioning
        int attempts = 0;
        while (!AuthenticationService.Instance.IsSignedIn && attempts < 10)
        {
            yield return new WaitForSeconds(0.1f);
            attempts++;
        }

        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log($"LoginUIManager: Authentication confirmed, transitioning to scene '{sceneName}'");
            GoToScene(sceneName);
        }
        else
        {
            Debug.LogWarning($"LoginUIManager: Authentication state not confirmed, but attempting transition to '{sceneName}' anyway");
            GoToScene(sceneName);
        }
    }

    bool IsValidUsername(string username)
    {
        // Username: 3-20 characters, only letters A-Z, numbers, and symbols ., -, @, _
        if (username.Length < 3 || username.Length > 20)
            return false;

        foreach (char c in username)
        {
            if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                  (c >= '0' && c <= '9') || c == '.' || c == '-' || c == '@' || c == '_'))
            {
                return false;
            }
        }
        return true;
    }

    bool IsValidPassword(string password)
    {
        // Password: 8-30 characters, at least 1 lowercase, 1 uppercase, 1 number, 1 symbol
        if (password.Length < 8 || password.Length > 30)
            return false;

        bool hasLower = false;
        bool hasUpper = false;
        bool hasNumber = false;
        bool hasSymbol = false;

        foreach (char c in password)
        {
            if (c >= 'a' && c <= 'z') hasLower = true;
            else if (c >= 'A' && c <= 'Z') hasUpper = true;
            else if (c >= '0' && c <= '9') hasNumber = true;
            else hasSymbol = true; // Any other character is considered a symbol
        }

        return hasLower && hasUpper && hasNumber && hasSymbol;
    }

    /**
     * Start preloading the video in the background
     * This allows the video to be ready when we transition to the video scene
     */
    void StartPreloadingVideo()
    {
        if (preloadVideo && !string.IsNullOrEmpty(videoUrl))
        {
            Debug.Log($"LoginUIManager: Starting to preload video: {videoUrl}");
            VideoPreloader.PreloadVideo(videoUrl);
        }
    }
}
