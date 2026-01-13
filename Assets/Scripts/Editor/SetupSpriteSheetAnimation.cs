using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/**
 * Simple tool to slice sprite sheet and create animation clip
 * 
 * Usage:
 * 1. Select a sprite sheet PNG in Project window
 * 2. Assets -> Setup Sprite Sheet Animation
 */
public class SetupSpriteSheetAnimation : EditorWindow
{
    private Object selectedSprite;
    private int pixelsPerUnit = 100;
    private float frameRate = 12f;
    private Vector2Int gridSize = new Vector2Int(8, 1);
    private bool autoDetectGrid = true;

    [MenuItem("Assets/Setup Sprite Sheet Animation", false, 1)]
    public static void ShowWindow()
    {
        GetWindow<SetupSpriteSheetAnimation>("Setup Sprite Sheet Animation");
    }

    void OnGUI()
    {
        GUILayout.Label("Setup Sprite Sheet Animation", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        selectedSprite = EditorGUILayout.ObjectField("Sprite Sheet:", selectedSprite, typeof(Texture2D), false);

        if (selectedSprite == null)
        {
            EditorGUILayout.HelpBox("Select a sprite sheet PNG file in the Project window, then use this tool.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        pixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit:", pixelsPerUnit);
        frameRate = EditorGUILayout.FloatField("Frame Rate:", frameRate);
        autoDetectGrid = EditorGUILayout.Toggle("Auto-detect Grid Size:", autoDetectGrid);

        if (!autoDetectGrid)
        {
            gridSize = EditorGUILayout.Vector2IntField("Grid Size (Columns x Rows):", gridSize);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Setup Animation", GUILayout.Height(30)))
        {
            ProcessSpriteSheet();
        }
    }

    void ProcessSpriteSheet()
    {
        string path = AssetDatabase.GetAssetPath(selectedSprite);
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        if (texture == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not load texture!", "OK");
            return;
        }

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        // Set import settings
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        // Detect grid size if auto
        Vector2Int currentGridSize = gridSize;
        if (autoDetectGrid)
        {
            int width = texture.width;
            int height = texture.height;

            if (width > height * 2)
            {
                // Horizontal strip - try to find best frame count
                int frameHeight = height;
                int[] possibleFrames = { 4, 6, 8, 10, 12, 16, 20 };
                int bestFit = width / frameHeight; // Initial estimate

                foreach (int frames in possibleFrames)
                {
                    int frameWidth = width / frames;
                    // Check if divides evenly and sprite dimensions are reasonable
                    if (width % frames == 0 && frameWidth > frameHeight * 0.3f && frameWidth < frameHeight * 3f)
                    {
                        bestFit = frames;
                        break;
                    }
                }

                currentGridSize = new Vector2Int(bestFit, 1);
            }
            else if (height > width * 2)
            {
                // Vertical strip
                int frameWidth = width;
                int[] possibleFrames = { 4, 6, 8, 10, 12, 16, 20 };
                int bestFit = height / frameWidth;

                foreach (int frames in possibleFrames)
                {
                    int frameHeight = height / frames;
                    if (height % frames == 0 && frameHeight > frameWidth * 0.3f && frameHeight < frameWidth * 3f)
                    {
                        bestFit = frames;
                        break;
                    }
                }

                currentGridSize = new Vector2Int(1, bestFit);
            }
            else
            {
                // Try common grid sizes
                if (width % 8 == 0 && height % 8 == 0) currentGridSize = new Vector2Int(8, 8);
                else if (width % 6 == 0 && height % 6 == 0) currentGridSize = new Vector2Int(6, 6);
                else if (width % 4 == 0 && height % 4 == 0) currentGridSize = new Vector2Int(4, 4);
                else currentGridSize = new Vector2Int(8, 1);
            }
        }

        // Create sprite rects
        int spriteWidth = texture.width / currentGridSize.x;
        int spriteHeight = texture.height / currentGridSize.y;
        List<SpriteMetaData> sprites = new List<SpriteMetaData>();
        string animName = Path.GetFileNameWithoutExtension(path);

        for (int row = 0; row < currentGridSize.y; row++)
        {
            for (int col = 0; col < currentGridSize.x; col++)
            {
                SpriteMetaData sprite = new SpriteMetaData
                {
                    name = $"{animName}_{row * currentGridSize.x + col}",
                    rect = new Rect(col * spriteWidth, texture.height - (row + 1) * spriteHeight, spriteWidth, spriteHeight),
                    pivot = new Vector2(0.5f, 0.5f),
                    alignment = (int)SpriteAlignment.Center
                };
                sprites.Add(sprite);
            }
        }

        // Set sprite sheet data (using deprecated API with warning suppression)
        // Note: The new ISpriteEditorDataProvider API is complex and may require additional setup
        // The old API still works, so we suppress the warning
#pragma warning disable CS0618
        importer.spritesheet = sprites.ToArray();
#pragma warning restore CS0618
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        // Get all sprites
        Object[] spriteObjects = AssetDatabase.LoadAllAssetsAtPath(path);
        List<Sprite> spriteList = new List<Sprite>();
        foreach (Object obj in spriteObjects)
        {
            if (obj is Sprite sprite)
                spriteList.Add(sprite);
        }

        if (spriteList.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No sprites found after slicing!", "OK");
            return;
        }

        // Sort sprites
        spriteList.Sort((a, b) => string.Compare(a.name, b.name));

        // Create animation clip
        string folderPath = Path.GetDirectoryName(path);
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

        AssetDatabase.CreateAsset(clip, animPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success",
            $"Animation created!\n\n" +
            $"Sprites: {spriteList.Count}\n" +
            $"Animation: {animPath}\n\n" +
            "Next: Create Animator Controller and assign this clip.",
            "OK");
    }
}

