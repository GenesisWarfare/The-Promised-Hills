using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameButton : MonoBehaviour
{
    [SerializeField] private string tutorialSceneName = "Tutorial";
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
    }

    void Update()
    {
        if (MobileInputHelper.WasPressedThisFrame())
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(MobileInputHelper.GetPosition());
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit != null && hit.gameObject == gameObject)
            {
                LoadTutorialScene();
            }
        }

        // Visual feedback on hover
        if (spriteRenderer != null)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(MobileInputHelper.GetPosition());
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit != null && hit.gameObject == gameObject)
            {
                spriteRenderer.color = hoverColor;
            }
            else
            {
                spriteRenderer.color = normalColor;
            }
        }
    }

    void LoadTutorialScene()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }
}

