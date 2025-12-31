using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

/**
 * Creates a simple Animator Controller for units
 * 
 * Usage:
 * Assets -> Create -> Unit Animator Controller
 */
public class CreateAnimatorController : EditorWindow
{
    private string controllerName = "SoldierAnimator";
    private AnimationClip idleClip;
    private AnimationClip runClip;
    private AnimationClip attackClip;
    private AnimationClip deathClip;
    private AnimationClip takeHitClip;

    [MenuItem("Assets/Create/Unit Animator Controller", false, 1)]
    public static void ShowWindow()
    {
        GetWindow<CreateAnimatorController>("Create Animator Controller");
    }

    void OnGUI()
    {
        GUILayout.Label("Create Unit Animator Controller", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        controllerName = EditorGUILayout.TextField("Controller Name:", controllerName);

        EditorGUILayout.Space();
        GUILayout.Label("Animation Clips:", EditorStyles.boldLabel);

        idleClip = (AnimationClip)EditorGUILayout.ObjectField("Idle:", idleClip, typeof(AnimationClip), false);
        runClip = (AnimationClip)EditorGUILayout.ObjectField("Run:", runClip, typeof(AnimationClip), false);
        attackClip = (AnimationClip)EditorGUILayout.ObjectField("Attack:", attackClip, typeof(AnimationClip), false);
        deathClip = (AnimationClip)EditorGUILayout.ObjectField("Death:", deathClip, typeof(AnimationClip), false);
        takeHitClip = (AnimationClip)EditorGUILayout.ObjectField("Take Hit:", takeHitClip, typeof(AnimationClip), false);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Assign your animation clips above, then click Create.\n" +
            "You can assign clips later in the Animator window if needed.",
            MessageType.Info
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Controller", GUILayout.Height(30)))
        {
            CreateController();
        }
    }

    void CreateController()
    {
        string folderPath = "Assets/Animations";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Animations");
        }

        string controllerPath = Path.Combine(folderPath, $"{controllerName}.controller");
        
        // Delete existing if it exists
        if (File.Exists(controllerPath))
        {
            AssetDatabase.DeleteAsset(controllerPath);
            AssetDatabase.Refresh();
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        stateMachine.name = controllerName;

        // Create states
        AnimatorState idleState = null;
        AnimatorState runState = null;
        AnimatorState attackState = null;
        AnimatorState deathState = null;
        AnimatorState takeHitState = null;

        if (idleClip != null)
        {
            idleState = stateMachine.AddState("Idle");
            idleState.motion = idleClip;
            idleState.writeDefaultValues = false;
            stateMachine.defaultState = idleState;
        }
        else
        {
            idleState = stateMachine.AddState("Idle");
            idleState.writeDefaultValues = false;
            stateMachine.defaultState = idleState;
        }

        if (runClip != null)
        {
            runState = stateMachine.AddState("Run");
            runState.motion = runClip;
            runState.writeDefaultValues = false;
        }
        else
        {
            runState = stateMachine.AddState("Run");
            runState.writeDefaultValues = false;
        }

        if (attackClip != null)
        {
            attackState = stateMachine.AddState("Attack");
            attackState.motion = attackClip;
            attackState.writeDefaultValues = false;
        }
        else
        {
            attackState = stateMachine.AddState("Attack");
            attackState.writeDefaultValues = false;
        }

        if (deathClip != null)
        {
            deathState = stateMachine.AddState("Death");
            deathState.motion = deathClip;
            deathState.writeDefaultValues = false;
        }
        else
        {
            deathState = stateMachine.AddState("Death");
            deathState.writeDefaultValues = false;
        }

        if (takeHitClip != null)
        {
            takeHitState = stateMachine.AddState("TakeHit");
            takeHitState.motion = takeHitClip;
            takeHitState.writeDefaultValues = false;
        }
        else
        {
            takeHitState = stateMachine.AddState("TakeHit");
            takeHitState.writeDefaultValues = false;
        }

        // Add parameters
        controller.AddParameter("Idle", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Run", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("TakeHit", AnimatorControllerParameterType.Trigger);

        // Create transitions
        if (idleState != null && runState != null)
        {
            AnimatorStateTransition idleToRun = idleState.AddTransition(runState);
            idleToRun.AddCondition(AnimatorConditionMode.If, 0f, "Run");
            idleToRun.hasExitTime = false;
            idleToRun.duration = 0.1f;

            AnimatorStateTransition runToIdle = runState.AddTransition(idleState);
            runToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "Run");
            runToIdle.hasExitTime = false;
            runToIdle.duration = 0.1f;
        }

        if (idleState != null && attackState != null)
        {
            AnimatorStateTransition idleToAttack = idleState.AddTransition(attackState);
            idleToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            idleToAttack.hasExitTime = false;
            idleToAttack.duration = 0.1f;

            AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
            attackToIdle.hasExitTime = true;
            attackToIdle.exitTime = 0.8f;
            attackToIdle.duration = 0.1f;
        }

        if (runState != null && attackState != null)
        {
            AnimatorStateTransition runToAttack = runState.AddTransition(attackState);
            runToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            runToAttack.hasExitTime = false;
            runToAttack.duration = 0.1f;

            AnimatorStateTransition attackToRun = attackState.AddTransition(runState);
            attackToRun.AddCondition(AnimatorConditionMode.If, 0f, "Run");
            attackToRun.hasExitTime = true;
            attackToRun.exitTime = 0.8f;
            attackToRun.duration = 0.1f;
        }

        // Death from any state
        if (deathState != null)
        {
            if (idleState != null)
            {
                AnimatorStateTransition toDeath = idleState.AddTransition(deathState);
                toDeath.AddCondition(AnimatorConditionMode.If, 0f, "Death");
                toDeath.hasExitTime = false;
                toDeath.duration = 0.1f;
            }
            if (runState != null)
            {
                AnimatorStateTransition toDeath = runState.AddTransition(deathState);
                toDeath.AddCondition(AnimatorConditionMode.If, 0f, "Death");
                toDeath.hasExitTime = false;
                toDeath.duration = 0.1f;
            }
            if (attackState != null)
            {
                AnimatorStateTransition toDeath = attackState.AddTransition(deathState);
                toDeath.AddCondition(AnimatorConditionMode.If, 0f, "Death");
                toDeath.hasExitTime = false;
                toDeath.duration = 0.1f;
            }
        }

        // Take Hit
        if (takeHitState != null)
        {
            if (idleState != null)
            {
                AnimatorStateTransition idleToTakeHit = idleState.AddTransition(takeHitState);
                idleToTakeHit.AddCondition(AnimatorConditionMode.If, 0f, "TakeHit");
                idleToTakeHit.hasExitTime = false;
                idleToTakeHit.duration = 0.1f;

                AnimatorStateTransition takeHitToIdle = takeHitState.AddTransition(idleState);
                takeHitToIdle.hasExitTime = true;
                takeHitToIdle.exitTime = 0.8f;
                takeHitToIdle.duration = 0.1f;
            }
            if (runState != null)
            {
                AnimatorStateTransition runToTakeHit = runState.AddTransition(takeHitState);
                runToTakeHit.AddCondition(AnimatorConditionMode.If, 0f, "TakeHit");
                runToTakeHit.hasExitTime = false;
                runToTakeHit.duration = 0.1f;

                AnimatorStateTransition takeHitToRun = takeHitState.AddTransition(runState);
                takeHitToRun.AddCondition(AnimatorConditionMode.If, 0f, "Run");
                takeHitToRun.hasExitTime = true;
                takeHitToRun.exitTime = 0.8f;
                takeHitToRun.duration = 0.1f;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success",
            $"Animator Controller created at:\n{controllerPath}\n\n" +
            "Open it in the Animator window to assign animation clips to states.",
            "OK");

        Selection.activeObject = controller;
    }
}

