using UnityEngine;
using UnityEditor;
using System.IO;

/**
 * Creates a soldier prefab with all required components
 *
 * Usage:
 * Tools -> Create Soldier Prefab
 */
public class CreateSoldierPrefab : EditorWindow
{
    private string prefabName = "Soldier";
    private bool useAnimatedUnit = true;
    private string unitTag = "PlayerUnit";
    private Sprite soldierSprite;

    [MenuItem("Tools/Create Soldier Prefab", false, 1)]
    public static void ShowWindow()
    {
        GetWindow<CreateSoldierPrefab>("Create Soldier Prefab");
    }

    void OnGUI()
    {
        GUILayout.Label("Create Soldier Prefab", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        prefabName = EditorGUILayout.TextField("Prefab Name:", prefabName);
        useAnimatedUnit = EditorGUILayout.Toggle("Use Animated Unit:", useAnimatedUnit);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Unit Tag:");
        unitTag = EditorGUILayout.TextField(unitTag);

        EditorGUILayout.Space();
        soldierSprite = (Sprite)EditorGUILayout.ObjectField("Soldier Sprite:", soldierSprite, typeof(Sprite), false);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This will create a GameObject with:\n" +
            "- SpriteRenderer\n" +
            "- BoxCollider2D (Trigger)\n" +
            "- Unit or AnimatedUnit script\n" +
            "- Animator (if AnimatedUnit)\n" +
            "- Proper tag",
            MessageType.Info
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Prefab", GUILayout.Height(30)))
        {
            CreatePrefab();
        }
    }

    void CreatePrefab()
    {
        // Create GameObject
        GameObject soldier = new GameObject(prefabName);

        // Add SpriteRenderer
        SpriteRenderer sr = soldier.AddComponent<SpriteRenderer>();
        if (soldierSprite != null)
        {
            sr.sprite = soldierSprite;
        }
        sr.sortingOrder = 1;

        // Add Collider2D (as trigger for detection)
        BoxCollider2D collider = soldier.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.5f, 1f); // Adjust size as needed

        // Add Unit script
        if (useAnimatedUnit)
        {
            soldier.AddComponent<AnimatedUnit>();

            // Add Animator
            Animator animator = soldier.AddComponent<Animator>();
            // Note: User will need to assign Animator Controller manually
        }
        else
        {
            soldier.AddComponent<Unit>();
        }

        // Set tag
        if (!TagExists(unitTag))
        {
            CreateTag(unitTag);
        }
        soldier.tag = unitTag;

        // Set position
        soldier.transform.position = Vector3.zero;

        // Create prefab
        string prefabPath = $"Assets/Prefabs/{prefabName}.prefab";

        // Make sure Prefabs folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // Delete existing prefab if it exists
        if (File.Exists(prefabPath))
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(soldier, prefabPath);
        DestroyImmediate(soldier);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = prefab;

        EditorUtility.DisplayDialog("Success",
            $"Soldier prefab created at:\n{prefabPath}\n\n" +
            (useAnimatedUnit ? "Don't forget to:\n- Assign Animator Controller\n- Assign Animation Clips" : ""),
            "OK");
    }

    bool TagExists(string tag)
    {
        try
        {
            GameObject.FindGameObjectWithTag(tag);
            return true;
        }
        catch
        {
            return false;
        }
    }

    void CreateTag(string tag)
    {
        // Add tag through SerializedObject
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Check if tag already exists
        bool exists = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tag))
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = tag;
            tagManager.ApplyModifiedProperties();
        }
    }
}
