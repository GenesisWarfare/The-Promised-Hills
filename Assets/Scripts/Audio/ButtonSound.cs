using UnityEngine;
using UnityEngine.UI;

/**
 * ButtonSound - Plays sound when button is clicked
 * Attach this to any Button GameObject
 * Exclude UnitSelectionButton (it doesn't use Unity Button component)
 */
[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }

    void OnButtonClicked()
    {
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.PlayButtonClick();
        }
    }
}
