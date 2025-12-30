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
    [SerializeField] private string menuSceneName = "Menu";
    
    [Header("Authentication")]
    [SerializeField] private AuthenticationManagerWithPassword authManager;
    
    void Start()
    {
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
        
        string message = await authManager.LoginWithUsernameAndPassword(username, password);
        SetStatus(message, message.Contains("success") ? Color.green : Color.red);
        
        if (message.Contains("success"))
        {
            // OnSignedIn will be called automatically via event
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
        
        string message = await authManager.RegisterWithUsernameAndPassword(username, password);
        SetStatus(message, message.Contains("success") ? Color.green : Color.red);
        
        if (message.Contains("success"))
        {
            // OnSignedIn will be called automatically via event
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
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogError("Menu scene name not set!");
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
}
