using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

/**
 * Handles Unity Gaming Services initialization and username+password authentication
 */
public class AuthenticationManagerWithPassword : MonoBehaviour
{
    // Initializing the Unity Services SDK
    async void Awake()
    {
        Debug.Log("AuthenticationManagerWithPassword Awake");
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log($"Unity Services initialized! State: {UnityServices.State}");

            if (AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log($"Player is already signed in as: {AuthenticationService.Instance.PlayerId}");
            }
            else
            {
                Debug.Log("Player is not signed in yet - waiting for sign-in");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize Unity Services: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
        }
    }

    /**
     * Sign up a new user with username and password.
     * Return the success/error message.
     */
    public async Task<string> RegisterWithUsernameAndPassword(string username, string password)
    {
        try
        {
            Debug.Log($"Attempting to register user: {username}");
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log($"Registration successful! Player ID: {AuthenticationService.Instance.PlayerId}");
            return "Registration successful!";
        }
        catch (AuthenticationException ex)
        {
            return GetUserFriendlyRegisterError(ex);
        }
        catch (RequestFailedException ex)
        {
            return GetUserFriendlyRequestError(ex, "Registration");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unexpected error during registration: {ex}");
            return "Registration failed: An unexpected error occurred. Please try again.";
        }
    }

    /**
     * Sign in an existing user with username and password.
     * Return the success/error message.
     */
    public async Task<string> LoginWithUsernameAndPassword(string username, string password)
    {
        try
        {
            Debug.Log($"Attempting to login user: {username}");
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            Debug.Log($"Login successful! Player ID: {AuthenticationService.Instance.PlayerId}");
            return "Login successful!";
        }
        catch (AuthenticationException ex)
        {
            return GetUserFriendlyLoginError(ex);
        }
        catch (RequestFailedException ex)
        {
            return GetUserFriendlyRequestError(ex, "Login");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unexpected error during login: {ex}");
            return "Login failed: An unexpected error occurred. Please try again.";
        }
    }

    /**
     * Convert authentication exception to user-friendly message for registration
     */
    private string GetUserFriendlyRegisterError(AuthenticationException ex)
    {
        string errorCode = ex.ErrorCode.ToString();
        string message = ex.Message ?? "";

        // Check error code for specific cases (Unity Services uses string error codes)
        if (errorCode.Contains("InvalidParameters") || message.Contains("invalid") || message.Contains("Invalid"))
        {
            if (message.Contains("username") || message.Contains("Username"))
            {
                return "Invalid username. Username must be 3-20 characters and contain only letters, numbers, and symbols: . - @ _";
            }
            if (message.Contains("password") || message.Contains("Password"))
            {
                return "Invalid password. Password must be 8-30 characters with at least one lowercase, uppercase, number, and symbol.";
            }
            return "Invalid username or password format. Please check your input.";
        }

        if (errorCode.Contains("UsernameAlreadyExists") || errorCode.Contains("AccountAlreadyExists") ||
            message.Contains("already exists") || message.Contains("already taken") || message.Contains("duplicate"))
        {
            return "Username already exists. Please choose a different username.";
        }

        if (errorCode.Contains("InvalidSessionToken") || errorCode.Contains("Session"))
        {
            return "Session error. Please try again.";
        }

        if (errorCode.Contains("AccountAlreadyLinked") || message.Contains("already linked"))
        {
            return "This account is already linked to another user.";
        }

        // Generic authentication error
        return $"Registration failed: {GetReadableErrorMessage(message)}";
    }

    /**
     * Convert authentication exception to user-friendly message for login
     */
    private string GetUserFriendlyLoginError(AuthenticationException ex)
    {
        string errorCode = ex.ErrorCode.ToString();
        string message = ex.Message ?? "";

        // Check error code for specific cases (Unity Services uses string error codes)
        if (errorCode.Contains("InvalidCredentials") || errorCode.Contains("InvalidPassword") ||
            message.Contains("invalid credentials") || message.Contains("wrong password") || message.Contains("incorrect"))
        {
            return "Invalid username or password. Please check your credentials and try again.";
        }

        if (errorCode.Contains("AccountNotFound") || errorCode.Contains("UserNotFound") ||
            message.Contains("not found") || message.Contains("does not exist"))
        {
            return "Account not found. Please check your username or register a new account.";
        }

        if (errorCode.Contains("InvalidParameters") || message.Contains("invalid") || message.Contains("Invalid"))
        {
            return "Invalid username or password format. Please check your input.";
        }

        if (errorCode.Contains("InvalidSessionToken") || errorCode.Contains("Session") ||
            message.Contains("session expired") || message.Contains("expired"))
        {
            return "Session expired. Please try logging in again.";
        }

        if (errorCode.Contains("TooManyRequests") || errorCode.Contains("RateLimit") ||
            message.Contains("too many") || message.Contains("rate limit"))
        {
            return "Too many login attempts. Please wait a moment and try again.";
        }

        // Generic authentication error
        return $"Login failed: {GetReadableErrorMessage(message)}";
    }

    /**
     * Convert request failed exception to user-friendly message
     */
    private string GetUserFriendlyRequestError(RequestFailedException ex, string operation)
    {
        // Network errors
        if (ex.ErrorCode == 0 || ex.Message.Contains("network") || ex.Message.Contains("Network"))
        {
            return $"{operation} failed: Network error. Please check your internet connection and try again.";
        }

        if (ex.Message.Contains("timeout") || ex.Message.Contains("Timeout"))
        {
            return $"{operation} failed: Request timed out. Please try again.";
        }

        if (ex.Message.Contains("unavailable") || ex.Message.Contains("Unavailable"))
        {
            return $"{operation} failed: Service is temporarily unavailable. Please try again later.";
        }

        // Generic request error
        return $"{operation} failed: {GetReadableErrorMessage(ex.Message)}";
    }

    /**
     * Convert technical error messages to more readable format
     */
    private string GetReadableErrorMessage(string technicalMessage)
    {
        // Remove common technical prefixes
        string message = technicalMessage;
        if (message.Contains(":"))
        {
            int colonIndex = message.IndexOf(':');
            if (colonIndex > 0 && colonIndex < message.Length - 1)
            {
                message = message.Substring(colonIndex + 1).Trim();
            }
        }

        // Capitalize first letter
        if (message.Length > 0)
        {
            message = char.ToUpper(message[0]) + message.Substring(1);
        }

        return message;
    }

    /**
     * Sign out the current user
     */
    public void SignOut()
    {
        AuthenticationService.Instance.SignOut();  // this returns "void", so it cannot be awaited.
        Debug.Log("Player signed out");
    }
}

