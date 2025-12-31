using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/**
 * Fixes Medieval King Pack 2 sprite sheet slicing
 * Re-slices all sprite sheets with better detection
 */
public class FixMedievalKingAnimations : EditorWindow
{
    private int pixelsPerUnit = 100;
    private float frameRate = 12f;

    [MenuItem("Tools/Fix Medieval King Animations", false, 2)]
    public static void ShowWindow()
    {
        GetWindow<FixMedievalKingAnimations>("Fix Medieval King Animations");
    }

    void OnGUI()
    {
        GUILayout.Label("Fix Medieval King Pack 2 Animations", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This will re-slice all Medieval King Pack 2 sprite sheets with better detection.\n" +
            "It will fix incorrectly sliced animations.",
            MessageType.Info
        );

        EditorGUILayout.Space();
        pixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit:", pixelsPerUnit);
        frameRate = EditorGUILayout.FloatField("Frame Rate:", frameRate);

        EditorGUILayout.Space();

        if (GUILayout.Button("Fix All Animations", GUILayout.Height(30)))
        {
            FixAllAnimations();
        }
    }

    void FixAllAnimations()
    {
        string folderPath = "Assets/Sprites/Medieval King Pack 2";
        string[] pngFiles = Directory.GetFiles(folderPath, "*.png", SearchOption.TopDirectoryOnly);

        int fixedCount = 0;
        int failed = 0;
        List<string> errors = new List<string>();

        foreach (string filePath in pngFiles)
        {
            string relativePath = filePath.Replace('\\', '/');
            if (!relativePath.StartsWith("Assets/"))
            {
                if (filePath.StartsWith(Application.dataPath))
                {
                    relativePath = "Assets" + filePath.Substring(Application.dataPath.Length).Replace('\\', '/');
                }
                else
                {
                    continue;
                }
            }

            try
            {
                ReSliceSpriteSheet(relativePath);
                fixedCount++;
            }
            catch (System.Exception e)
            {
                errors.Add($"{Path.GetFileName(filePath)}: {e.Message}");
                failed++;
            }
        }

        AssetDatabase.Refresh();

        string message = $"Fixed {fixedCount} sprite sheets.";
        if (failed > 0)
        {
            message += $"\n{failed} failed:\n" + string.Join("\n", errors);
        }

        EditorUtility.DisplayDialog("Fix Complete", message, "OK");
    }

    void ReSliceSpriteSheet(string texturePath)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture == null) return;

        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer == null) return;

        // Set import settings
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        // Better grid detection for Medieval King sprites
        int width = texture.width;
        int height = texture.height;
        Vector2Int gridSize;

        // Medieval King sprites are typically horizontal strips
        // Try to detect frame count more accurately
        if (width > height)
        {
            // Horizontal strip - calculate frames more carefully
            int frameHeight = height;
            
            // Try different frame counts
            int[] possibleFrames = { 4, 6, 8, 10, 12, 16 };
            int bestFit = 8; // Default
            
            foreach (int frames in possibleFrames)
            {
                int frameWidth = width / frames;
                // Check if this divides evenly and sprite width is reasonable
                if (width % frames == 0 && frameWidth > frameHeight * 0.5f && frameWidth < frameHeight * 2f)
                {
                    bestFit = frames;
                    break;
                }
            }
            
            gridSize = new Vector2Int(bestFit, 1);
        }
        else if (height > width)
        {
            // Vertical strip
            int frameWidth = width;
            int[] possibleFrames = { 4, 6, 8, 10, 12, 16 };
            int bestFit = 8;
            
            foreach (int frames in possibleFrames)
            {
                int frameHeight = height / frames;
                if (height % frames == 0 && frameHeight > frameWidth * 0.5f && frameHeight < frameWidth * 2f)
                {
                    bestFit = frames;
                    break;
                }
            }
            
            gridSize = new Vector2Int(1, bestFit);
        }
        else
        {
            // Square - try grid
            if (width % 8 == 0 && height % 8 == 0) gridSize = new Vector2Int(8, 8);
            else if (width % 4 == 0 && height % 4 == 0) gridSize = new Vector2Int(4, 4);
            else gridSize = new Vector2Int(8, 1);
        }

        // Create sprite rects
        int spriteWidth = texture.width / gridSize.x;
        int spriteHeight = texture.height / gridSize.y;
        List<SpriteMetaData> sprites = new List<SpriteMetaData>();
        string animName = Path.GetFileNameWithoutExtension(texturePath);

        for (int row = 0; row < gridSize.y; row++)
        {
            for (int col = 0; col < gridSize.x; col++)
            {
                SpriteMetaData sprite = new SpriteMetaData
                {
                    name = $"{animName}_{row * gridSize.x + col}",
                    rect = new Rect(col * spriteWidth, texture.height - (row + 1) * spriteHeight, spriteWidth, spriteHeight),
                    pivot = new Vector2(0.5f, 0.5f),
                    alignment = (int)SpriteAlignment.Center
                };
                sprites.Add(sprite);
            }
        }

        importer.spritesheet = sprites.ToArray();
        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        // Recreate animation clip
        Object[] spriteObjects = AssetDatabase.LoadAllAssetsAtPath(texturePath);
        List<Sprite> spriteList = new List<Sprite>();
        foreach (Object obj in spriteObjects)
        {
            if (obj is Sprite sprite)
                spriteList.Add(sprite);
        }

        if (spriteList.Count == 0) return;

        // Sort sprites by name
        spriteList.Sort((a, b) => string.Compare(a.name, b.name));

        // Recreate animation clip
        string folderPath = Path.GetDirectoryName(texturePath);
        string animPath = Path.Combine(folderPath, $"{animName}.anim");

        AnimationClip clip = new AnimationClip();
        clip.frameRate = frameRate;

        EditorCurveBinding binding = new EditorCurveBinding
        {
            path = "",
            type = typeof(SpriteRenderer),
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[spriteList.Count];
        for (int i = 0; i < spriteList.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / frameRate,
                value = spriteList[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // Delete old clip if exists
        if (File.Exists(animPath))
        {
            AssetDatabase.DeleteAsset(animPath);
        }

        AssetDatabase.CreateAsset(clip, animPath);
    }
}

