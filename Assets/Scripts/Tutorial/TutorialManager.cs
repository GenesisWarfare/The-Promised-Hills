using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Level_1/Map_1";
    [SerializeField] private GameObject Explanation1;
    [SerializeField] private GameObject Explanation2;
    [SerializeField] private GameObject Explanation3;
    [SerializeField] private GameObject Explanation4;

    private int currentStep = 0;
    private const int TOTAL_STEPS = 4;

    void Start()
    {
        ShowCurrentStep();
    }

    void Update()
    {
        // Advance to next step on any input
        if (Keyboard.current.anyKey.wasPressedThisFrame
            || Mouse.current.leftButton.wasPressedThisFrame)
        {
            AdvanceStep();
        }
    }

    void AdvanceStep()
    {
        currentStep++;

        if (currentStep >= TOTAL_STEPS)
        {
            LoadNextScene();
        }
        else
        {
            ShowCurrentStep();
        }
    }

    void ShowCurrentStep()
    {
        // Hide all explanations
        if (Explanation1 != null)
            Explanation1.SetActive(false);
        if (Explanation2 != null)
            Explanation2.SetActive(false);
        if (Explanation3 != null)
            Explanation3.SetActive(false);
        if (Explanation4 != null)
            Explanation4.SetActive(false);

        switch (currentStep)
        {
            case 0:
                if (Explanation1 != null)
                    Explanation1.SetActive(true);
                break;
            case 1:
                if (Explanation2 != null)
                    Explanation2.SetActive(true);
                break;
            case 2:
                if (Explanation3 != null)
                    Explanation3.SetActive(true);
                break;
            case 3:
                if (Explanation4 != null)
                    Explanation4.SetActive(true);
                break;
        }
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}

