using UnityEngine;
using System;

/**
 * Manages local storage of user credentials (username and password)
 * Uses PlayerPrefs for local storage
 */
public static class UserDataManager
{
    private const string USERNAME_KEY = "SavedUsername";
    private const string PASSWORD_KEY = "SavedPassword";

    /**
     * Save username and password locally
     */
    public static void SaveUserCredentials(string username, string password)
    {
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("UserDataManager: Cannot save empty username");
            return;
        }

        PlayerPrefs.SetString(USERNAME_KEY, username);

        // Note: In a real app, you should NEVER save passwords in plain text
        // This is for convenience only - consider hashing or using secure storage
        if (!string.IsNullOrEmpty(password))
        {
            PlayerPrefs.SetString(PASSWORD_KEY, password);
        }

        PlayerPrefs.Save();
        Debug.Log($"UserDataManager: Saved credentials for user: {username}");
    }

    /**
     * Get saved username
     */
    public static string GetSavedUsername()
    {
        return PlayerPrefs.GetString(USERNAME_KEY, "");
    }

    /**
     * Get saved password
     */
    public static string GetSavedPassword()
    {
        return PlayerPrefs.GetString(PASSWORD_KEY, "");
    }

    /**
     * Check if user credentials are saved
     */
    public static bool HasSavedCredentials()
    {
        return !string.IsNullOrEmpty(GetSavedUsername());
    }

    /**
     * Clear saved credentials (for logout)
     */
    public static void ClearSavedCredentials()
    {
        PlayerPrefs.DeleteKey(USERNAME_KEY);
        PlayerPrefs.DeleteKey(PASSWORD_KEY);
        PlayerPrefs.Save();
        Debug.Log("UserDataManager: Cleared saved credentials");
    }
}
