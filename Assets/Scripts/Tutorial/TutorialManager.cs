using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Level_1/Map_1";
    [SerializeField] private GameObject initialExplanation;
    [SerializeField] private GameObject LaunchSoldierExplanation;
    [SerializeField] private GameObject goalExplanation;

    private int currentStep = 0;
    private const int TOTAL_STEPS = 3;

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
        if (initialExplanation != null)
            initialExplanation.SetActive(false);
        if (LaunchSoldierExplanation != null)
            LaunchSoldierExplanation.SetActive(false);
        if (goalExplanation != null)
            goalExplanation.SetActive(false);

        // Show current step
        switch (currentStep)
        {
            case 0: // Launching soldiers
                if (initialExplanation != null)
                    initialExplanation.SetActive(true);
                break;
            case 1: // When soldiers fight
                if (LaunchSoldierExplanation != null)
                    LaunchSoldierExplanation.SetActive(true);
                break;
            case 2: // The goal
                if (goalExplanation != null)
                    goalExplanation.SetActive(true);
                break;
        }
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}

