using UnityEngine;

/**
 * Manages invisible bases at screen edges
 * Creates bases when units reach the end of the screen
 */
public class BaseManager : MonoBehaviour
{
    [Header("Base Settings")]
    public int playerBaseHealth = 200;
    public int enemyBaseHealth = 200;

    [Header("Screen Edge Detection")]
    public float edgeDetectionDistance = 0.5f;

    private GameBase playerBase;
    private GameBase enemyBase;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("BaseManager: Main camera not found!");
            return;
        }

        CreateBases();
    }

    void CreateBases()
    {
        // Get screen bounds
        float screenLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
        float screenRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;

        // Create player base (left side of screen)
        GameObject playerBaseObj = new GameObject("PlayerBase");
        playerBaseObj.transform.position = new Vector3(screenLeft - edgeDetectionDistance, 0, 0);
        playerBase = playerBaseObj.AddComponent<GameBase>();
        playerBase.isPlayerBase = true;
        playerBase.maxHealth = playerBaseHealth;

        // Add invisible collider for detection
        BoxCollider2D playerCollider = playerBaseObj.AddComponent<BoxCollider2D>();
        playerCollider.isTrigger = true;
        playerCollider.size = new Vector2(edgeDetectionDistance * 2, 20f); // Tall enough to catch units

        // Create enemy base (right side of screen)
        GameObject enemyBaseObj = new GameObject("EnemyBase");
        enemyBaseObj.transform.position = new Vector3(screenRight + edgeDetectionDistance, 0, 0);
        enemyBase = enemyBaseObj.AddComponent<GameBase>();
        enemyBase.isPlayerBase = false;
        enemyBase.maxHealth = enemyBaseHealth;

        // Add invisible collider for detection
        BoxCollider2D enemyCollider = enemyBaseObj.AddComponent<BoxCollider2D>();
        enemyCollider.isTrigger = true;
        enemyCollider.size = new Vector2(edgeDetectionDistance * 2, 20f);

        Debug.Log($"Bases created: Player at {playerBaseObj.transform.position.x} (health: {playerBaseHealth}), Enemy at {enemyBaseObj.transform.position.x} (health: {enemyBaseHealth})");
    }

    public GameBase GetPlayerBase() => playerBase;
    public GameBase GetEnemyBase() => enemyBase;
}
