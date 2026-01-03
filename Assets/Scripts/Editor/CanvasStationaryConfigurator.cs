using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/**
 * Editor tool to configure all canvases for stationary UI
 * This makes UI elements stay in the same pixel position regardless of screen size or browser zoom
 * 
 * Usage: Tools -> Configure All Canvases for Stationary UI
 */
public class CanvasStationaryConfigurator : EditorWindow
{
    [MenuItem("Tools/Configure All Canvases for Stationary UI")]
    public static void ConfigureAllCanvases()
    {
        int canvasCount = 0;
        int modifiedCount = 0;

        // Find all Canvas objects in the project
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();

        foreach (Canvas canvas in allCanvases)
        {
            // Skip prefabs that aren't in scenes (only process scene objects)
            if (!canvas.gameObject.scene.IsValid())
            {
                continue;
            }

            canvasCount++;

            // Get or add CanvasScaler
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                modifiedCount++;
            }

            // Configure for Constant Pixel Size (stationary UI)
            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ConstantPixelSize)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                scaler.scaleFactor = 1f;
                modifiedCount++;
            }

            // Mark scene as dirty if it's a scene object
            if (canvas.gameObject.scene.IsValid())
            {
                EditorUtility.SetDirty(canvas.gameObject);
            }
        }

        Debug.Log($"Canvas Stationary Configurator: Found {canvasCount} canvases, modified {modifiedCount}.");
        EditorUtility.DisplayDialog("Canvas Configuration Complete",
            $"Configured {canvasCount} canvases.\nModified {modifiedCount} canvases to use Constant Pixel Size mode.",
            "OK");
    }

    [MenuItem("Tools/Configure Active Scene Canvases for Stationary UI")]
    public static void ConfigureActiveSceneCanvases()
    {
        Canvas[] sceneCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        int modifiedCount = 0;

        foreach (Canvas canvas in sceneCanvases)
        {
            // Get or add CanvasScaler
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                modifiedCount++;
            }

            // Configure for Constant Pixel Size (stationary UI)
            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ConstantPixelSize)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                scaler.scaleFactor = 1f;
                modifiedCount++;
            }

            EditorUtility.SetDirty(canvas.gameObject);
        }

        Debug.Log($"Canvas Stationary Configurator: Modified {modifiedCount} canvases in active scene.");
        EditorUtility.DisplayDialog("Canvas Configuration Complete",
            $"Configured {sceneCanvases.Length} canvases in active scene.\nModified {modifiedCount} canvases.",
            "OK");
    }
}
