using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace DysonSphereProgram.Modding.OffGridConstruction;

public static class GridRotation
{
  private static Quaternion rotationOverride = Quaternion.identity;
  private static Quaternion rotationOverrideInverse = Quaternion.Inverse(rotationOverride);

  public static void SetRotationOverride(in Quaternion q)
  {
    rotationOverride = q;
    rotationOverrideInverse = Quaternion.Inverse(rotationOverride);
  }

  public static Vector3 PrefixPatch(Vector3 vector)
  {
    return rotationOverrideInverse * vector;
  }
  public static void PrefixPatch(ref Vector3 vector)
  {
    vector = rotationOverrideInverse * vector;
  }
  public static void PrefixPatch(ref Quaternion q)
  {
    q = rotationOverrideInverse * q;
  }
  public static Quaternion PrefixPatch(Quaternion q)
  {
    return rotationOverrideInverse * q;
  }

  public static Vector3 PostfixPatch(Vector3 vector)
  {
    return rotationOverride * vector;
  }
  public static void PostfixPatch(ref Vector3 vector)
  {
    vector = rotationOverride * vector;
  }
  public static void PostfixPatch(ref Quaternion q)
  {
    q = rotationOverride * q;
  }
  public static Quaternion PostfixPatch(Quaternion q)
  {
    return rotationOverride * q;
  }
}

public static class GridRotationPatches
{
  [HarmonyPrefix]
  [HarmonyPatch(typeof(PlanetGrid), nameof(PlanetGrid.CalcLocalGridSize))]
  public static void Prefix_CalcLocalGridSize(ref Vector3 posR, ref Vector3 dir)
  {
    GridRotation.PrefixPatch(ref posR);
    GridRotation.PrefixPatch(ref dir);
  }

  [HarmonyPrefix]
  [HarmonyPatch(typeof(PlanetGrid), nameof(PlanetGrid.CalcSegmentsAcross))]
  public static void Prefix_CalcSegmentsAcross(ref Vector3 posR, ref Vector3 posA, ref Vector3 posB)
  {
    GridRotation.PrefixPatch(ref posR);
    GridRotation.PrefixPatch(ref posA);
    GridRotation.PrefixPatch(ref posB);
  }

  [HarmonyPrefix]
  [HarmonyPatch(typeof(PlanetGrid), nameof(PlanetGrid.GratboxByCenterSize))]
  public static void Prefix_GratboxByCenterSize(ref Vector3 center)
  {
    GridRotation.PrefixPatch(ref center);
  }

  [HarmonyPrefix]
  [HarmonyPatch(typeof(PlanetGrid), nameof(PlanetGrid.IsPointInGratbox))]
  public static void Prefix_IsPointInGratbox(ref Vector3 point)
  {
    GridRotation.PrefixPatch(ref point);
  }

  [HarmonyPrefix]
  [HarmonyPatch(typeof(PlanetGrid), nameof(PlanetGrid.SnapLineNonAlloc))]
  public static void Prefix_SnapLineNonAlloc(ref Vector3 begin, ref Vector3 end, ref Vector3[] snaps)
  {
    GridRotation.PrefixPatch(ref begin);
    GridRotation.PrefixPatch(ref end);
    for (int i = 0; i < snaps.Length; i++)
      GridRotation.PrefixPatch(ref snaps[i]);
  }

  [HarmonyPostfix]
  [HarmonyPatch(typeof(PlanetGrid), nameof(PlanetGrid.SnapLineNonAlloc))]
  public static void Postfix_SnapLineNonAlloc(ref Vector3[] snaps)
  {
    for (int i = 0; i < snaps.Length; i++)
      GridRotation.PostfixPatch(ref snaps[i]);
  }

  [HarmonyPrefix]
  [HarmonyPatch(typeof(PlanetAuxData), nameof(PlanetAuxData.ReformSnap))]
  public static void Prefix_ReformSnap(ref Vector3 pos, ref Vector3[] reformPoints)
  {
    GridRotation.PrefixPatch(ref pos);
    for (int i = 0; i < reformPoints.Length; i++)
      GridRotation.PrefixPatch(ref reformPoints[i]);
  }

