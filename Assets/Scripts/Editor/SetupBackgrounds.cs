using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/**
 * Sets up backgrounds for Level 3 and Level 4 battlefield scenes
 * Creates a Background GameObject with SpriteRenderer positioned correctly
 */
public class SetupBackgrounds : EditorWindow
{
    [MenuItem("Tools/Setup Level 3 & 4 Backgrounds")]
    public static void SetupLevel3And4Backgrounds()
    {
        // Level 3 background
        SetupLevelBackground("Level_3", "Background_3.png", "Background_3");

        // Level 4 background
        SetupLevelBackground("Level_4", "Background_4.png", "Background_4");

        EditorUtility.DisplayDialog("Background Setup", 
            "Background setup complete!\n\n" +
            "Level 3: Background_3.png\n" +
            "Level 4: Background_4.png\n\n" +
            "Check the scenes to verify backgrounds are positioned correctly.",
            "OK");
    }

    static void SetupLevelBackground(string levelFolder, string spriteName, string backgroundName)
    {
        // Find all Level 3 or Level 4 battlefield scenes
        string[] sceneGuids = AssetDatabase.FindAssets("Battlefield t:Scene", new[] { $"Assets/Scenes/{levelFolder}" });

        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            EditorSceneManager.OpenScene(scenePath);

            // Check if background already exists
            GameObject existingBg = GameObject.Find(backgroundName);
            if (existingBg != null)
            {
                Debug.Log($"[SetupBackgrounds] {backgroundName} already exists in {scenePath}");
                continue;
            }

            // Find the sprite
            string[] spriteGuids = AssetDatabase.FindAssets($"{spriteName} t:Sprite", new[] { "Assets/Sprites" });
            if (spriteGuids.Length == 0)
            {
                Debug.LogWarning($"[SetupBackgrounds] Could not find sprite: {spriteName}");
                continue;
            }

            string spritePath = AssetDatabase.GUIDToAssetPath(spriteGuids[0]);
            Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            
            if (backgroundSprite == null)
            {
                Debug.LogWarning($"[SetupBackgrounds] Could not load sprite at: {spritePath}");
                continue;
            }

            // Create background GameObject
            GameObject background = new GameObject(backgroundName);
            SpriteRenderer sr = background.AddComponent<SpriteRenderer>();
            sr.sprite = backgroundSprite;
            sr.sortingOrder = -10; // Behind everything

            // Position at origin, scale to fit camera view
            background.transform.position = Vector3.zero;

            // Get camera to calculate proper scale
            Camera mainCam = Camera.main;
            if (mainCam != null && mainCam.orthographic)
            {
                float orthoSize = mainCam.orthographicSize;
                float aspect = mainCam.aspect;
                float cameraWidth = orthoSize * 2f * aspect;
                float cameraHeight = orthoSize * 2f;

                // Scale background to cover camera view (with some padding)
                Bounds spriteBounds = backgroundSprite.bounds;
                float scaleX = (cameraWidth * 1.2f) / spriteBounds.size.x;
                float scaleY = (cameraHeight * 1.2f) / spriteBounds.size.y;
                float scale = Mathf.Max(scaleX, scaleY);

                background.transform.localScale = new Vector3(scale, scale, 1f);
            }

            EditorUtility.SetDirty(background);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            
            Debug.Log($"[SetupBackgrounds] Created {backgroundName} in {scenePath}");
        }
    }
}
