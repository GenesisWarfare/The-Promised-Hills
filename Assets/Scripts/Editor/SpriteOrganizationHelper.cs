using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/**
 * Helper tool to analyze and organize sprites
 * Shows what sprite sheets need slicing, what animations exist, etc.
 */
public class SpriteOrganizationHelper : EditorWindow
{
    private Vector2 scrollPosition;
    private Dictionary<string, List<string>> spriteInfo = new Dictionary<string, List<string>>();

    [MenuItem("Tools/Sprite Organization Helper")]
    public static void ShowWindow()
    {
        GetWindow<SpriteOrganizationHelper>("Sprite Organization Helper");
    }

    void OnGUI()
    {
        GUILayout.Label("Sprite Organization Analysis", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("Analyze Sprites Folder", GUILayout.Height(30)))
        {
            AnalyzeSprites();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var category in spriteInfo.Keys)
        {
            EditorGUILayout.LabelField(category, EditorStyles.boldLabel);
            foreach (var item in spriteInfo[category])
            {
                EditorGUILayout.LabelField("  • " + item, EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();
    }

    void AnalyzeSprites()
    {
        spriteInfo.Clear();

        // Find all sprites
        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites" });
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites" });

        List<string> individualSprites = new List<string>();
        List<string> spriteSheets = new List<string>();
        List<string> animationPacks = new List<string>();
        List<string> backgrounds = new List<string>();
        List<string> uiElements = new List<string>();

        // Analyze textures (potential sprite sheets)
        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null)
            {
                if (importer.textureType == TextureImporterType.Sprite)
                {
                    if (importer.spriteImportMode == SpriteImportMode.Multiple)
                    {
                        spriteSheets.Add(path);
                    }
                    else
                    {
                        individualSprites.Add(path);
                    }
                }
            }
        }

        // Find animation packs
        string[] animGuids = AssetDatabase.FindAssets("t:AnimationClip", new[] { "Assets/Sprites" });
        HashSet<string> packFolders = new HashSet<string>();
        foreach (string guid in animGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string folder = System.IO.Path.GetDirectoryName(path);
            if (!packFolders.Contains(folder))
            {
                packFolders.Add(folder);
                animationPacks.Add(folder.Replace("Assets/Sprites/", ""));
            }
        }

        // Categorize backgrounds
        foreach (string path in individualSprites)
        {
            string name = System.IO.Path.GetFileName(path);
            if (name.ToLower().Contains("background") || name.ToLower().Contains("desert") || 
                name.ToLower().Contains("castle") || name.ToLower().Contains("building"))
            {
                backgrounds.Add(name);
            }
            else if (path.Contains("/UI/"))
            {
                uiElements.Add(name);
            }
        }

        // Organize info
        spriteInfo["Animation Packs Found"] = animationPacks.Distinct().ToList();
        spriteInfo["Sprite Sheets (Multiple)"] = spriteSheets.Select(p => System.IO.Path.GetFileName(p)).ToList();
        spriteInfo["Background Sprites"] = backgrounds.Distinct().ToList();
        spriteInfo["UI Elements"] = uiElements.Distinct().ToList();
        spriteInfo["Individual Sprites"] = individualSprites
            .Where(p => !backgrounds.Contains(System.IO.Path.GetFileName(p)) && !p.Contains("/UI/"))
            .Select(p => System.IO.Path.GetFileName(p))
            .Take(20)
            .ToList();

        if (individualSprites.Count > 20)
        {
            spriteInfo["Individual Sprites"].Add($"... and {individualSprites.Count - 20} more");
        }

        Debug.Log($"Sprite Analysis Complete: Found {animationPacks.Count} animation packs, {spriteSheets.Count} sprite sheets, {backgrounds.Count} backgrounds");
    }
}
