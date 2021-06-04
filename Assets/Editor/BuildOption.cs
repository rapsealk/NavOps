using System;
using UnityEditor;
/*
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public static class BuildOption
{
    private const string DEFINE_NAME = "NavOpsBuildOption";

    private const string MENUITEM_DIST_CONTINUOUS = "NavOps/Dist/Continuous";
    private const string MENUITEM_DIST_DISCRETE = "NavOps/Dist/Discrete";
    private const string MENUITEM_DIST_MULTIDISCRETE = "NavOps/Dist/MultiDiscrete";

    [MenuItem(MENUITEM_DIST_CONTINUOUS)]
    public static void SetBuildVersionContinuous()
    {
        BehaviorParameters behaviorParameters = Selection.activeGameObject.GetComponent<BehaviorParameters>();
        behaviorParameters.BrainParameters.VectorObservationSize = 118;
        behaviorParameters.BrainParameters.ActionSpec = new ActionSpec(numContinuousActions: 6);
    }

    [MenuItem(MENUITEM_DIST_CONTINUOUS, isValidateFunction: true)]
    public static bool ValidateSetBuildVersionContinuous()
    {
        return Selection.activeGameObject.GetComponent<BehaviorParameters>() != null;
    }

    [MenuItem(MENUITEM_DIST_DISCRETE)]
    public static void SetBuildVersionDiscrete()
    {
        BehaviorParameters behaviorParameters = Selection.activeGameObject.GetComponent<BehaviorParameters>();
        behaviorParameters.BrainParameters.VectorObservationSize = 118;
        behaviorParameters.BrainParameters.ActionSpec = new ActionSpec(discreteBranchSizes: new int[] { 6 });
    }

    [MenuItem(MENUITEM_DIST_DISCRETE, isValidateFunction: true)]
    public static bool ValidateSetBuildVersionDiscrete()
    {
        return Selection.activeGameObject.GetComponent<BehaviorParameters>() != null;
    }

    [MenuItem(MENUITEM_DIST_MULTIDISCRETE)]
    public static void SetBuildVersionMultiDiscrete()
    {
        BehaviorParameters behaviorParameters = Selection.activeGameObject.GetComponent<BehaviorParameters>();
        behaviorParameters.BrainParameters.VectorObservationSize = 118;
        behaviorParameters.BrainParameters.ActionSpec = new ActionSpec(discreteBranchSizes: new int[] { 5, 2 });
    }

    [MenuItem(MENUITEM_DIST_MULTIDISCRETE, isValidateFunction: true)]
    public static bool ValidateSetBuildVersionMultiDiscrete()
    {
        return Selection.activeGameObject.GetComponent<BehaviorParameters>() != null;
    }
}

public static class BuildOptionManager
{
    public static void AddCompilerDefine(string newDefineCompileConstant, BuildTargetGroup[] buildTargetGroups = null)
    {
        if (buildTargetGroups == null)
        {
            buildTargetGroups = new BuildTargetGroup[] { BuildTargetGroup.Standalone };
        }

        foreach (BuildTargetGroup buildTargetGroup in buildTargetGroups)
        {
            if (buildTargetGroup == BuildTargetGroup.Unknown)
            {
                continue;
            }

            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (!defines.Contains(newDefineCompileConstant))
            {
                if (defines.Length > 0)
                {
                    defines += ";";
                }

                defines += newDefineCompileConstant;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            }
        }
    }

    public static void RemoveCompileDefine(string defineCompileConstant, BuildTargetGroup[] buildTargetGroups = null)
    {
        if (buildTargetGroups == null)
        {
            buildTargetGroups = new BuildTargetGroup[] { BuildTargetGroup.Standalone };
        }

        foreach (BuildTargetGroup buildTargetGroup in buildTargetGroups)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            int index = defines.IndexOf(defineCompileConstant);
            if (index < 0)
            {
                continue;
            }
            else if (index > 0)
            {
                index -= 1;
            }

            int lengthToRemove = Math.Min(defineCompileConstant.Length + 1, defines.Length - index);
            defines = defines.Remove(index, lengthToRemove);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
        }
    }
}
*/
