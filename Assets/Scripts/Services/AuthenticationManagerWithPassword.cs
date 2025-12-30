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
        await UnityServices.InitializeAsync();

        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log($"Player is already signed in as: {AuthenticationService.Instance.PlayerId}");
        }
        else
        {
            Debug.Log("Player is not signed in yet - waiting for sign-in");
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
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            return ($"Register successful! Player ID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (AuthenticationException ex)
        {
            return ($"Register failed: {ex.Message}");
        }
        catch (RequestFailedException ex)
        {
            return ($"Register request failed: {ex.Message}");
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
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            return $"Login successful! Player ID: {AuthenticationService.Instance.PlayerId}";
        }
        catch (AuthenticationException ex)
        {
            return $"Login failed: {ex.Message}";
        }
        catch (RequestFailedException ex)
        {
            return $"Login request failed: {ex.Message}";
        }
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

