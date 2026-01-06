using UnityEngine;

/**
 * Guest Button - Click to join as guest
 * 
 * SETUP INSTRUCTIONS:
 * Option 1 (Recommended - Same as Login/Register buttons):
 * 1. Select your "Join as Guest" Image GameObject in the scene
 * 2. Add Component -> UI -> Button
 * 3. In the Button component's "OnClick" section, click the "+" button
 * 4. Drag the LoginUIManager GameObject from the scene into the object field
 * 5. In the dropdown, select: LoginUIManager -> OnGuestButtonClicked()
 * 
 * Option 2 (Using this script):
 * 1. Add this GuestButton component to your Image GameObject
 * 2. Make sure the Image has "Raycast Target" checked (in Image component)
 * 3. Ensure there's an EventSystem in the scene (Unity usually adds this automatically)
 * 4. Ensure the Canvas has a GraphicRaycaster component (usually added automatically)
 */
public class GuestButton : MonoBehaviour
{
    private LoginUIManager loginUIManager;

    void Start()
    {
        // Find LoginUIManager in the scene
        loginUIManager = FindFirstObjectByType<LoginUIManager>();
        if (loginUIManager == null)
        {
            Debug.LogError("GuestButton: Could not find LoginUIManager in scene!");
        }
    }

    /**
     * Public method that can be called from Unity Button's OnClick event
     * This is the recommended way - wire it up in the Inspector
     */
    public void OnGuestButtonClick()
    {
        if (loginUIManager != null)
        {
            loginUIManager.OnGuestButtonClicked();
        }
        else
        {
            Debug.LogError("GuestButton: LoginUIManager not found! Cannot handle guest login.");
        }
    }
}
