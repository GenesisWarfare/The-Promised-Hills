using UnityEngine;
using UnityEditor;

/**
 * Editor script to validate that a Unit prefab has all required components
 */
public class UnitPrefabValidator : EditorWindow
{
    [MenuItem("Tools/Validate Unit Prefab")]
    public static void ValidateSelectedPrefab()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a GameObject in the scene or a prefab.", "OK");
            return;
        }

        bool isValid = true;
        System.Text.StringBuilder issues = new System.Text.StringBuilder();

        // Check for required components
        if (selected.GetComponent<Transform>() == null)
        {
            issues.AppendLine("❌ Missing: Transform (should always be present)");
            isValid = false;
        }

        if (selected.GetComponent<SpriteRenderer>() == null)
        {
            issues.AppendLine("❌ Missing: SpriteRenderer");
            isValid = false;
        }

        Rigidbody2D rb = selected.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            issues.AppendLine("❌ Missing: Rigidbody2D");
            isValid = false;
        }
        else
        {
            if (rb.bodyType != RigidbodyType2D.Dynamic)
            {
                issues.AppendLine($"⚠️ Rigidbody2D Body Type is {rb.bodyType}, should be Dynamic");
            }
            if (rb.collisionDetectionMode == CollisionDetectionMode2D.Discrete)
            {
                issues.AppendLine("⚠️ Collision Detection is Discrete, Continuous is recommended");
            }
            if (rb.gravityScale != 0)
            {
                issues.AppendLine($"⚠️ Gravity Scale is {rb.gravityScale}, should be 0 for 2D side-scrolling");
            }
            if (!rb.freezeRotation)
            {
                issues.AppendLine("⚠️ Rotation is not frozen, should freeze Z rotation");
            }
        }

        Collider2D col = selected.GetComponent<Collider2D>();
        if (col == null)
        {
            issues.AppendLine("❌ Missing: Collider2D (BoxCollider2D recommended)");
            isValid = false;
        }
        else
        {
            if (col.isTrigger)
            {
                issues.AppendLine("❌ Collider2D is set as Trigger! OnCollisionEnter2D won't fire.");
                isValid = false;
            }
        }

        Unit unit = selected.GetComponent<Unit>();
        AnimatedUnit animUnit = selected.GetComponent<AnimatedUnit>();
        if (unit == null && animUnit == null)
        {
            issues.AppendLine("❌ Missing: Unit or AnimatedUnit script");
            isValid = false;
        }
        else if (animUnit != null)
        {
            if (selected.GetComponent<Animator>() == null)
            {
                issues.AppendLine("⚠️ AnimatedUnit found but no Animator component");
            }
        }

        // Check tag
        if (selected.CompareTag("Untagged"))
        {
            issues.AppendLine("⚠️ Tag is 'Untagged', should be 'PlayerUnit' or 'EnemyUnit'");
        }
        else if (!selected.CompareTag("PlayerUnit") && !selected.CompareTag("EnemyUnit"))
        {
            issues.AppendLine($"⚠️ Tag is '{selected.tag}', should be 'PlayerUnit' or 'EnemyUnit'");
        }

        // Display results
        string message = isValid ? "✅ Prefab is valid!" : "❌ Prefab has issues:";
        message += "\n\n" + issues.ToString();

        EditorUtility.DisplayDialog("Prefab Validation", message, "OK");

        if (!isValid)
        {
            Debug.LogError($"Prefab Validation Failed for {selected.name}:\n{issues}");
        }
    }

    [MenuItem("Tools/Validate Unit Prefab", true)]
    public static bool ValidateSelectedPrefabValidate()
    {
        return Selection.activeGameObject != null;
    }
}

