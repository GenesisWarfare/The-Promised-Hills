using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/**
 * MobileInputHelper - Makes mouse input work on mobile (touch) and WebGL
 * Use this instead of Mouse.current for cross-platform compatibility
 */
public static class MobileInputHelper
{
    /// <summary>
    /// Returns true if mouse/touch was pressed this frame
    /// </summary>
    public static bool WasPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        // Check mouse first (desktop)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;
        
        // Check touch (mobile) - Began phase means pressed this frame
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                    return true;
            }
        }
        return false;
#else
        return Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
#endif
    }

    /// <summary>
    /// Returns current mouse/touch position in screen coordinates
    /// </summary>
    public static Vector2 GetPosition()
    {
#if ENABLE_INPUT_SYSTEM
        // Try mouse first (desktop)
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
        
        // Try touch (mobile)
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            return Touchscreen.current.touches[0].position.ReadValue();
        
        return Vector2.zero;
#else
        if (Input.touchCount > 0)
            return Input.GetTouch(0).position;
        return Input.mousePosition;
#endif
    }

    /// <summary>
    /// Returns true if mouse/touch is currently held down
    /// </summary>
    public static bool IsPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            return true;
        
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                var phase = touch.phase.ReadValue();
                // Touch is pressed if it's Began, Moved, or Stationary (not Ended or Canceled)
                if (phase == UnityEngine.InputSystem.TouchPhase.Began ||
                    phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                    phase == UnityEngine.InputSystem.TouchPhase.Stationary)
                    return true;
            }
        }
        return false;
#else
        return Input.GetMouseButton(0) || Input.touchCount > 0;
#endif
    }

    /// <summary>
    /// Returns true if mouse/touch was released this frame
    /// </summary>
    public static bool WasReleasedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            return true;
        
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                // Check if touch phase is Ended (released this frame)
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
                    return true;
            }
        }
        return false;
#else
        return Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);
#endif
    }
}