  [HarmonyPrefix]
  [HarmonyPatch(typeof(PlanetAuxData), nameof(PlanetAuxData.ReformSnap))]
  public static void Postfix_ReformSnap(ref Vector3 reformCenter)
  {
    GridRotation.PostfixPatch(ref reformCenter);
  }

  [HarmonyPrefix]
  [HarmonyPatch(typeof(PlanetAuxData), nameof(PlanetAuxData.SnapDotsByGratBoxNonAlloc))]
  public static void Prefix_SnapDotsByGratBoxNonAlloc(ref Vector3 begin, ref Vector3 end, ref Vector3[] snaps)
  {
    GridRotation.PrefixPatch(ref begin);
    GridRotation.PrefixPatch(ref end);
    for (int i = 0; i < snaps.Length; i++)
      GridRotation.PrefixPatch(ref snaps[i]);
  }

  [HarmonyPostfix]
  [HarmonyPatch(typeof(PlanetAuxData), nameof(PlanetAuxData.SnapDotsByGratBoxNonAlloc))]
  public static void Postfix_SnapDotsByGratBoxNonAlloc(ref Vector3[] snaps)
  {
    for (int i = 0; i < snaps.Length; i++)
      GridRotation.PostfixPatch(ref snaps[i]);
  }

  [HarmonyPrefix]
  [HarmonyPatch(typeof(PlanetAuxData), nameof(PlanetAuxData.SnapDotsNonAlloc))]
  public static void Prefix_SnapDotsNonAlloc(ref Vector3 begin, ref Vector3 end, ref Vector3[] snaps)
  {
    GridRotation.PrefixPatch(ref begin);
    GridRotation.PrefixPatch(ref end);
    for (int i = 0; i < snaps.Length; i++)
      GridRotation.PrefixPatch(ref snaps[i]);
  }

  [HarmonyPostfix]
  [HarmonyPatch(typeof(PlanetAuxData), nameof(PlanetAuxData.SnapDotsNonAlloc))]
  public static void Postfix_SnapDotsNonAlloc(ref Vector3[] snaps)
  {
    for (int i = 0; i < snaps.Length; i++)
      GridRotation.PostfixPatch(ref snaps[i]);
  }

  [HarmonyPrefix]
  [HarmonyPatch(typeof(PlanetAuxData), nameof(PlanetAuxData.Snap))]
  public static void Prefix_Snap(ref Vector3 pos)
  {
    GridRotation.PrefixPatch(ref pos);
  }

  [HarmonyPostfix]
  [HarmonyPatch(typeof(PlanetAuxData), nameof(PlanetAuxData.Snap))]
  public static void Postfix_Snap(ref Vector3 __result)
  {
    GridRotation.PostfixPatch(ref __result);
  }

  [HarmonyPrefix]
  [HarmonyPatch(typeof(BlueprintUtils), nameof(BlueprintUtils.GenerateAreaGratBoxByBPData))]
  [HarmonyPatch(typeof(BlueprintUtils), nameof(BlueprintUtils.RefreshBuildPreview))]
  public static void Prefix_BlueprintPasteGeneration(ref Vector3[] _dots)
  {
    for (int i = 0; i < _dots.Length; i++)
      GridRotation.PrefixPatch(ref _dots[i]);
  }
  
  [HarmonyPostfix]
  [HarmonyPatch(typeof(BlueprintUtils), nameof(BlueprintUtils.GenerateAreaGratBoxByBPData))]
  public static void Postfix_BlueprintPasteGeneration(ref Vector3[] _dots)
  {
    for (int i = 0; i < _dots.Length; i++)
      GridRotation.PostfixPatch(ref _dots[i]);
  }
  
  [HarmonyPostfix]
  [HarmonyPatch(typeof(BlueprintUtils), nameof(BlueprintUtils.RefreshBuildPreview))]
  public static void Postfix_BlueprintBuildPreviews(ref Vector3[] _dots, BuildPreview[] _bpArray)
  {
    for (int i = 0; i < _dots.Length; i++)
      GridRotation.PostfixPatch(ref _dots[i]);
    for (int i = 0; i < _bpArray.Length; i++)
    {
      GridRotation.PostfixPatch(ref _bpArray[i].lpos);
      GridRotation.PostfixPatch(ref _bpArray[i].lpos2);
      GridRotation.PostfixPatch(ref _bpArray[i].lrot);
      GridRotation.PostfixPatch(ref _bpArray[i].lrot2);
    }
  }

