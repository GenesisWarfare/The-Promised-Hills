using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * Scene Loader Button - Loads a scene when clicked
 * 
 * SETUP INSTRUCTIONS:
 * 1. Select your button Image GameObject in the scene
 * 2. Add Component -> UI -> Button
 * 3. In the Button component's "OnClick" section, click the "+" button
 * 4. Drag this GameObject (with SceneLoaderButton script) into the object field
 * 5. In the dropdown, select: SceneLoaderButton -> LoadScene()
 * 6. In the Inspector, set the "Scene Name" field to the name of the scene you want to load
 *    (e.g., "Map" for the map scene, "Tutorial" for tutorial scene)
 * 
 * NOTE: Make sure the scene name matches exactly what's in Build Settings!
 */
public class SceneLoaderButton : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneName = ""; // Name of the scene to load (must match Build Settings)

    [Header("Button Delay Settings")]
    [Tooltip("Delay in seconds before the button becomes clickable after scene loads")]
    [SerializeField] private float enableDelay = 2f; // Default 2 seconds

    private Button button;
    private bool isEnabled = false;

    void Start()
    {
        // Get the Button component
        button = GetComponent<Button>();

        // Disable button initially
        if (button != null)
        {
            button.interactable = false;
        }

        // Start coroutine to enable button after delay
        StartCoroutine(EnableButtonAfterDelay());
    }

    private System.Collections.IEnumerator EnableButtonAfterDelay()
    {
        yield return new WaitForSeconds(enableDelay);

        // Enable button after delay
        if (button != null)
        {
            button.interactable = true;
            isEnabled = true;
        }
    }

    /**
     * Public method that can be called from Unity Button's OnClick event
     * This is the recommended way - wire it up in the Inspector
     */
    public void LoadScene()
    {
        // Don't allow loading if button hasn't been enabled yet
        if (!isEnabled)
        {
            Debug.Log($"SceneLoaderButton: Button not enabled yet. Please wait {enableDelay} seconds after scene loads.");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"SceneLoaderButton on '{gameObject.name}': Scene name is not set! Please set it in the Inspector.");
            return;
        }

        Debug.Log($"SceneLoaderButton: Loading scene '{sceneName}'");
        SceneManager.LoadScene(sceneName);
    }

    /**
     * Alternative method if you want to load a scene by name passed as parameter
     * (useful if you want to reuse the same button for different scenes)
     */
    public void LoadSceneByName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError($"SceneLoaderButton on '{gameObject.name}': Scene name is empty!");
            return;
        }

        Debug.Log($"SceneLoaderButton: Loading scene '{name}'");
        SceneManager.LoadScene(name);
    }
}
