using System.Collections.Generic;
using HarmonyLib;
using System.Reflection.Emit;
using UnityEngine;

namespace DysonSphereProgram.Modding.OffGridConstruction;

public class OffGridConstruction
{
  static void matchIgnoreGridAndCheckIfRotatable(CodeMatcher matcher, out Label? ifBlockEntryLabel, out Label? elseBlockEntryLabel)
  {
    Label? thisIfBlockEntryLabel = null;
    Label? thisElseBlockEntryLabel = null;

    matcher.MatchForward(
      false
      , new CodeMatch(ci => ci.Calls(AccessTools.PropertyGetter(typeof(VFInput), nameof(VFInput._ignoreGrid))))
      , new CodeMatch(ci => ci.Branches(out thisElseBlockEntryLabel))
      , new CodeMatch(ci => ci.IsLdarg())
      , new CodeMatch(OpCodes.Ldfld)
      , new CodeMatch(OpCodes.Ldfld)
      , new CodeMatch(ci => ci.LoadsConstant(EMinerType.Vein))
      , new CodeMatch(ci => ci.Branches(out thisIfBlockEntryLabel))
      , new CodeMatch(ci => ci.IsLdarg())
      , new CodeMatch(OpCodes.Ldfld)
      , new CodeMatch(OpCodes.Ldfld)
      , new CodeMatch(ci => ci.Branches(out _))
    );

    ifBlockEntryLabel = thisIfBlockEntryLabel;
    elseBlockEntryLabel = thisElseBlockEntryLabel;
  }

  [HarmonyTranspiler]
  [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.UpdateRaycast))]
  [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.DeterminePreviews))]
  public static IEnumerable<CodeInstruction> AllowOffGridConstruction(IEnumerable<CodeInstruction> code, ILGenerator generator)
  {
    var matcher = new CodeMatcher(code, generator);

    matchIgnoreGridAndCheckIfRotatable(matcher, out var entryLabel, out _);

    if (matcher.IsInvalid)
      return code;

    matcher.Advance(2);
    matcher.Insert(new CodeInstruction(OpCodes.Br, entryLabel.Value));

    return matcher.InstructionEnumeration();
  }

  [HarmonyTranspiler]
  [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.DeterminePreviews))]
  public static IEnumerable<CodeInstruction> PreventDraggingWhenOffGrid(IEnumerable<CodeInstruction> code, ILGenerator generator)
  {
    var matcher = new CodeMatcher(code, generator);

    Label? exitLabel = null;

    matcher.MatchForward(
      false
      , new CodeMatch(ci => ci.Branches(out exitLabel))
      , new CodeMatch(OpCodes.Ldarg_0)
      , new CodeMatch(ci => ci.LoadsConstant(1))
      , new CodeMatch(ci => ci.StoresField(AccessTools.Field(typeof(BuildTool_Click), nameof(BuildTool_Click.isDragging))))
    );

    if (matcher.IsInvalid)
      return code;

    matcher.Advance(1);
    matcher.Insert(
      new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(VFInput), nameof(VFInput._ignoreGrid)))
      , new CodeInstruction(OpCodes.Brtrue, exitLabel)
    );

    return matcher.InstructionEnumeration();
  }
  
  public static IEnumerable<CodeInstruction> PatchToPerformSteppedRotate(IEnumerable<CodeInstruction> code, ILGenerator generator)
  {
    var matcher = new CodeMatcher(code, generator);

    matchIgnoreGridAndCheckIfRotatable(matcher, out var ifBlockEntryLabel, out var elseBlockEntryLabel);

    if (matcher.IsInvalid)
      return code;

    while (!matcher.Labels.Contains(elseBlockEntryLabel.Value))
      matcher.Advance(1);

    Label? ifBlockExitLabel = null;

    matcher.MatchBack(false, new CodeMatch(ci => ci.Branches(out ifBlockExitLabel)));

    if (matcher.IsInvalid)
      return code;

    while (!matcher.Labels.Contains(ifBlockEntryLabel.Value))
      matcher.Advance(-1);

    var instructionToClone = matcher.Instruction.Clone();
    var overwriteWith = CodeInstruction.LoadField(typeof(VFInput), nameof(VFInput.control));

    matcher.SetAndAdvance(overwriteWith.opcode, overwriteWith.operand);
    matcher.Insert(instructionToClone);
    matcher.CreateLabel(out var existingEntryLabel);
    matcher.InsertAndAdvance(
      new CodeInstruction(OpCodes.Brfalse, existingEntryLabel)
      , new CodeInstruction(OpCodes.Ldarg_0)
      , CodeInstruction.Call(typeof(OffGridConstruction), nameof(OffGridConstruction.RotateStepped))
      , new CodeInstruction(OpCodes.Br, ifBlockExitLabel)
    );

    return matcher.InstructionEnumeration();
  }

  public static float steppedRotationDegrees = 15f;

  public static void RotateStepped(BuildTool_Click instance)
  {
    if (VFInput._rotate.onDown)
    {
      instance.yaw += steppedRotationDegrees;
      instance.yaw = Mathf.Repeat(instance.yaw, 360f);
      instance.yaw = Mathf.Round(instance.yaw / steppedRotationDegrees) * steppedRotationDegrees;
    }

    if (VFInput._counterRotate.onDown)
    {
      instance.yaw -= steppedRotationDegrees;
      instance.yaw = Mathf.Repeat(instance.yaw, 360f);
      instance.yaw = Mathf.Round(instance.yaw / steppedRotationDegrees) * steppedRotationDegrees;
    }
  }
}
