using System;
using UnityEditor;
/*
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public static class TaskForceSettings
{
    private const string DEFINE_NAME = "NavOpsTaskForceSettings";

    private const string MENUITEM_TASKFORCE_SINGLE = "NavOps/Settings/TaskForce/Single";
    private const string MENUITEM_TASKFORCE_MULTIPLE = "NavOps/Settings/TaskForce/Multiple(3)";

    [MenuItem(MENUITEM_TASKFORCE_SINGLE)]
    public static void SetTaskForceSingle()
    {
        TaskForce taskForce = Selection.activeGameObject.GetComponent<TaskForce>();
        taskForce.Units = new Warship[1];

        Warship[] taskForceUnits = Selection.activeGameObject.GetComponentsInChildren<Warship>();
        taskForce.Units[0] = taskForceUnits[0];
    }

    [MenuItem(MENUITEM_TASKFORCE_SINGLE, isValidateFunction: true)]
    public static bool ValidateSetTaskForceSingle()
    {
        return Selection.activeGameObject.GetComponent<TaskForce>() != null;
    }

    [MenuItem(MENUITEM_TASKFORCE_MULTIPLE)]
    public static void SetTaskForceMultiple()
    {
        TaskForce taskForce = Selection.activeGameObject.GetComponent<TaskForce>();
        taskForce.Units = new Warship[3];

        Warship[] taskForceUnits = Selection.activeGameObject.GetComponentsInChildren<Warship>();
        for (int i = 0; i < taskForceUnits.Length; i++)
        {
            taskForce.Units[i] = taskForceUnits[i];
        }
    }

    [MenuItem(MENUITEM_TASKFORCE_MULTIPLE, isValidateFunction: true)]
    public static bool ValidateSetTaskForceMultiple()
    {
        return Selection.activeGameObject.GetComponent<TaskForce>() != null;
    }

    // [MenuItem(MENUITEM_DIST_DISCRETE)]
    // public static void SetBuildVersionDiscrete()
    // {
    //     BehaviorParameters behaviorParameters = Selection.activeGameObject.GetComponent<BehaviorParameters>();
    //     behaviorParameters.BrainParameters.VectorObservationSize = 118;
    //     behaviorParameters.BrainParameters.ActionSpec = new ActionSpec(discreteBranchSizes: new int[] { 6 });
    // }

    // [MenuItem(MENUITEM_DIST_DISCRETE, isValidateFunction: true)]
    // public static bool ValidateSetBuildVersionDiscrete()
    // {
    //     return Selection.activeGameObject.GetComponent<BehaviorParameters>() != null;
    // }

    // [MenuItem(MENUITEM_DIST_MULTIDISCRETE)]
    // public static void SetBuildVersionMultiDiscrete()
    // {
    //     BehaviorParameters behaviorParameters = Selection.activeGameObject.GetComponent<BehaviorParameters>();
    //     behaviorParameters.BrainParameters.VectorObservationSize = 118;
    //     behaviorParameters.BrainParameters.ActionSpec = new ActionSpec(discreteBranchSizes: new int[] { 5, 2 });
    // }

    // [MenuItem(MENUITEM_DIST_MULTIDISCRETE, isValidateFunction: true)]
    // public static bool ValidateSetBuildVersionMultiDiscrete()
    // {
    //     return Selection.activeGameObject.GetComponent<BehaviorParameters>() != null;
    // }
}
*/