  public static Quaternion InterceptSphericalRotation(Vector3 pos, float angle)
  {
    GridRotation.PrefixPatch(ref pos);
    return GridRotation.PostfixPatch(Maths.SphericalRotation(pos, angle));
  }

  static CodeMatch[] matchSphericalRotation = {
      new(ci => ci.IsLdloc())
    , new(OpCodes.Ldarg_0)
    , new(ci => ci.LoadsField(AccessTools.Field(typeof(BuildTool_Click), nameof(BuildTool_Click.yaw))))
    , new(ci => ci.Calls(AccessTools.Method(typeof(Maths), nameof(Maths.SphericalRotation))))
    , new(ci => ci.IsStloc())
  };


  [HarmonyTranspiler]
  [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.DeterminePreviews))]
  static IEnumerable<CodeInstruction> InterceptSphericalRotationCall(IEnumerable<CodeInstruction> code, ILGenerator generator)
  {
    var matcher = new CodeMatcher(code, generator);

    matcher.MatchForward(true, matchSphericalRotation);
    matcher.MatchForward(true, matchSphericalRotation);

    matcher.Advance(-1);

    matcher.SetOperandAndAdvance(AccessTools.Method(typeof(GridRotationPatches), nameof(InterceptSphericalRotation)));

    return matcher.InstructionEnumeration();
  }

  [HarmonyTranspiler]
  [HarmonyPatch(typeof(BuildTool_BlueprintCopy), nameof(BuildTool_BlueprintCopy.InitPreSelectGratBox))]
  [HarmonyPatch(typeof(BuildTool_BlueprintCopy), nameof(BuildTool_BlueprintCopy.DeterminePreSelectGratBox))]
  [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.InitArcGratBox))]
  [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.DetermineDragDots))]
  [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.DeterminRotate))]
  static IEnumerable<CodeInstruction> InterceptLatLngConversionCalls(IEnumerable<CodeInstruction> code, ILGenerator generator)
  {
    var matcher = new CodeMatcher(code, generator);
    
    var methodsToIntercept = new Dictionary<MethodInfo, MethodInfo>
    {
        { AccessTools.Method(typeof(BlueprintUtils), nameof(BlueprintUtils.GetMinimumGratBox)),
          AccessTools.Method(typeof(GridRotationPatches), nameof(InterceptGetMinimumGratBoxPrefix)) }
      , { AccessTools.Method(typeof(BlueprintUtils), nameof(BlueprintUtils.GetLatitudeRad)),
          AccessTools.Method(typeof(GridRotationPatches), nameof(InterceptGetLatitudeRadPrefix)) }
      , { AccessTools.Method(typeof(BlueprintUtils), nameof(BlueprintUtils.GetLongitudeRad)),
          AccessTools.Method(typeof(GridRotationPatches), nameof(InterceptGetLongitudeRadPrefix)) }
      , { AccessTools.Method(typeof(BlueprintUtils), nameof(BlueprintUtils.GetLongitudeRadPerGrid), new [] { typeof(Vector3), typeof(int) }),
          AccessTools.Method(typeof(GridRotationPatches), nameof(InterceptGetLongitudeRadPerGridPrefix)) }
    };

    foreach (var kvp in methodsToIntercept)
    {
      var methodToIntercept = kvp.Key;
      var replacement = kvp.Value;
      
      matcher.Start();
    
      matcher
        .MatchForward(true, new CodeMatch[] { new(ci => ci.Calls(methodToIntercept))})
        .Repeat(x => x.SetOperandAndAdvance(replacement));
    }

    return matcher.InstructionEnumeration();
  }
  
  [HarmonyTranspiler]
  [HarmonyPatch(typeof(BlueprintUtils), nameof(BlueprintUtils.GetBoundingRange))]
  static IEnumerable<CodeInstruction> InterceptSnapOutput(IEnumerable<CodeInstruction> code, ILGenerator generator)
  {
    var matcher = new CodeMatcher(code, generator);
    
    var methodsToIntercept = new Dictionary<MethodInfo, MethodInfo>
    {
        { AccessTools.Method(typeof(PlanetAuxData), nameof(PlanetAuxData.Snap)),
          AccessTools.Method(typeof(GridRotationPatches), nameof(InterceptSnap)) }
    };

    foreach (var kvp in methodsToIntercept)
    {
      var methodToIntercept = kvp.Key;
      var replacement = kvp.Value;
      
      matcher.Start();
    
      matcher
        .MatchForward(true, new CodeMatch[] { new(ci => ci.Calls(methodToIntercept))})
        .Repeat(x => x.SetOperandAndAdvance(replacement));
    }

    return matcher.InstructionEnumeration();
  }
  
  [HarmonyTranspiler]
  [HarmonyPatch(typeof(BlueprintUtils), nameof(BlueprintUtils.GenerateBlueprintData))]
  static IEnumerable<CodeInstruction> InterceptNormalizedGetterOutput(IEnumerable<CodeInstruction> code, ILGenerator generator)
  {
    var matcher = new CodeMatcher(code, generator);
    
    var methodsToIntercept = new Dictionary<MethodInfo, MethodInfo>
    {
      { AccessTools.PropertyGetter(typeof(Vector3), nameof(Vector3.normalized)),
        AccessTools.Method(typeof(GridRotationPatches), nameof(InterceptNormalizedGetter)) }
    };

    foreach (var kvp in methodsToIntercept)
    {
      var methodToIntercept = kvp.Key;
      var replacement = kvp.Value;
      
      matcher.Start();
    
      matcher
        .MatchForward(true, new CodeMatch[] { new(ci => ci.Calls(methodToIntercept))})
        .Repeat(x => x.SetOperandAndAdvance(replacement));
    }

    return matcher.InstructionEnumeration();
  }

  public static void InterceptGetMinimumGratBoxPrefix(Vector3 _npos, ref BPGratBox _gratbox)
  {
    GridRotation.PrefixPatch(ref _npos);
    BlueprintUtils.GetMinimumGratBox(_npos, ref _gratbox);
  }

  public static float InterceptGetLongitudeRadPrefix(Vector3 _npos)
  {
    GridRotation.PrefixPatch(ref _npos);
    return BlueprintUtils.GetLongitudeRad(_npos);
  }
  
  public static float InterceptGetLatitudeRadPrefix(Vector3 _npos)
  {
    GridRotation.PrefixPatch(ref _npos);
    return BlueprintUtils.GetLatitudeRad(_npos);
  }

  public static float InterceptGetLongitudeRadPerGridPrefix(Vector3 _npos, int _segmentCnt)
  {
    GridRotation.PrefixPatch(ref _npos);
    return BlueprintUtils.GetLongitudeRadPerGrid(_npos, _segmentCnt);
  }

  public static Vector3 InterceptSnap(PlanetAuxData instance, Vector3 pos, bool onTerrain)
  {
    var res = instance.Snap(pos, onTerrain);
    GridRotation.PrefixPatch(ref res);
    return res;
  }
  
  public static Vector3 InterceptNormalizedGetter(ref Vector3 instance)
  {
    var res = instance;
    GridRotation.PrefixPatch(ref res);
    return res.normalized;
  }

  [HarmonyPrefix]
  [HarmonyPatch(typeof(BPGratBox), nameof(BPGratBox.InGratBox), typeof(Vector3))]
  public static void InterceptInGratBox(ref Vector3 pos)
  {
    GridRotation.PrefixPatch(ref pos);
  }
  
  [HarmonyPrefix]
  [HarmonyPatch(typeof(BlueprintUtils), nameof(BlueprintUtils.RecalculateCursorPos))]
  [HarmonyPatch(typeof(BlueprintUtils), nameof(BlueprintUtils.RecoverOldCursorPos))]
  public static void Prefix_CursorPos(ref Vector3 _cursorPos)
  {
    GridRotation.PrefixPatch(ref _cursorPos);
  }
  
  [HarmonyPostfix]
  [HarmonyPatch(typeof(BlueprintUtils), nameof(BlueprintUtils.RecalculateCursorPos))]
  [HarmonyPatch(typeof(BlueprintUtils), nameof(BlueprintUtils.RecoverOldCursorPos))]
  public static void Postfix_CursorPos(ref Vector3 __result)
  {
    GridRotation.PostfixPatch(ref __result);
  }
}